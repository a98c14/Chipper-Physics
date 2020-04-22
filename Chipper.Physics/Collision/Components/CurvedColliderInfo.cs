using Unity.Entities;
using Unity.Mathematics;

public struct CurvedColliderInfo : IComponentData
{
    public float2 Offset; 
    public float  Height;
    public float  Angle;
    public float  Radius;
}
