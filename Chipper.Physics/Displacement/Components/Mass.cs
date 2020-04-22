using Unity.Entities;

namespace Chipper.Physics
{
    public struct Mass : IComponentData
    {
        public bool  IsDisabled;
        public float Value;
    }
}
