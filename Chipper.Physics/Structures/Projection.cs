using Unity.Collections;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct Projection
    {
        public float Min;
        public float Max;

        public Projection(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public Projection(float2 v, NativeArray<ColliderVertex> vertices)
        {
            var min = float.MaxValue;
            var max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i].Value;
                var d = math.dot(vertex, v);
                min = math.min(d, min);
                max = math.max(d, max);
            }

            Min = min;
            Max = max;
        }

        public Projection(float2 v, CircleCollider c)
        {
            var d = math.dot(c.Center, v);
            Min = d - c.Radius;
            Max = d + c.Radius;
        }

        public Projection(float2 v, Bounds2D b)
        {
            var d0 = math.dot(b.BL, v);
            var d1 = math.dot(b.TL, v);
            var d2 = math.dot(b.TR, v);
            var d3 = math.dot(b.BR, v);

            Min = math.min(math.min(d0, d1), math.min(d2, d3));
            Max = math.max(math.max(d0, d1), math.max(d2, d3));
        }

        public static bool DoesOverlap(Projection p0, Projection p1)
        {
            return !(p0.Min > p1.Max || p1.Min > p0.Max);
        }

        public static bool DoesOverlap(Projection p, float v)
        {
            return v > p.Min && v < p.Max;
        }

        public static float GetOverlapAmount(Projection p0, Projection p1)
        {
            return math.min(p0.Max - p1.Min, p1.Max - p0.Min);
        }
    }
}