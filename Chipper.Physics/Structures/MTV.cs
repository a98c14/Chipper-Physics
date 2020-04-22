using Unity.Mathematics;

// Minimum Translation Vector
namespace Chipper.Physics
{
    public struct MTV
    {
        public float2 Direction;
        public float Magnitude;

        public static MTV Max => new MTV
        {
            Magnitude = float.MaxValue,
        };
        
        public void Calculate(float2 normal, Projection p0, Projection p1, float2 center0, float2 center1)
        {
            var o0 = p0.Max - p1.Min;
            var o1 = p0.Min - p1.Max;
            var m = math.min(math.abs(o0), math.abs(o1));
            var v = center1 - center0;
            var d = math.dot(v, normal);
            var sign = d > 0 ? 1 : -1;

            if (math.abs(m) <= math.abs(Magnitude))
            {
                Magnitude = m;
                Direction = normal * sign;
            }
        }

        public override string ToString() => $"MTV ({ Direction.x.ToString("0.##") }, { Direction.y.ToString("0.##") })\nLength: { Magnitude }";
    }
}