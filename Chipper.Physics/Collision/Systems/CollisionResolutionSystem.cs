using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Chipper.Transforms;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(CollisionSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class CollisionResolutionSystem : SystemBase
    {
        [BurstCompile]
        partial struct TranslationJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<MoveOutOfCollision> MoveOutOfCollisionType;
            [ReadOnly] public ComponentTypeHandle<ForceOutOfCollision> ForceOutOfCollisionType;
            [ReadOnly] public BufferTypeHandle<CollisionBuffer> CollisionBufferType;

            public ComponentTypeHandle<Force> ForceType;
            public ComponentTypeHandle<Position2D> PositionType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var count = chunk.Count;
                var shouldForce = chunk.Has(ForceOutOfCollisionType) && chunk.Has(ForceType);
                var shouldMove = chunk.Has(MoveOutOfCollisionType);

                if (shouldMove)
                {
                    var positions = chunk.GetNativeArray(PositionType);
                    var collisionBuffers = chunk.GetBufferAccessor(CollisionBufferType);
                    for (int i = 0; i < count; i++)
                    {
                        var position = positions[i];
                        var collisions = collisionBuffers[i];
                        var mtv = GetCollisionTranslationSum(collisions.AsNativeArray());
                        position.Value += new float3(mtv, 0);
                        positions[i] = position;
                    }
                }

                if (shouldForce)
                {
                    var forces = chunk.GetNativeArray(ForceType);
                    var collisionBuffers = chunk.GetBufferAccessor(CollisionBufferType);

                    for(int i = 0; i < count; i++)
                    {
                        var force = forces[i];
                        var collisions = collisionBuffers[i];
                        var mtv = GetCollisionTranslationSum(collisions.AsNativeArray());
                        force.Value -= new float3(mtv, 0);
                        forces[i] = force;
                    }
                }
            }

            float2 GetCollisionTranslationSum(NativeArray<CollisionBuffer> collisions)
            {
                var sum = float2.zero;
                for (int i = 0; i < collisions.Length; i++)
                    sum += GetTranslationVector(collisions[i]);
                return sum;
            }

            float2 GetTranslationVector(CollisionBuffer collision) => collision.Depth * collision.Normal;
        }

        EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(CollisionBuffer)),
                    typeof(Position2D),
                },
                Any = new ComponentType[]
                {
                    typeof(ForceOutOfCollision),
                    typeof(MoveOutOfCollision),
                }
            });
        }

        protected override void OnUpdate()
        {
            new TranslationJob
            {                                
                CollisionBufferType = GetBufferTypeHandle<CollisionBuffer>(true),
                ForceOutOfCollisionType = GetComponentTypeHandle<ForceOutOfCollision>(true),
                MoveOutOfCollisionType = GetComponentTypeHandle<MoveOutOfCollision>(true),
                PositionType = GetComponentTypeHandle<Position2D>(false),
                ForceType = GetComponentTypeHandle<Force>(false),
            }.Schedule(m_Query);
        }

    }
}
