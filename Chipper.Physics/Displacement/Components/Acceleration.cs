using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Acceleration : IComponentData
    {
        public float3 Value;
    }
}
