using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace Chipper.Physics
{
    [RequiresEntityConversion, DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Physics/Collider Authoring")]
    public class ColliderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public ColliderTagType ColliderTags;
        public ColliderTagType CollidesWith;
        public CollisionResolutionType CollisionResolution;
        public Vector3 PivotOffset; 
        public float Height;           
        public ColliderShapeType Shape;

        public bool IsBiggerThanCellSize;
        public float Radius;           // Circle & Cone
        public Vector2 CenterOffset;   // Circle & Cone 
        public float Angle;            // Cone
        public List<Vector2> Vertices; // Polygon

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<CollisionBuffer>(entity);
            dstManager.AddComponentData(entity, new Pivot());
            dstManager.AddComponentData(entity, new PivotOffset(PivotOffset));
            dstManager.AddComponentData(entity, new Bounds2D());
            dstManager.AddComponentData(entity, new ColliderShape { Value = Shape });
            dstManager.AddComponentData(entity, new ColliderTag { Self = CollidesWith, Target = ColliderTags});

            if (IsBiggerThanCellSize)
                dstManager.AddComponentData(entity, new LargeCollider());

            switch (CollisionResolution)
            {
                case CollisionResolutionType.MoveOut:
                    dstManager.AddComponentData(entity, new MoveOutOfCollision());
                    break;
                case CollisionResolutionType.ForceOut:
                    dstManager.AddComponentData(entity, new ForceOutOfCollision());
                    break;
            }

            AddColliderInfo(entity, dstManager, Shape);
        }

        void AddColliderInfo(Entity entity, EntityManager dstManager, ColliderShapeType shape)
        {
            switch (shape)
            {
                case ColliderShapeType.Circle:
                    AddCircleCollider(entity, dstManager);
                    break;
                case ColliderShapeType.Polygon:
                    AddPolygonCollider(entity, dstManager);
                    break;
                case ColliderShapeType.Cone:
                    break;
            }
        }

        void AddCircleCollider(Entity entity, EntityManager dstManager)
        {
            dstManager.AddComponentData(entity, new CurvedColliderInfo
            {
                Height = Height,
                Angle = Angle,
                Radius = Radius,
            });
            dstManager.AddComponentData(entity, new CircleCollider());
        }

        void AddPolygonCollider(Entity entity, EntityManager dstManager)
        {
            dstManager.AddComponentData(entity, new PolygonColliderInfo
            {
                Height = Height,
                VertexOffsets = CreateBlobReference(Vertices),
            });
            var vertices = dstManager.AddBuffer<ColliderVertex>(entity);
            vertices.ResizeUninitialized(Vertices.Count);
            var normals = dstManager.AddBuffer<EdgeNormal>(entity);
            normals.ResizeUninitialized(Vertices.Count);
        }

        BlobAssetReference<BlobArray<float2>> CreateBlobReference(List<Vector2> vertices)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobArray<float2>>();
            var arr = builder.Allocate(ref root, vertices.Count);

            for(int i = 0; i < vertices.Count; i++)
                arr[i] = vertices[i];

            var reference = builder.CreateBlobAssetReference<BlobArray<float2>>(Allocator.Persistent);
            builder.Dispose();

            return reference;
        }
    }
}
