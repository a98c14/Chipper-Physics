using Unity.Entities;

namespace Chipper.Physics
{
    public enum ColliderShapeType
    {
        Polygon,
        Circle,
        Cone,
    }

    public struct ColliderShape : IComponentData
    {
        public ColliderShapeType Value;
    }
}
