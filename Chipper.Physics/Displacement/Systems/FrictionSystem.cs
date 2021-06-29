using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(VelocitySystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class FrictionSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var settings = HasSingleton<PhysicsSettings>() ? GetSingleton<PhysicsSettings>() : PhysicsSettings.Default;
            var dt = Time.DeltaTime;

            inputDeps = Entities.WithName("FrictionSystem")
            .ForEach((ref Force force, ref Velocity velocity, ref Acceleration acceleration, in Mass mass) =>
            {
                //var frictionDirection = math.normalizesafe(velocity.Value) * -1;
                //force.Value += mass.Value * settings.Gravity * settings.FrictionCoefficient * dt * frictionDirection;
                acceleration.Value = math.lerp(acceleration.Value, 0, dt * 16f);
                velocity.Value = math.lerp(velocity.Value, 0, dt * 6f);
            })
            .Schedule(inputDeps);

            return inputDeps;
        }
    }
}
