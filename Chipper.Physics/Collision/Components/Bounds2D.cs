using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Bounds2D : IComponentData
    {
        public float2 Min;
        public float2 Max;

        public float2 Center  => Min + Extents;
        public float2 Extents => (Max - Min) / 2f;

        // Vertices
        public float2 BL => Min;
        public float2 TR => Max;
        public float2 BR => new float2(Max.x, Min.y);
        public float2 TL => new float2(Min.x, Max.y);

        // Normals
        public float2 N0 => new float2(1, 0);
        public float2 N1 => new float2(0, 1);

        public float2 ClosestPoint(float2 v)
        {
            return math.clamp(v, Min, Max);
        }

        public Bounds2D(float2 min, float2 max)
        {
            Min = min;
            Max = max;
        }

        public Bounds2D(NativeArray<ColliderVertex> vertices)
        {
            var v0 = vertices[0].Value;
            var minX = v0.x;
            var minY = v0.y;
            var maxX = v0.x;
            var maxY = v0.y;

            for (int i = 1; i < vertices.Length; i++)
            {
                var v = vertices[i].Value;
                minX = math.min(v.x, minX);
                minY = math.min(v.y, minY);
                maxX = math.max(v.x, maxX);
                maxY = math.max(v.y, maxY);
            }

            Min = new float2(minX, minY);
            Max = new float2(maxX, maxY);
        }

        public Bounds2D(CircleCollider c)
        {
            Min = new float2(c.Center.x - c.Radius, c.Center.y - c.Radius);
            Max = new float2(c.Center.x + c.Radius, c.Center.y + c.Radius);
        }

        public bool IsBiggerThanCellSize => (Max.x - Min.x > Constants.CellSize) || (Max.y - Min.y > Constants.CellSize);

        public static bool DoesCollide(Bounds2D a, Bounds2D b)
        {
            return a.Min.x < b.Max.x &&
                   a.Max.x > b.Min.x &&
                   a.Max.y > b.Min.y &&
                   a.Min.y < b.Max.y;
        }
    }
}
