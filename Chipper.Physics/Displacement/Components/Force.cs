using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Force : IComponentData
    {
        public static Force Zero => new Force(0, 0, 0);
        public float3 Value;

        public Force(float3 v) => Value = v;
        public Force(float x, float y, float z) => Value = new float3(x, y, z);
    }
}
