using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public sealed class BattlefieldGridAuthoring : MonoBehaviour
    {
        public BattlefieldGrid Grid;

        public sealed class Baker : Baker<BattlefieldGridAuthoring>
        {
            public override void Bake(BattlefieldGridAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                using var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BattlefieldGridBlob>();

                root.Origin = authoring.Grid.Origin;
                root.CellSize = authoring.Grid.CellSize;
                root.Width = authoring.Grid.Width;
                root.Height = authoring.Grid.Height;

                var walkable = builder.Allocate(ref root.Walkable, authoring.Grid.Width * authoring.Grid.Height);
                var source = authoring.Grid.Walkable;

                for (int i = 0; i < walkable.Length; i++)
                    walkable[i] = source[i];

                var blobRef = builder.CreateBlobAssetReference<BattlefieldGridBlob>(Allocator.Persistent);

                AddComponent(entity, new BattlefieldGridSingleton {
                    Value = blobRef
                });
            }
        }
    }
}