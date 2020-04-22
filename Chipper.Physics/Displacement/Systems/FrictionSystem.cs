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
                // force.Value += mass.Value * settings.Gravity * settings.FrictionCoefficient * math.normalizesafe(velocity.Value) * -1 * dt;
                acceleration.Value = math.lerp(acceleration.Value, 0, dt * 4f);
                velocity.Value = math.lerp(velocity.Value, 0, dt * 1f);
            })
            .Schedule(inputDeps);

            return inputDeps;
        }
    }
}
