using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Chipper.Physics;
using Chipper.Transforms;
using Unity.Mathematics;

public struct ColliderData
{
    public Entity Entity;
    public Entity ColliderEntity;
    public Bounds2D Bounds;
    public ColliderShapeType Shape;
    public ColliderTagType Tags;
}

public struct LargeColliderData
{
    public Entity Entity;
    public Bounds2D Bounds;
    public ColliderShapeType Shape;
    public ColliderTagType SourceTags;
    public ColliderTagType TargetTags;
}

namespace Chipper.Physics
{
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(BoundsCalculationSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class SpatialPartitionSystem : SystemBase
    {
        public NativeParallelMultiHashMap<int, ColliderData> ColliderMap;
        public NativeParallelMultiHashMap<int, ColliderData> TargetMap;

        public NativeQueue<LargeColliderData> LargeColliders;
        public JobHandle PartitionJobHandle;
    
        EntityQuery m_ColliderGroup;

        protected override void OnUpdate()
        {
            ClearMapsIfCreated();

            if (!m_ColliderGroup.IsEmptyIgnoreFilter)
            {
                new PartitionJob
                {
                    EntityType        = GetEntityTypeHandle(),
                    BoundsType        = GetComponentTypeHandle<Bounds2D>(true),
                    ShapeType         = GetComponentTypeHandle<ColliderShape>(true),
                    LargeColliderType = GetComponentTypeHandle<LargeCollider>(true),
                    ParentType        = GetComponentTypeHandle<Parent2D>(true),
                    TagType           = GetComponentTypeHandle<ColliderTag>(true),
                    ColliderTargetMap = TargetMap.AsParallelWriter(),
                    ColliderSourceMap = ColliderMap.AsParallelWriter(),
                    LargeColliders    = LargeColliders.AsParallelWriter(),                
                }.Schedule(m_ColliderGroup);
            }

            // Save the final handle for other systems to use
            PartitionJobHandle = Dependency;
        }

        protected override void OnCreate()
        {
            AllocateMaps(100);
            m_ColliderGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(Bounds2D)),
                    ComponentType.ReadOnly(typeof(ColliderShape)),
                    ComponentType.ReadOnly(typeof(ColliderTag)),
                },
            });
        }

        protected override void OnStopRunning()
        {
            PartitionJobHandle.Complete();
            DisposeMapsIfCreated();
        }

        bool HasCreatedMaps => ColliderMap.IsCreated || TargetMap.IsCreated || LargeColliders.IsCreated;

        void AllocateMaps(int capacity)
        {
            ColliderMap    = new NativeParallelMultiHashMap<int, ColliderData>(capacity, Allocator.Persistent);
            TargetMap      = new NativeParallelMultiHashMap<int, ColliderData>(capacity * 10, Allocator.Persistent);
            LargeColliders = new NativeQueue<LargeColliderData>(Allocator.Persistent);
        }

        // NOTE: If `NativeMultiHashMap.Clear()` is too slow we can try
        // using `NativeHashMap` + entities with dynamic buffers instead.
        void ClearMapsIfCreated()
        {
            if (HasCreatedMaps)
            {
                ColliderMap.Clear();
                TargetMap.Clear();
                LargeColliders.Clear();
            }
        }

        void DisposeMapsIfCreated()
        {
            if (HasCreatedMaps)
            {
                ColliderMap.Dispose();
                TargetMap.Dispose();
                LargeColliders.Dispose();
            }
        }
    }
}
