using Unity.Entities;
using Unity.Mathematics;

public struct PolygonColliderInfo : IComponentData
{
    public float Height;
    public BlobAssetReference<BlobArray<float2>> VertexOffsets;
}
