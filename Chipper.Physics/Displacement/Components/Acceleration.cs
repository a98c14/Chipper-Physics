using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Acceleration : IComponentData
    {
        public static Acceleration Zero => new Acceleration { Value = new float3(0, 0, 0) };

        public float3 Value;
    }
}
