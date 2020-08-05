using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Chipper.Transforms;

namespace Chipper.Physics
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class ColliderTranslationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct PivotTranslationJob : IJobForEach<Position2D, PivotOffset, Pivot>
        {
            public void Execute([ReadOnly] ref Position2D position, [ReadOnly] ref PivotOffset offset, ref Pivot pivot)
            {
                pivot.Value = position.Value + offset.Value;
            }
        }

        [BurstCompile]
        struct TranslationJob : IJobChunk
        {
            [ReadOnly] public uint LastSystemVersion;
            [ReadOnly] public ComponentTypeHandle<Pivot> PivotType;
            [ReadOnly] public ComponentTypeHandle<PolygonColliderInfo> PolygonInfoType;
            [ReadOnly] public ComponentTypeHandle<CurvedColliderInfo> CurvedInfoType;
            [ReadOnly] public ComponentTypeHandle<Rotation2D> RotationType;

            public BufferTypeHandle<ColliderVertex> VertexType;
            public BufferTypeHandle<EdgeNormal> NormalType;
            public ComponentTypeHandle<CircleCollider> CircleType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var count = chunk.Count;
                var pivots = chunk.GetNativeArray(PivotType);

                // TODO: Instead of finding position/rotation of every vertex every frame we could 
                // move them using their previous position. So we could only manipulate values
                // that change.
                if (chunk.Has(PolygonInfoType) /*&& chunk.Has(RotationType) && chunk.DidChange(RotationType, LastSystemVersion)*/)
                {
                    var vBuffers = chunk.GetBufferAccessor(VertexType);
                    var nBuffers = chunk.GetBufferAccessor(NormalType);
                    var rotations = chunk.GetNativeArray(RotationType);
                    for (int i = 0; i < count; i++)
                    {
                        var vertices = vBuffers[i].AsNativeArray();
                        var normals = nBuffers[i].AsNativeArray();

                        var rotation = MathUtil.GetRotationMatrix(rotations[i].Value);
                        var pivot = pivots[i].Value;
                        var infos = chunk.GetNativeArray(PolygonInfoType);
                        ref var info = ref infos[i].VertexOffsets.Value;
                        
                        for(int j = 0; j < vertices.Length; j++)
                            vertices[j] = math.mul(rotation, info[j].xy);

                        for (int j = 0; j < vertices.Length; j++)
                            vertices[j] += pivot.xy;

                        for (int j = 0; j < normals.Length; j++)
                        {
                            var v0 = vertices[j].Value;
                            var v1 = vertices[(j + 1) % normals.Length].Value;
                            var n = math.normalize(v0 - v1);
                            n = new float2(-n.y, n.x);
                            normals[j] = new EdgeNormal { Value = n };
                        }
                    }
                }
                //else if (chunk.Has(PolygonInfoType))
                //{
                //    var vertexBuffers = chunk.GetBufferAccessor(VertexType);
                //    var infos = chunk.GetNativeArray(PolygonInfoType);
                //    for(int i = 0; i < count; i++)
                //    {
                //        var pivot = pivots[i].Value;
                //        var vertices = vertexBuffers[i].AsNativeArray();
                //        ref var info = ref infos[i].VertexOffsets.Value;

                //        for(int j = 0; j < info.Length; j++)
                //            vertices[i] = info[j].xy + pivot.xy;
                //    }
                //}
                else if (chunk.Has(CurvedInfoType))
                {
                    var colliders = chunk.GetNativeArray(CircleType);
                    var infos = chunk.GetNativeArray(CurvedInfoType);
                    for(int i = 0; i < count; i++)
                    {
                        var pivot = pivots[i].Value;
                        var info = infos[i];

                        colliders[i] = new CircleCollider
                        {
                            Center = pivot.xy,
                            Radius = info.Radius,
                            RadiusSq = info.Radius * info.Radius,
                        };
                    }
                }
                // TODO: Add part circle (2D Cone) collider. It will have a radius, angle and rotation
            }
        }
    
        EntityQuery m_ColliderGroup;
        EntityQuery m_PivotGroup;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var pivotJob = new PivotTranslationJob().Schedule(m_PivotGroup, inputDeps);
            return new TranslationJob
            {
                LastSystemVersion = LastSystemVersion,
                VertexType = GetBufferTypeHandle<ColliderVertex>(false),
                NormalType = GetBufferTypeHandle<EdgeNormal>(false),
                CircleType = GetComponentTypeHandle<CircleCollider>(false),
                PivotType = GetComponentTypeHandle<Pivot>(true),
                PolygonInfoType = GetComponentTypeHandle<PolygonColliderInfo>(true),
                CurvedInfoType = GetComponentTypeHandle<CurvedColliderInfo>(true),
                RotationType = GetComponentTypeHandle<Rotation2D>(true),
            }.Schedule(m_ColliderGroup, pivotJob);
        }

        protected override void OnCreate()
        {
            m_PivotGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(Position2D)),
                    ComponentType.ReadOnly(typeof(PivotOffset)),
                    typeof(Pivot),
                }
            });

            m_ColliderGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] 
                { 
                    ComponentType.ReadOnly(typeof(Pivot)),
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(PolygonColliderInfo)),
                    ComponentType.ReadOnly(typeof(CurvedColliderInfo)),
                },
            });
        }
    }
}
