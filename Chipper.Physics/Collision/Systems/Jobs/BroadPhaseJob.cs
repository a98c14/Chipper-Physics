using Unity.Burst;
using Unity.Collections;

namespace Chipper.Physics
{
    [BurstCompile]
    struct BroadPhaseJob : IJobNativeMultiHashMapVisitKeyValue<int, ColliderData>
    {
        [ReadOnly] public NativeMultiHashMap<int, ColliderData> TargetMap;
        [WriteOnly] public NativeMultiHashMap<int, PossibleCollision>.ParallelWriter PossibleCollisions;

        public void ExecuteNext(int key, ColliderData source)
        {
            var hasValue = TargetMap.TryGetFirstValue(key, out var target, out var iterator);
            while (hasValue)
            {
                // TODO: We need to check collider heights aswell
                if (CanCollide(source.Tags, target.Tags) && source.Entity != target.Entity && Bounds2D.DoesCollide(source.Bounds, target.Bounds)) 
                {
                    PossibleCollisions.Add(key, new PossibleCollision
                    {
                        Source = source.Entity,
                        SourceType = source.Shape,
                        Target = target.Entity,
                        TargetType = target.Shape,
                        SourceCenter = source.Bounds.Center,
                        TargetCenter = target.Bounds.Center,
                    });
                }

                hasValue = TargetMap.TryGetNextValue(out target, ref iterator);
            }
        }

        bool CanCollide(ColliderTagType source, ColliderTagType target) => (source & target) != 0;
    }
}
