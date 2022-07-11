using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Chipper.Physics
{
    [UpdateBefore(typeof(SpatialPartitionSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class CollisionBufferCleaner : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithName("CollisionBufferCleaner")
                .ForEach((DynamicBuffer<CollisionBuffer> buffer) => buffer.Clear())
                .Schedule();
        }
    }
}
