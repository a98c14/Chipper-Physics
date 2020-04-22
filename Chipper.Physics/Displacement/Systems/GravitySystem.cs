using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(ForceSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class GravitySystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var settings = HasSingleton<PhysicsSettings>() ? GetSingleton<PhysicsSettings>() : PhysicsSettings.Default;
            inputDeps = Entities
            .WithName("GravitySystem")
            .WithAll<AffectedByGravity>()
            .ForEach((ref Acceleration acceleration, in Mass mass) =>
            {
                if (mass.IsDisabled)
                    return;

                acceleration.Value += new float3(0, 0, -1) * settings.Gravity / mass.Value;
            }).Schedule(inputDeps);

            return inputDeps;
        }
    }
}
