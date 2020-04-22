using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using System.Runtime.InteropServices;

namespace Chipper.Physics
{
    public struct PhysicsUtil 
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct IntPacker
        {
            [FieldOffset(0)]
            public int LowInt;

            [FieldOffset(4)]
            public int HighInt;

            [FieldOffset(0)]
            public long LongValue;

            public void Clear()
            {
                LowInt = 0;
                HighInt = 0;
            }
        }

        public static float2 ClosestPoint(float2 v, NativeArray<ColliderVertex> vertices)
        {
            var closest = v;
            var minDistSq = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i].Value;
                var distSq = math.distancesq(v, vertex);
                if(distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = vertex;
                }
            }
            return closest;
        }

        public static float2 ClosestPoint(float2 v, Bounds2D aabb)
        {
            return math.clamp(v, aabb.Min, aabb.Max);
        }

        public static long GetCollisionID(Entity source, Entity target)
        {
            var packer = new IntPacker
            {
                LowInt = source.Index,
                HighInt = target.Index
            };
            return packer.LongValue;
        }

        public static float2 Project(float2 a, float2 b)
        {
            var d = math.dot(a, b);
            var lensq = math.dot(b,b);
            return new float2((d / lensq) * b.x, (d / lensq) * b.y);
        }

        public static bool IsPointInsideCircle(float2 point, CircleCollider collider)
        {
            var distSq = math.lengthsq(collider.Center - point);
            return distSq <= collider.RadiusSq;
        }

        /// <summary>
        /// Returns the grid offsets in respect to position -> int4(minX, minY, maxX, maxY)
        /// </summary>
        public static int4 GetSearchOffsets(float2 position, float radius, int cellSize)
        {
            var center = HashUtil.Quantize(position, cellSize);
            var min = HashUtil.Quantize(position - radius, cellSize);
            var max = HashUtil.Quantize(position + radius, cellSize);

            var minX = min.x - center.x - 1;
            var minY = min.y - center.y - 1;
            var maxX = max.x - center.x + 1;
            var maxY = max.y - center.y + 1;

            return new int4(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Returns the grid offsets in respect to position -> int4(minX, minY, maxX, maxY)
        /// </summary>
        public static int4 GetGridBounds(Bounds2D bounds, int cellSize)
        {
            var min = HashUtil.Quantize(bounds.Min, cellSize);
            var max = HashUtil.Quantize(bounds.Max, cellSize);

            var minX = min.x - 1;
            var minY = min.y - 1;
            var maxX = max.x + 1;
            var maxY = max.y + 1;

            return new int4(minX, minY, maxX, maxY);
        }
    }
}
