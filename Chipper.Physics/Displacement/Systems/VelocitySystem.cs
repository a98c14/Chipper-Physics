using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Chipper.Transforms;
using Unity.Mathematics;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(ForceSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class VelocitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            Entities.WithName("VelocitySystem")
            .ForEach((ref Position2D position, in Velocity velocity) =>
            {
                var p = position.Value;
                p += velocity.Value * dt;
                var z = p.z;
                z = math.max(0, z);
                p = new float3(p.x, p.y, z);
                position.Value = p;
            }).Schedule();
        }
    }
}
