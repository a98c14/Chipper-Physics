using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Velocity : IComponentData
    {
        public static Velocity Zero => new Velocity { Value = new float3(0, 0, 0)};

        public float3 Value;

        public bool IsZeroApprox => math.lengthsq(Value) <= .5f;
    }
}
