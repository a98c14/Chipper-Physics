using UnityEngine;
using Unity.Entities;

namespace Chipper.Physics
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Physics/Physics Body Authoring")]
    public class PhysicsBodyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public bool IsAffectedByGravity;
        public float Mass;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Mass
            {
                Value = Mass,
            });

            dstManager.AddComponentData(entity, Force.Zero);
            dstManager.AddComponentData(entity, Velocity.Zero);
            dstManager.AddComponentData(entity, Acceleration.Zero);

            if (IsAffectedByGravity)
                dstManager.AddComponentData(entity, new AffectedByGravity());
        }
    }
}
