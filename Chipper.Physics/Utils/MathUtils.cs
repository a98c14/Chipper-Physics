using Unity.Mathematics;
using Unity.Transforms;
using System.Runtime.CompilerServices;

namespace Chipper.Physics
{
    public static class MathUtil
    {
        public const float Deg2Rad = 0.0174532924F;
        public const float Rad2Deg = 57.295779513F;
        public const float Epsilon = 1.192093e-07F;

        public readonly static float2[] cellOffsets =
        {
            new int2( 0,  0),
            new int2(-1,  0),
            new int2( 0, -1),
            new int2( 1,  0),
            new int2( 0,  1),
            new int2( 1,  1),
            new int2(-1, -1),
            new int2( 1, -1),
            new int2(-1,  1),
        };

        public static Random AutoSeedRandom
        {
            get
            {
                var random = new Random((uint)System.Environment.TickCount);
                random.NextFloat(0, 1);
                return random;
            }
        }

        public static Random AutoSeedRandomSafe
        {
            get
            {
                var random = new Random((uint)System.Environment.TickCount);
                random.NextFloat(0, 1);
                random.NextFloat(0, 1);
                return random;
            }
        }

        public static uint AutoSeed
        {
            get
            {
                return (uint)System.Environment.TickCount;
            }
        }

        public static float3 MidPointBetweenVectors(float3 v0, float3 v1)
        {
            return v0 + ((v1 - v0) / 2);
        }
    
        public static float3 MidPointBetweenVectors(float2 v0, float2 v1)
        {
            return new float3(v0 + ((v1 - v0) / 2), 0);
        }

        public static bool Approx(float a, float b)
        {
            return math.abs(a - b) < Epsilon;
        }

        public static float2 Project(float2 a, float2 b)
        {
            var dp = math.dot(a, b);
            var db = math.dot(b, b);
            var s  = dp / db;
            return new float2(s * b.x, s * b.y);
        }

        public static float3 Project(float3 a, float3 b)
        {
            var dp = math.dot(a, b);
            var db = math.dot(b, b);
            var s  = dp / db;
            return new float3(s * b.x, s * b.y, 0);
        }

        public static float3 AngleToNormalizedVec3(float angle)
        {
            float rad = angle * Deg2Rad;
            float cos = math.cos(rad);
            float sin = math.sin(rad);
            return math.normalize(new float3(cos, sin, 0));
        }

        public static float2 AngleToNormalizedVec2(float angle)
        {
            float rad = angle * Deg2Rad;
            float cos = math.cos(rad);
            float sin = math.sin(rad);
            return math.normalize(new float2(cos, sin));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 NormalizeAndRotate2(float2 v, float angle)
        {
            var v3 = new float3(math.normalize(v).xy, 0);
            var c = math.cos(angle * Deg2Rad);
            var s = math.sin(angle * Deg2Rad);
            return new float3(v3.x * c + v3.y * -s, v3.x * s + v3.y * c, v3.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 NormalizeAndRotate3(float3 v, float angle)
        {
            var normalized = math.normalize(v);
            var c = math.cos(angle * Deg2Rad);
            var s = math.sin(angle * Deg2Rad);
            return new float3(normalized.x * c + normalized.y * -s, normalized.x * s + normalized.y * c, normalized.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RotateVector(float3 v, float angle)
        {
            var c = math.cos(angle * Deg2Rad);
            var s = math.sin(angle * Deg2Rad);
            return new float3(v.x * c + v.y * -s, v.x * s + v.y * c, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 RotateVector(float2 v, float angle)
        {
            var c = math.cos(angle * Deg2Rad);
            var s = math.sin(angle * Deg2Rad);
            return new float2(v.x * c + v.y * -s, v.x * s + v.y * c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x2 GetRotationMatrix(float angle)
        {
            var c = math.cos(angle * Deg2Rad);
            var s = math.sin(angle * Deg2Rad);
            return new float2x2(c, -s, s, c);
        }

        public static float Angle(float3 v, float3 w)
        {
            float angleV = math.atan2(v.y, v.x);
            float angleW = math.atan2(w.y, w.x);
            return angleV - angleW;
        }

        public static float Angle(float3 v)
        {
            return math.atan2(v.y, v.x) * Rad2Deg;
        }

        public static bool IsInRange(float2 a, float2 b, float distance)
        {
            return math.length(a - b) < distance;
        }

        public static float GetFallDuration(float gravity, float height)
        {
            return math.sqrt(2 * height / gravity);
        }

        public static float GetFlightDuration(float gravity, float zForce)
        {
            return 2 * zForce / gravity;
        }

        public static float3 GetRenderPosition(float3 position)
        {
            return new float3(position.x, position.y + position.z * Constants.ZConstant, 0);
        }
    }
}
