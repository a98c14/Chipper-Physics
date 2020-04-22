using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Pivot : IComponentData
    {
        public float3 Value;
    }
}
