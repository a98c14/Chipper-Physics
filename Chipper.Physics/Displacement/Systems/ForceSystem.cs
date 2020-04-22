using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(CollisionResolutionSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class ForceSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps) 
        {
            return Entities.WithName("ForceSystem")
            .ForEach((ref Force force, ref Acceleration accel, in Mass mass) =>
            {
                accel.Value += force.Value / mass.Value;
                force.Value = 0;
            })
            .Schedule(inputDeps);
        }
    }
}
