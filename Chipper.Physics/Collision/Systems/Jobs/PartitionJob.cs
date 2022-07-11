using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Chipper.Transforms;

namespace Chipper.Physics
{
    [BurstCompile]
    struct PartitionJob : IJobChunk
    {
        [ReadOnly] public EntityTypeHandle EntityType;
        [ReadOnly] public ComponentTypeHandle<Bounds2D> BoundsType;
        [ReadOnly] public ComponentTypeHandle<ColliderShape> ShapeType;
        [ReadOnly] public ComponentTypeHandle<ColliderTag> TagType;
        [ReadOnly] public ComponentTypeHandle<LargeCollider> LargeColliderType;
        [ReadOnly] public ComponentTypeHandle<Parent2D> ParentType;

        // Every collider in Source map checks collisions against Target map
        [WriteOnly] public NativeParallelMultiHashMap<int, ColliderData>.ParallelWriter ColliderSourceMap;
        [WriteOnly] public NativeParallelMultiHashMap<int, ColliderData>.ParallelWriter ColliderTargetMap;

        [WriteOnly] public NativeQueue<LargeColliderData>.ParallelWriter LargeColliders;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var isRegular = !chunk.Has(LargeColliderType);
            var hasParent = chunk.Has(ParentType);

            if (!hasParent && isRegular)
                ProcessRegularCollider(chunk);
            else if (hasParent && isRegular)
                ProcessParentCollider(chunk);
            else
                ProcessLargeCollider(chunk);
        }

        void ProcessParentCollider(ArchetypeChunk chunk)
        {
            var count = chunk.Count;
            var parents = chunk.GetNativeArray(ParentType);
            var entities = chunk.GetNativeArray(EntityType);
            var bounds = chunk.GetNativeArray(BoundsType);
            var shapes = chunk.GetNativeArray(ShapeType);
            var tags = chunk.GetNativeArray(TagType);
            var cellSize = Constants.CellSize;

            for (int i = 0; i < count; i++)
            {
                var entity = entities[i];
                var aabb   = bounds[i];
                var shape  = shapes[i];
                var tag    = tags[i];
                var parent = parents[i];

                var sourceCollider = new ColliderData
                {
                    Bounds = aabb,
                    Entity = parent.Value,
                    ColliderEntity = entity,
                    Shape  = shape.Value,
                    Tags   = tag.Target,
                };

                var targetCollider = new ColliderData
                {
                    Bounds = aabb,
                    Entity = parent.Value,
                    ColliderEntity = entity,
                    Shape = shape.Value,
                    Tags = tag.Self,
                };

                // Add collider to where its center is inside collider map
                var hash = HashUtil.Hash(aabb.Center, cellSize);
                ColliderSourceMap.Add(hash, sourceCollider);

                // Add collider to every cell it stays in + neighbour cells inside target map
                var gridBounds = PhysicsUtil.GetGridBounds(aabb, cellSize);
                for (int x = gridBounds.x; x < gridBounds.z; x++)
                {
                    for (int y = gridBounds.y; y < gridBounds.w; y++)
                    {
                        hash = HashUtil.Hash(new int2(x, y));
                        ColliderTargetMap.Add(hash, targetCollider);
                    }
                }
            }
        }

        void ProcessRegularCollider(ArchetypeChunk chunk)
        {
            var count    = chunk.Count;
            var entities = chunk.GetNativeArray(EntityType);
            var bounds   = chunk.GetNativeArray(BoundsType);
            var shapes   = chunk.GetNativeArray(ShapeType);
            var tags     = chunk.GetNativeArray(TagType);
            var cellSize = Constants.CellSize;

            for (int i = 0; i < count; i++)
            {
                var entity = entities[i];
                var aabb = bounds[i];
                var shape = shapes[i];
                var tag = tags[i];

                var sourceCollider = new ColliderData
                {
                    Bounds = aabb,
                    Entity = entity,
                    ColliderEntity = entity,
                    Shape  = shape.Value,
                    Tags   = tag.Target,
                };

                var targetCollider = new ColliderData
                {
                    Bounds = aabb,
                    Entity = entity,
                    ColliderEntity = entity,
                    Shape  = shape.Value,
                    Tags   = tag.Self,
                };
                 
                // Add collider to where its center is inside collider map
                var hash = HashUtil.Hash(aabb.Center, cellSize);
                ColliderSourceMap.Add(hash, sourceCollider);

                // Add collider to every cell it stays in + neighbour cells inside target map
                var gridBounds = PhysicsUtil.GetGridBounds(aabb, cellSize);
                for (int x = gridBounds.x; x < gridBounds.z; x++)
                {
                    for (int y = gridBounds.y; y < gridBounds.w; y++)
                    {
                        hash = HashUtil.Hash(new int2(x, y));
                        ColliderTargetMap.Add(hash, targetCollider);
                    }
                }
            }
        }

        void ProcessLargeCollider(ArchetypeChunk chunk)
        {
            var count = chunk.Count;
            var entities = chunk.GetNativeArray(EntityType);
            var bounds = chunk.GetNativeArray(BoundsType);
            var shapes = chunk.GetNativeArray(ShapeType);
            var tags = chunk.GetNativeArray(TagType);

            for (int i = 0; i < count; i++)
            {
                var entity = entities[i];
                var aabb = bounds[i];
                var shape = shapes[i];
                var tag = tags[i];

                var collider = new LargeColliderData
                {
                    Bounds = aabb,
                    Entity = entity,
                    Shape = shape.Value,
                    SourceTags = tag.Self,
                    TargetTags = tag.Target,
                };

                LargeColliders.Enqueue(collider);
            }
        }
    }

}