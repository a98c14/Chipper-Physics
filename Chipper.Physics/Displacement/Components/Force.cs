using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Force : IComponentData
    {
        public float3 Value;

        public Force(float3 v) => Value = v;
    }
}
