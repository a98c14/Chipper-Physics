using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Chipper.Physics
{
    [UpdateBefore(typeof(SpatialPartitionSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class CollisionBufferCleaner : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return Entities.WithName("CollisionBufferCleaner")
                .ForEach((DynamicBuffer<CollisionBuffer> buffer) => buffer.Clear())
                .Schedule(inputDeps);
        }
    }
}
