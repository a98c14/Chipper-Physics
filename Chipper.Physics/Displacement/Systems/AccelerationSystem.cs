using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Chipper.Physics
{
    public class AccelerationSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var dt = Time.DeltaTime;
            return Entities.WithName("AccelerationSystem")
                .ForEach((ref Velocity velocity, in Acceleration acceleration) =>
                {
                    velocity.Value += acceleration.Value * dt;
                }).Schedule(inputDeps);
        }
    }
}
