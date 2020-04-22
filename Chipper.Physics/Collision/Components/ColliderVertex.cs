using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    [InternalBufferCapacity(3)]
    public struct ColliderVertex : IBufferElementData
    {
        public float2 Value;

        public static implicit operator ColliderVertex(float2 v) => new ColliderVertex { Value = v };
        public static implicit operator float2(ColliderVertex v) => v.Value;
    }
}
