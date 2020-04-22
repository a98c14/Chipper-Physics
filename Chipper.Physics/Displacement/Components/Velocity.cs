using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Velocity : IComponentData
    {
        public float3 Value;

        public bool IsZeroApprox => math.lengthsq(Value) <= .5f;
    }
}
