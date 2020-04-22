using UnityEngine;
using Unity.Entities;

namespace Chipper.Physics
{
    [RequiresEntityConversion, DisallowMultipleComponent]
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

            dstManager.AddComponentData(entity, new Force());
            dstManager.AddComponentData(entity, new Velocity());
            dstManager.AddComponentData(entity, new Acceleration());

            if (IsAffectedByGravity)
                dstManager.AddComponentData(entity, new AffectedByGravity());
        }
    }
}
