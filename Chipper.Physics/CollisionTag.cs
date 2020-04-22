using Unity.Entities;

namespace Chipper.Physics
{
    [System.Flags]
    public enum ColliderTagType
    {
        Disabled     = 0 << 0,
        Corpse       = 1 << 0,
        Enemy        = 1 << 1,
        Player       = 1 << 2,
        PlayerAttack = 1 << 3,
        EnemyAttack  = 1 << 4,
        Interaction  = 1 << 5,
    }

    public struct ColliderTag : IComponentData
    {
        // Tags other colliders check when colliding with this entity
        public ColliderTagType Self;

        // Tags collider collides with
        public ColliderTagType Target;
    }
}