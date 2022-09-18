using Unity.Entities;   
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Chipper.Transforms;

namespace Chipper.Physics
{
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SpatialPartitionSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class CollisionSystem : SystemBase
    {
        public JobHandle CollisionJobHandle;

        bool                      m_MapSwitch;
        NativeParallelHashMap<long, byte> m_FrameCollisions0;
        NativeParallelHashMap<long, byte> m_FrameCollisions1;
        NativeArray<float2>       m_CellOffsets;
        SpatialPartitionSystem    m_PartitionSystem;
        NativeParallelMultiHashMap<int, PossibleCollision> m_PossibleCollisions;

        protected override void OnUpdate()
        {
            
            // Every frame we switch the previous frame's currentCollisions to this frame's
            // previous collisions. Then we clear the previous frame's previousCollisions and
            // make it this frame's fresh currentCollisions table.
            var currentCollisions  = m_MapSwitch ? m_FrameCollisions0 : m_FrameCollisions1;
            var previousCollisions = m_MapSwitch ? m_FrameCollisions1 : m_FrameCollisions0;
            m_MapSwitch = !m_MapSwitch;
            currentCollisions.Clear();
            m_PossibleCollisions.Clear();
            
            // Despite having UpdateAfter(SpatialPartitionSystem) as attribute we still need to 
            // combine the dependencies between them to ensure public native collection is ready
            // UpdateAfter attribute only handles the IComponentData dependencies and not public native
            // collections.
            var partitionHandle = JobHandle.CombineDependencies(Dependency, m_PartitionSystem.PartitionJobHandle);
            var broadPhase = new BroadPhaseJob
            {
                PossibleCollisions = m_PossibleCollisions.AsParallelWriter(),
                TargetMap = m_PartitionSystem.TargetMap,
            }.Schedule(m_PartitionSystem.ColliderMap, 1, partitionHandle);

            var narrowPhase = new NarrowPhaseJob
            {
                CurrentFrameCollisions    = currentCollisions.AsParallelWriter(),
                PreviousFrameCollisions   = previousCollisions,
                ColliderNormalFromEntity  = GetBufferFromEntity<EdgeNormal>(true),
                ColliderVertexFromEntity  = GetBufferFromEntity<ColliderVertex>(true),
                Positions                 = GetComponentDataFromEntity<Position2D>(true),
                CircleColliderFromEntity  = GetComponentDataFromEntity<CircleCollider>(true),
                CollisionBufferFromEntity = GetBufferFromEntity<CollisionBuffer>(false),
            }.Schedule(m_PossibleCollisions, 1, broadPhase);

            CollisionJobHandle = narrowPhase;
        }
    
        protected override void OnCreate()
        {            
            m_MapSwitch = false;
            m_PossibleCollisions = new NativeParallelMultiHashMap<int, PossibleCollision>(100, Allocator.Persistent);
            m_FrameCollisions0 = new NativeParallelHashMap<long, byte>(1000, Allocator.Persistent);
            m_FrameCollisions1 = new NativeParallelHashMap<long, byte>(1000, Allocator.Persistent);
            m_PartitionSystem = World.GetOrCreateSystem<SpatialPartitionSystem>();
            Enabled = false;
        }

        protected override void OnDestroy()
        {
            if (m_PossibleCollisions.IsCreated) m_PossibleCollisions.Dispose();
            if (m_FrameCollisions0.IsCreated) m_FrameCollisions0.Dispose();
            if (m_FrameCollisions1.IsCreated) m_FrameCollisions1.Dispose();
            if (m_CellOffsets.IsCreated) m_CellOffsets.Dispose();
        }
    }
}
