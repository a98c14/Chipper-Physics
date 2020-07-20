using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Chipper.Transforms;
using Unity.Mathematics;
using System.Data;

namespace Chipper.Physics
{
    public class AccelerationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            Entities
            .WithName("AccelerationSystem")
            .WithBurst()
            .ForEach((ref Velocity velocity, in Acceleration acceleration, in Position2D position) =>
            {
                var z           = position.Value.z;
                var za          = acceleration.Value.z;
                var m           = math.select(1f, -.8f, z <= 0);
                m               = math.select(m, 0, math.abs(za) <= Constants.MinZAccel && math.abs(z) <= 0);
                velocity.Value *= new float3(1, 1, m);
                velocity.Value += acceleration.Value * dt;
            }).Schedule();
        }
    }
}
