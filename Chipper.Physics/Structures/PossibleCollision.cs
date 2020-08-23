using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    struct PossibleCollision
    {
        public ColliderShapeType SourceType;
        public ColliderShapeType TargetType;
        public Entity SourceEntity;
        public Entity TargetEntity;
        public Entity SourceCollider;
        public Entity TargetCollider;
        public float2 SourceCenter;
        public float2 TargetCenter;

        public long CollisionID
        {
            get
            {
                var packer = new PhysicsUtil.IntPacker
                {
                    LowInt  = SourceCollider.Index,
                    HighInt = TargetCollider.Index
                };
                return packer.LongValue;
            }
        }
    }
}
