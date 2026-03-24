#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATDots.Editor
{
    [DisallowMultipleComponent]
    public sealed class VATBaker : MonoBehaviour
    {
        [Serializable]
        public sealed class CharacterEntry
        {
            public string name;
            public GameObject sourcePrefab;
            [Tooltip("Optional transform path to a specific SkinnedMeshRenderer. Leave empty to use the first one found.")]
            public string skinnedMeshRendererPath;
            public List<AnimationClip> clips = new();
        }

        private sealed class FrameData
        {
            public Vector3[] positions;
            public Vector3[] normals;
        }

        [Header("Bake Output")]
        public string outputFolder = "Assets/VATBaked";
        public string outputName = "UnitsVAT";
        [Min(1)] public int framesPerClip = 16;
        public Material targetMaterial;
        public VATAnimationLibrary targetLibraryAsset;

        [Header("Input")]
        public List<CharacterEntry> characters = new();

        [ContextMenu("Bake VAT")]
        public void Bake()
        {
            if (framesPerClip < 1)
            {
                Debug.LogError("[VAT DOTS] framesPerClip must be >= 1.", this);
                return;
            }

            if (characters == null || characters.Count == 0)
            {
                Debug.LogError("[VAT DOTS] Add at least one character entry.", this);
                return;
            }

            Directory.CreateDirectory(outputFolder);

            var meshEntries = new List<VATAnimationLibrary.MeshEntry>();
            var clipEntries = new List<VATAnimationLibrary.ClipEntry>();
            var frames = new List<FrameData>();

            int maxVertexCount = 0;
            Vector3 globalMin = new(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 globalMax = new(float.MinValue, float.MinValue, float.MinValue);

            AnimationMode.StartAnimationMode();
            try
            {
                int meshIndex = 0;
                foreach (var character in characters)
                {
                    if (character == null || character.sourcePrefab == null)
                        continue;

                    GameObject instance = null;
                    try
                    {
                        instance = (GameObject)PrefabUtility.InstantiatePrefab(character.sourcePrefab);
                        if (instance == null)
                            throw new InvalidOperationException($"Failed to instantiate prefab '{character.sourcePrefab.name}'.");

                        instance.hideFlags = HideFlags.HideAndDontSave;
                        instance.transform.position = Vector3.zero;
                        instance.transform.rotation = Quaternion.identity;
                        instance.transform.localScale = Vector3.one;

                        var skinnedRenderer = ResolveRenderer(instance, character.skinnedMeshRendererPath);
                        if (skinnedRenderer == null)
                            throw new InvalidOperationException($"No SkinnedMeshRenderer found for '{character.sourcePrefab.name}'.");

                        skinnedRenderer.updateWhenOffscreen = true;
                        Mesh sourceMesh = skinnedRenderer.sharedMesh;
                        if (sourceMesh == null)
                            throw new InvalidOperationException($"SkinnedMeshRenderer on '{character.sourcePrefab.name}' has no sharedMesh.");

                        maxVertexCount = Mathf.Max(maxVertexCount, sourceMesh.vertexCount);
                        meshEntries.Add(new VATAnimationLibrary.MeshEntry
                        {
                            name = string.IsNullOrWhiteSpace(character.name) ? character.sourcePrefab.name : character.name,
                            sourceMesh = sourceMesh,
                            meshIndex = meshIndex,
                            vertexCount = sourceMesh.vertexCount,
                        });

                        foreach (var clip in character.clips)
                        {
                            if (clip == null)
                                continue;

                            int rowStart = frames.Count;
                            for (int frame = 0; frame < framesPerClip; frame++)
                            {
                                float t = framesPerClip == 1 ? 0f : (clip.length * frame) / framesPerClip;
                                SampleClip(instance, clip, t);

                                using var bakedMesh = new DisposableMesh();
                                skinnedRenderer.BakeMesh(bakedMesh.Mesh);

                                Vector3[] positions = bakedMesh.Mesh.vertices;
                                Vector3[] normals = bakedMesh.Mesh.normals;
                                if (normals == null || normals.Length != positions.Length)
                                    normals = sourceMesh.normals;

                                frames.Add(new FrameData
                                {
                                    positions = positions,
                                    normals = normals,
                                });

                                for (int i = 0; i < positions.Length; i++)
                                {
                                    globalMin = Vector3.Min(globalMin, positions[i]);
                                    globalMax = Vector3.Max(globalMax, positions[i]);
                                }
                            }

                            clipEntries.Add(new VATAnimationLibrary.ClipEntry
                            {
                                name = $"{meshEntries[^1].name}/{clip.name}",
                                clipName = clip.name,
                                clipIndex = clipEntries.Count,
                                meshIndex = meshIndex,
                                rowStart = rowStart,
                                frameCount = framesPerClip,
                                length = Mathf.Max(clip.length, 0.0001f),
                            });
                        }
                    }
                    finally
                    {
                        if (instance != null)
                            DestroyImmediate(instance);
                    }

                    meshIndex++;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                return;
            }
            finally
            {
                AnimationMode.StopAnimationMode();
            }

            if (clipEntries.Count == 0 || frames.Count == 0)
            {
                Debug.LogError("[VAT DOTS] No clips were baked.", this);
                return;
            }

            Vector3 extent = globalMax - globalMin;
            extent.x = Mathf.Max(extent.x, 0.0001f);
            extent.y = Mathf.Max(extent.y, 0.0001f);
            extent.z = Mathf.Max(extent.z, 0.0001f);

            Texture2D positionTexture = CreateAndFillPositionTexture(maxVertexCount, frames, globalMin, extent);
            Texture2D normalTexture = CreateAndFillNormalTexture(maxVertexCount, frames);
            Texture2D metadataTexture = CreateMetadataTexture(clipEntries);

            string positionPath = SaveEXR(positionTexture, outputFolder, outputName + "_Position.exr");
            string normalPath = SaveEXR(normalTexture, outputFolder, outputName + "_Normal.exr");
            string metadataPath = SaveEXR(metadataTexture, outputFolder, outputName + "_Meta.exr");

            AssetDatabase.Refresh();
            ConfigureTextureImporter(positionPath);
            ConfigureTextureImporter(normalPath);
            ConfigureTextureImporter(metadataPath);
            AssetDatabase.Refresh();

            var library = targetLibraryAsset == null
                ? ScriptableObject.CreateInstance<VATAnimationLibrary>()
                : targetLibraryAsset;

            library.positionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(positionPath);
            library.normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            library.metadataTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(metadataPath);
            library.boundsMin = globalMin;
            library.boundsExtent = extent;
            library.maxVertexCount = maxVertexCount;
            library.totalFrameRows = frames.Count;
            library.meshes = meshEntries.ToArray();
            library.clips = clipEntries.ToArray();

            string libraryPath = string.IsNullOrEmpty(AssetDatabase.GetAssetPath(library))
                ? AssetDatabase.GenerateUniqueAssetPath(Path.Combine(outputFolder, outputName + "_Library.asset"))
                : AssetDatabase.GetAssetPath(library);

            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(library)))
                AssetDatabase.CreateAsset(library, libraryPath);

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (targetMaterial != null)
            {
                ApplyLibraryToMaterial(targetMaterial, library);
                EditorUtility.SetDirty(targetMaterial);
            }

            DestroyImmediate(positionTexture);
            DestroyImmediate(normalTexture);
            DestroyImmediate(metadataTexture);

            Debug.Log($"[VAT DOTS] Bake complete. Meshes={meshEntries.Count}, Clips={clipEntries.Count}, Rows={frames.Count}, MaxVertices={maxVertexCount}", this);
        }

        public static void ApplyLibraryToMaterial(Material material, VATAnimationLibrary library)
        {
            if (material == null || library == null)
                return;

            material.enableInstancing = true;
            material.SetTexture("_VATPositionTex", library.positionTexture);
            material.SetTexture("_VATNormalTex", library.normalTexture);
            material.SetTexture("_VATMetaTex", library.metadataTexture);
            material.SetVector("_VATBoundsMin", new Vector4(library.boundsMin.x, library.boundsMin.y, library.boundsMin.z, 0f));
            material.SetVector("_VATBoundsExtent", new Vector4(library.boundsExtent.x, library.boundsExtent.y, library.boundsExtent.z, 0f));
        }

        private static SkinnedMeshRenderer ResolveRenderer(GameObject root, string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Transform t = root.transform.Find(path);
                if (t != null && t.TryGetComponent(out SkinnedMeshRenderer found))
                    return found;
            }

            return root.GetComponentInChildren<SkinnedMeshRenderer>(true);
        }

        private static void SampleClip(GameObject instance, AnimationClip clip, float time)
        {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(instance, clip, time);
            AnimationMode.EndSampling();
        }

        private static Texture2D CreateAndFillPositionTexture(int width, List<FrameData> frames, Vector3 min, Vector3 extent)
        {
            var tex = new Texture2D(width, frames.Count, TextureFormat.RGBAHalf, mipChain: false, linear: true)
            {
                name = "VAT_Position",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };

            var pixels = new Color[width * frames.Count];
            for (int row = 0; row < frames.Count; row++)
            {
                Vector3[] positions = frames[row].positions;
                for (int x = 0; x < width; x++)
                {
                    int index = row * width + x;
                    if (x < positions.Length)
                    {
                        Vector3 p = positions[x];
                        pixels[index] = new Color(
                            (p.x - min.x) / extent.x,
                            (p.y - min.y) / extent.y,
                            (p.z - min.z) / extent.z,
                            1f);
                    }
                    else
                    {
                        pixels[index] = Color.black;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static Texture2D CreateAndFillNormalTexture(int width, List<FrameData> frames)
        {
            var tex = new Texture2D(width, frames.Count, TextureFormat.RGBAHalf, mipChain: false, linear: true)
            {
                name = "VAT_Normal",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };

            var pixels = new Color[width * frames.Count];
            for (int row = 0; row < frames.Count; row++)
            {
                Vector3[] normals = frames[row].normals;
                for (int x = 0; x < width; x++)
                {
                    int index = row * width + x;
                    if (x < normals.Length)
                    {
                        Vector3 n = normals[x].normalized;
                        pixels[index] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                    }
                    else
                    {
                        pixels[index] = new Color(0.5f, 0.5f, 1f, 1f);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static Texture2D CreateMetadataTexture(List<VATAnimationLibrary.ClipEntry> clips)
        {
            var tex = new Texture2D(Mathf.Max(1, clips.Count), 1, TextureFormat.RGBAFloat, mipChain: false, linear: true)
            {
                name = "VAT_Metadata",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };

            var pixels = new Color[Mathf.Max(1, clips.Count)];
            for (int i = 0; i < clips.Count; i++)
            {
                var clip = clips[i];
                pixels[i] = new Color(clip.rowStart, clip.frameCount, clip.length, clip.meshIndex);
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static string SaveEXR(Texture2D texture, string folder, string fileName)
        {
            string path = Path.Combine(folder, fileName).Replace("\\", "/");
            byte[] exr = texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat | Texture2D.EXRFlags.CompressZIP);
            File.WriteAllBytes(path, exr);
            return path;
        }

        private static void ConfigureTextureImporter(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        private sealed class DisposableMesh : IDisposable
        {
            public Mesh Mesh { get; } = new Mesh();

            public void Dispose()
            {
                if (Mesh != null)
                    DestroyImmediate(Mesh);
            }
        }
    }
}
#endif
