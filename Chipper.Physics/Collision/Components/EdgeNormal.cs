using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    [InternalBufferCapacity(2)]
    public struct EdgeNormal : IBufferElementData
    {
        public float2 Value;
    }
}
