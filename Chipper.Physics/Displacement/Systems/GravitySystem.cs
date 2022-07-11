using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Chipper.Transforms;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(ForceSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class GravitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var settings = HasSingleton<PhysicsSettings>() ? GetSingleton<PhysicsSettings>() : PhysicsSettings.Default;
            Entities
            .WithName("GravitySystem")
            .WithAll<AffectedByGravity>()
            .ForEach((ref Force force, in Mass mass, in Position2D pos) =>
            {
                if (mass.IsDisabled)
                    return;

                var z = pos.Value.z;
                if (z > 0)
                    force.Value += new float3(0, 0, -1) * settings.Gravity * mass.Value;
            }).Schedule();
        }
    }
}
