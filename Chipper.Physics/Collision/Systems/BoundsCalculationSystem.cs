using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(ColliderTranslationSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class BoundsCalculationSystem : SystemBase
    {
        [BurstCompile]
        partial struct BoundsJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<CircleCollider> CircleColliderType;
            [ReadOnly] public BufferTypeHandle<ColliderVertex> VertexType;

            public ComponentTypeHandle<Bounds2D> BoundsType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var count = chunk.Count;
                if (chunk.Has(CircleColliderType))
                {
                    var colliders = chunk.GetNativeArray(CircleColliderType);
                    var aabbs = chunk.GetNativeArray(BoundsType);
                    for(int i = 0; i < count; i++)
                        aabbs[i] = new Bounds2D(colliders[i]);
                }
                else if (chunk.Has(VertexType))
                {
                    var colliders = chunk.GetBufferAccessor(VertexType);
                    var aabbs = chunk.GetNativeArray(BoundsType);
                    for (int i = 0; i < count; i++)
                        aabbs[i] = new Bounds2D(colliders[i].AsNativeArray());
                }
            }
        }

        EntityQuery m_Query;

        protected override void OnUpdate()
        {
            new BoundsJob
            {
                CircleColliderType = GetComponentTypeHandle<CircleCollider>(true),
                VertexType = GetBufferTypeHandle<ColliderVertex>(true),
                BoundsType = GetComponentTypeHandle<Bounds2D>(false),
            }.Schedule(m_Query);
        }

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite<Bounds2D>(),
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly<ColliderVertex>(),
                    ComponentType.ReadOnly<CircleCollider>(),
                },
            });
        }
    }
}
