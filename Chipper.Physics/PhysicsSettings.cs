using Unity.Entities;

namespace Chipper.Physics
{
    public struct PhysicsSettings : IComponentData
    {
        public float Gravity;
        public float FrictionCoefficient;

        public static PhysicsSettings Default => new PhysicsSettings
        {
            FrictionCoefficient = .6f,
            Gravity = 10f,
        };
    }
}