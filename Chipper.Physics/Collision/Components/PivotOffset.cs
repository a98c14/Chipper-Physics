using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct PivotOffset : IComponentData
    {
        public float3 Value;

        public PivotOffset(float3 v)
        {
            Value = v;
        }
    }
}