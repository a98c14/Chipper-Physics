using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    struct PossibleCollision
    {
        public ColliderShapeType SourceType;
        public ColliderShapeType TargetType;
        public Entity Source;
        public Entity Target;
        public float2 SourceCenter;
        public float2 TargetCenter;

        public long CollisionID
        {
            get
            {
                var packer = new PhysicsUtil.IntPacker
                {
                    LowInt = Source.Index,
                    HighInt = Target.Index
                };
                return packer.LongValue;
            }
        }
    }
}
