using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Chipper.Physics
{
    [UpdateAfter(typeof(CollisionResolutionSystem))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class ForceSystem : SystemBase
    {
        protected override void OnUpdate() 
        {
            Entities.WithName("ForceSystem")
            .ForEach((ref Force force, ref Acceleration accel, in Mass mass) =>
            {
                accel.Value += force.Value / mass.Value;
                force.Value = 0;
            })
            .Schedule();
        }
    }
}
