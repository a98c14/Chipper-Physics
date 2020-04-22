using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Chipper.Transforms;

namespace Chipper.Physics
{
    [BurstCompile]
    struct NarrowPhaseJob : IJobNativeMultiHashMapVisitKeyValue<int, PossibleCollision>
    {
        [ReadOnly] public NativeHashMap<long, byte> PreviousFrameCollisions;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public BufferFromEntity<CollisionBuffer> CollisionBufferFromEntity;
        [WriteOnly] public NativeHashMap<long, byte>.ParallelWriter CurrentFrameCollisions;

        // Component Accessors
        [ReadOnly] public BufferFromEntity<ColliderVertex> ColliderVertexFromEntity;
        [ReadOnly] public BufferFromEntity<EdgeNormal> ColliderNormalFromEntity;
        [ReadOnly] public ComponentDataFromEntity<CircleCollider> CircleColliderFromEntity;
        [ReadOnly] public ComponentDataFromEntity<Position2D> Positions;

        public void ExecuteNext(int key, PossibleCollision p)
        {
            if(DoesCollide(p, out var mtv))
                ApplyCollision(p.CollisionID, p.Source, p.Target, mtv);
        }

        bool DoesCollide(PossibleCollision p, out MTV mtv)
        {
            var doesCollide = false;
            mtv = MTV.Max;
            if (p.SourceType == ColliderShapeType.Circle && p.TargetType == ColliderShapeType.Circle)
            {
                var c0 = CircleColliderFromEntity[p.Source];
                var c1 = CircleColliderFromEntity[p.Target];
                doesCollide = CollisionUtils.IsColliding(c0, c1, out mtv);
            }
            else if (p.SourceType == ColliderShapeType.Polygon && p.TargetType == ColliderShapeType.Circle)
            {
                var v = ColliderVertexFromEntity[p.Source].AsNativeArray();
                var n = ColliderNormalFromEntity[p.Source].AsNativeArray();
                var c = CircleColliderFromEntity[p.Target];
                doesCollide = CollisionUtils.IsColliding(c, v, n, p.SourceCenter, out mtv);
            }
            else if (p.SourceType == ColliderShapeType.Circle && p.TargetType == ColliderShapeType.Polygon)
            {
                var c = CircleColliderFromEntity[p.Source];
                var v = ColliderVertexFromEntity[p.Target].AsNativeArray();
                var n = ColliderNormalFromEntity[p.Target].AsNativeArray();
                doesCollide = CollisionUtils.IsColliding(c, v, n, p.TargetCenter, out mtv);
            }
            else if (p.SourceType == ColliderShapeType.Polygon && p.TargetType == ColliderShapeType.Polygon)
            {
                var v0 = ColliderVertexFromEntity[p.Source].AsNativeArray();
                var n0 = ColliderNormalFromEntity[p.Source].AsNativeArray();
                var v1 = ColliderVertexFromEntity[p.Target].AsNativeArray();
                var n1 = ColliderNormalFromEntity[p.Target].AsNativeArray();
                doesCollide = CollisionUtils.IsColliding(v0, v1, n0, n1, p.SourceCenter, p.TargetCenter, out mtv);
            }

            return doesCollide;
        }

        void ApplyCollision(long id, Entity source, Entity target, MTV mtv)
        {
            var collisionBuffer = CollisionBufferFromEntity[source];
            var sourcePos = Positions[source].Value;
            var targetPos = Positions[target].Value;
            collisionBuffer.Add(new CollisionBuffer
            {
                Entity = target,
                IsNew = !DoesExistInPreviousFrame(id),
                Point = (sourcePos + targetPos / 2),
                Normal = mtv.Direction,
                Depth = mtv.Magnitude,
            });
            CurrentFrameCollisions.TryAdd(id, 1);
        }

        bool DoesExistInPreviousFrame(long collisionID)
        {
            return PreviousFrameCollisions.ContainsKey(collisionID);
        }
    }
}
