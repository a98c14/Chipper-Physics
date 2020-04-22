using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Chipper.Transforms;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(ForceSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class VelocitySystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var dt = Time.DeltaTime;
            return Entities.WithName("VelocitySystem")
                .ForEach((ref Position2D position, in Velocity velocity) =>
                {
                    position.Value += velocity.Value * dt;
                }).Schedule(inputDeps);
        }
    }
}
