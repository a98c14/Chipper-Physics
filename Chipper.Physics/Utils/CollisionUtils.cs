using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using static Chipper.Physics.PhysicsUtil;

namespace Chipper.Physics
{
    public struct CollisionUtils
    {
        public static bool IsColliding(CircleCollider c0, CircleCollider c1)
        {
            var n = math.normalizesafe(c0.Center - c1.Center);
            var p0 = new Projection(n, c0);
            var p1 = new Projection(n, c1);
            return Projection.DoesOverlap(p0, p1);
        }

        public static bool IsColliding(CircleCollider c0, CircleCollider c1, out MTV mtv)
        {
            mtv = new MTV();
            var n = math.normalizesafe(c0.Center - c1.Center);
            var p0 = new Projection(n, c0);
            var p1 = new Projection(n, c1);
            if (Projection.DoesOverlap(p0, p1))
            {
                mtv.Calculate(n, p0, p1, c0.Center, c1.Center);
                return true;
            }
            return false;
        }

        public static bool IsColliding(
            NativeArray<ColliderVertex> v0,
            NativeArray<ColliderVertex> v1,
            NativeArray<EdgeNormal> n0,
            NativeArray<EdgeNormal> n1)
        {
            for (int i = 0; i < n0.Length; i++)
            {
                var normal = n0[i].Value;
                var p0 = new Projection(normal, v0);
                var p1 = new Projection(normal, v1);
                if (!Projection.DoesOverlap(p0, p1))
                    return false;
            }

            for (int i = 0; i < n1.Length; i++)
            {
                var normal = n1[i].Value;
                var p0 = new Projection(normal, v0);
                var p1 = new Projection(normal, v1);
                if (!Projection.DoesOverlap(p0, p1))
                    return false;
            }

            return true;
        }

        public static bool IsColliding(
            NativeArray<ColliderVertex> v0,
            NativeArray<ColliderVertex> v1,
            NativeArray<EdgeNormal> n0,
            NativeArray<EdgeNormal> n1,
            float2 center0,
            float2 center1,
            out MTV mtv)
        {
            mtv = MTV.Max;
            for (int i = 0; i < n0.Length; i++)
            {
                var normal = n0[i].Value;
                var p0 = new Projection(normal, v0);
                var p1 = new Projection(normal, v1);
                if (!Projection.DoesOverlap(p0, p1))
                    return false;

                mtv.Calculate(normal, p0, p1, center0, center1);
            }

            for (int i = 0; i < n1.Length; i++)
            {
                var normal = n1[i].Value;
                var p0 = new Projection(normal, v0);
                var p1 = new Projection(normal, v1);
                if (!Projection.DoesOverlap(p0, p1))
                    return false;

                mtv.Calculate(normal, p0, p1, center1, center0);
            }

            return true;
        }

        public static bool IsColliding(
            CircleCollider circle,
            NativeArray<ColliderVertex> vertices,
            NativeArray<EdgeNormal> normals,
            float2 center,
            out MTV mtv)
        {
            mtv = MTV.Max;
            var shouldCheckCorner = true;
            for (int i = 0; i < normals.Length; i++)
            {
                var n = normals[i].Value;
                var p0 = new Projection(n, circle);
                var p1 = new Projection(n, vertices);
                var d = math.dot(circle.Center, n);
                shouldCheckCorner = shouldCheckCorner && !Projection.DoesOverlap(p1, d);

                if (!Projection.DoesOverlap(p0, p1))
                    return false;

                mtv.Calculate(n, p0, p1, circle.Center, center);
            }

            if (shouldCheckCorner)
            {
                var closest = ClosestPoint(circle.Center, vertices);
                var n = math.normalizesafe(closest - circle.Center);
                var p0 = new Projection(n, circle);
                var p1 = new Projection(n, vertices);

                if (!Projection.DoesOverlap(p0, p1))
                    return false;

                mtv.Calculate(n, p0, p1, circle.Center, center);
            }

            return true;
        }

        public static Bounds2D CalculateBounds(NativeArray<ColliderVertex> vertices)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i].Value;
                minX = math.min(v.x, minX);
                minY = math.min(v.y, minY);
                maxX = math.max(v.x, maxX);
                maxY = math.max(v.y, maxY);
            }

            return new Bounds2D
            {
                Min = new float2(minX, minY),
                Max = new float2(maxX, maxY),
            };
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
    }
}
