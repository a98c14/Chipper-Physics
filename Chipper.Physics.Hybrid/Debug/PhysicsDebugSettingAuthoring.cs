using Unity.Entities;
using UnityEngine;

namespace Chipper.Physics
{
    [RequiresEntityConversion, DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Physics/Physics Debug Settings")]
    public class PhysicsDebugSettingAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public bool IsEnabled      = true;
        public bool DrawGrid       = true;
        public bool DrawBounds     = true;
        public bool DrawNormals    = true;
        public bool DrawInfoLabels = true;

        Entity m_ConvertedEntity = Entity.Null;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var settings = new PhysicsDebugSettings
            {
                IsEnabled   = IsEnabled,
                DrawGrid    = DrawGrid,
                DrawBounds  = DrawBounds,
                DrawNormals = DrawNormals,
                DrawInfoLabels = DrawInfoLabels,
            };
            dstManager.AddComponentData(entity, settings);
            m_ConvertedEntity = entity;
        }

        public void OnValidate()
        {
            if(!enabled || m_ConvertedEntity == Entity.Null) return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.HasComponent<PhysicsDebugSettings>(m_ConvertedEntity))
            {
                var component = entityManager.GetComponentData<PhysicsDebugSettings>(m_ConvertedEntity);
                component.IsEnabled   = IsEnabled;
                component.DrawBounds  = DrawBounds;
                component.DrawNormals = DrawNormals;
                component.DrawInfoLabels = DrawInfoLabels;
                component.DrawGrid = DrawGrid;

                entityManager.SetComponentData(m_ConvertedEntity, component);
            }
        }
    }
}
