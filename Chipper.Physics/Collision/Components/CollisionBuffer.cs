using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    [InternalBufferCapacity(4)]
    public struct CollisionBuffer : IBufferElementData
    {
        public bool   IsNew; // True if collision happened this frame and not previous frame
        public Entity Entity;
        public float3 Point;
        public float2 Normal;
        public float  Depth;
    }
}
