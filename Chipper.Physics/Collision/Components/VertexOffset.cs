using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    [InternalBufferCapacity(3)]
    public struct VertexOffset : IBufferElementData
    {
        public float2 Value;
    }
}