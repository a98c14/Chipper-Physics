using UnityEngine;
using Unity.Entities;

namespace Chipper.Physics
{
    [RequiresEntityConversion, DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Physics/Physics Settings")]
    public class PhysicsSettingsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Gravity;
        public float FrictionCoefficient;

        Entity m_ConvertedEntity;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            m_ConvertedEntity = entity;

            dstManager.AddComponentData(entity, new PhysicsSettings
            {
                FrictionCoefficient = FrictionCoefficient,
                Gravity = Gravity,
            });
        }

        public void OnValidate()
        {
            if (!enabled || m_ConvertedEntity == Entity.Null)
                return;

            var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if(dstManager.HasComponent<PhysicsSettings>(m_ConvertedEntity))
            {
                dstManager.SetComponentData(m_ConvertedEntity, new PhysicsSettings
                {
                    FrictionCoefficient = FrictionCoefficient,
                    Gravity = Gravity,
                });
            }
        }
    }
}

