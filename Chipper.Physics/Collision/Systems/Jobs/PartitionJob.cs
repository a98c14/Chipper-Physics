using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Physics
{
    [BurstCompile]
    struct PartitionJob : IJobChunk
    {
        [ReadOnly] public ArchetypeChunkEntityType EntityType;
        [ReadOnly] public ArchetypeChunkComponentType<Bounds2D> BoundsType;
        [ReadOnly] public ArchetypeChunkComponentType<ColliderShape> ShapeType;
        [ReadOnly] public ArchetypeChunkComponentType<ColliderTag> TagType;
        [ReadOnly] public ArchetypeChunkComponentType<LargeCollider> LargeColliderType;

        // Every collider in Source map checks collisions against Target map
        [WriteOnly] public NativeMultiHashMap<int, ColliderData>.ParallelWriter ColliderSourceMap;
        [WriteOnly] public NativeMultiHashMap<int, ColliderData>.ParallelWriter ColliderTargetMap;

        [WriteOnly] public NativeQueue<LargeColliderData>.ParallelWriter LargeColliders;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var isRegular = !chunk.Has(LargeColliderType);

            if (isRegular)
                ProcessRegularCollider(chunk);
            else
                ProcessLargeCollider(chunk);
        }

        void ProcessRegularCollider(ArchetypeChunk chunk)
        {
            var count = chunk.Count;
            var entities = chunk.GetNativeArray(EntityType);
            var bounds = chunk.GetNativeArray(BoundsType);
            var shapes = chunk.GetNativeArray(ShapeType);
            var tags = chunk.GetNativeArray(TagType);
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
                    Shape = shape.Value,
                    Tags = tag.Target,
                };

                var targetCollider = new ColliderData
                {
                    Bounds = aabb,
                    Entity = entity,
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