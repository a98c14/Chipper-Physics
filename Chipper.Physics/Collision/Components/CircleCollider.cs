using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    public struct CircleCollider : IComponentData
    {
        public float2 Center;
        public float  Radius;
        public float  RadiusSq;

        public float2 ClosestPoint(float2 v) => math.normalizesafe(v - Center) * Radius;
        public Projection Project(float2 v)  => new Projection(math.dot(Center, v) - Radius, math.dot(Center, v) + Radius);
    }
}
