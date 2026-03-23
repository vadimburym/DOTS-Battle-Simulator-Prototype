using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace VATDots
{
    [MaterialProperty("unity_RendererUserValuesPropertyEntry")]
    public struct VATRendererUserValue : IComponentData
    {
        public uint Value;
    }

    public struct VATDebugObjectId : IComponentData
    {
        public int Value;
    }

    public struct VATClipRuntimeData
    {
        public int MeshIndex;
        public float Length;
    }

    public struct VATAnimationLibraryBlob
    {
        public BlobArray<VATClipRuntimeData> Clips;
    }

    public struct VATLibraryBlobRef : IComponentData
    {
        public BlobAssetReference<VATAnimationLibraryBlob> Value;
    }

    public struct VATAnimator : IComponentData
    {
        public int MeshIndex;
        public int CurrentClipIndex;
        public float CurrentNormalizedTime;

        public int PreviousClipIndex;
        public float PreviousNormalizedTime;

        public float Blend01;
        public float BlendElapsed;
        public float BlendDuration;
        public float DefaultTransitionDuration;

        public float Speed;
        public byte Loop;
        public byte Playing;
        public byte Reserved0;
        public byte Reserved1;
    }

    public struct VATAnimationCommand : IComponentData
    {
        public int RequestedClipIndex;
        public float TransitionDuration;
        public float StartNormalizedTime;
        public byte RestartIfSame;
        public byte Reserved0;
        public byte Reserved1;
        public byte Reserved2;
    }

    [DisallowMultipleComponent]
    public sealed class VATAnimationAuthoring : MonoBehaviour
    {
        [Header("Library")]
        public VATAnimationLibrary library;
        public Renderer targetRenderer;
        public Mesh overrideMesh;
        public bool applyLibraryToSharedMaterialInEditor = true;

        [Header("Initial Playback")]
        public int initialClipIndex;
        [Range(0f, 1f)] public float initialNormalizedTime;
        public bool randomizeStartTime;
        public float speed = 1f;
        public bool loop = true;
        public bool playOnStart = true;
        public float defaultTransitionDuration = 0.15f;

        public Mesh GetResolvedMesh()
        {
            if (overrideMesh != null)
                return overrideMesh;

            if (TryGetComponent(out MeshFilter meshFilter))
                return meshFilter.sharedMesh;

            if (TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
                return skinnedMeshRenderer.sharedMesh;

            return null;
        }

        public int GetDetectedMeshIndex()
        {
            return library == null ? -1 : library.FindMeshIndex(GetResolvedMesh());
        }

        public void ApplyLibraryToSharedMaterial()
        {
            if (library == null)
                return;

            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer == null || targetRenderer.sharedMaterial == null)
                return;

            var sharedMaterial = targetRenderer.sharedMaterial;
            sharedMaterial.enableInstancing = true;
            sharedMaterial.SetTexture("_VATPositionTex", library.positionTexture);
            sharedMaterial.SetTexture("_VATNormalTex", library.normalTexture);
            sharedMaterial.SetTexture("_VATMetaTex", library.metadataTexture);
            sharedMaterial.SetVector("_VATBoundsMin", new Vector4(library.boundsMin.x, library.boundsMin.y, library.boundsMin.z, 0f));
            sharedMaterial.SetVector("_VATBoundsExtent", new Vector4(library.boundsExtent.x, library.boundsExtent.y, library.boundsExtent.z, 0f));
        }

        public bool RequestPlayClipOnRuntimeEntity(int clipIndex, float transitionDuration, float startNormalizedTime, bool restartIfSame)
        {
            if (!TryFindRuntimeEntity(out var entity))
                return false;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return false;

            var entityManager = world.EntityManager;
            if (!entityManager.Exists(entity) || !entityManager.HasComponent<VATAnimationCommand>(entity))
                return false;

            var command = entityManager.GetComponentData<VATAnimationCommand>(entity);
            command.RequestedClipIndex = clipIndex;
            command.TransitionDuration = transitionDuration;
            command.StartNormalizedTime = math.saturate(startNormalizedTime);
            command.RestartIfSame = restartIfSame ? (byte)1 : (byte)0;
            entityManager.SetComponentData(entity, command);
            return true;
        }

        public bool TryFindRuntimeEntity(out Entity entity)
        {
            entity = Entity.Null;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
                return false;

            var entityManager = world.EntityManager;
            var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<VATDebugObjectId>(), ComponentType.ReadOnly<VATAnimationCommand>());
            using var entities = query.ToEntityArray(Allocator.Temp);
            using var ids = query.ToComponentDataArray<VATDebugObjectId>(Allocator.Temp);

            int objectId = GetInstanceID();
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i].Value == objectId)
                {
                    entity = entities[i];
                    return true;
                }
            }

            return false;
        }

        private void Reset()
        {
            targetRenderer = GetComponent<Renderer>();
        }

        private sealed class Baker : Baker<VATAnimationAuthoring>
        {
            public override void Bake(VATAnimationAuthoring authoring)
            {
                if (authoring.library == null)
                    return;

                Entity entity = GetEntity(TransformUsageFlags.Renderable);
                Mesh mesh = authoring.GetResolvedMesh();
                int meshIndex = authoring.library.FindMeshIndex(mesh);
                if (meshIndex < 0)
                {
                    Debug.LogError($"[VAT DOTS] Mesh '{(mesh != null ? mesh.name : "<null>")}' is not present in VATAnimationLibrary '{authoring.library.name}'.", authoring);
                    return;
                }

                int initialClip = authoring.initialClipIndex;
                if (!authoring.library.ClipBelongsToMesh(initialClip, meshIndex))
                    initialClip = authoring.library.FindFirstClipForMesh(meshIndex);

                if (initialClip < 0)
                {
                    Debug.LogError($"[VAT DOTS] No baked clips found for meshIndex={meshIndex} in library '{authoring.library.name}'.", authoring);
                    return;
                }

                float startTime = math.saturate(authoring.initialNormalizedTime);
                if (authoring.randomizeStartTime)
                    startTime = HashTo01((uint)authoring.GetInstanceID());

                var blobRef = BuildLibraryBlob(authoring.library);
                AddBlobAsset(ref blobRef, out _);

                AddComponent(entity, new VATLibraryBlobRef
                {
                    Value = blobRef,
                });

                AddComponent(entity, new VATAnimator
                {
                    MeshIndex = meshIndex,
                    CurrentClipIndex = initialClip,
                    CurrentNormalizedTime = startTime,
                    PreviousClipIndex = -1,
                    PreviousNormalizedTime = 0f,
                    Blend01 = 1f,
                    BlendElapsed = 0f,
                    BlendDuration = math.max(0f, authoring.defaultTransitionDuration),
                    DefaultTransitionDuration = math.max(0f, authoring.defaultTransitionDuration),
                    Speed = authoring.speed,
                    Loop = authoring.loop ? (byte)1 : (byte)0,
                    Playing = authoring.playOnStart ? (byte)1 : (byte)0,
                });

                AddComponent(entity, new VATAnimationCommand
                {
                    RequestedClipIndex = -1,
                    TransitionDuration = -1f,
                    StartNormalizedTime = 0f,
                    RestartIfSame = 0,
                });

                AddComponent(entity, new VATRendererUserValue
                {
                    Value = 0u,
                });

                AddComponent(entity, new VATDebugObjectId
                {
                    Value = authoring.GetInstanceID(),
                });
            }

            private static BlobAssetReference<VATAnimationLibraryBlob> BuildLibraryBlob(VATAnimationLibrary library)
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<VATAnimationLibraryBlob>();
                int clipCount = library.clips != null ? library.clips.Length : 0;
                var clips = builder.Allocate(ref root.Clips, clipCount);
                for (int i = 0; i < clipCount; i++)
                {
                    var clip = library.clips[i];
                    clips[i] = new VATClipRuntimeData
                    {
                        MeshIndex = clip != null ? clip.meshIndex : -1,
                        Length = clip != null ? math.max(clip.length, 1e-5f) : 1e-5f,
                    };
                }

                return builder.CreateBlobAssetReference<VATAnimationLibraryBlob>(Allocator.Persistent);
            }

            private static float HashTo01(uint value)
            {
                value ^= 2747636419u;
                value *= 2654435769u;
                value ^= value >> 16;
                value *= 2654435769u;
                value ^= value >> 16;
                value *= 2654435769u;
                return (value & 0x00FFFFFFu) / 16777216.0f;
            }
        }
    }
}
