using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace _Project._Code.Tools.VatBaker
{
    public sealed class VertexAnimationTextureBaker : MonoBehaviour
    {
        [System.Serializable]
        public struct MeshAnimations
        {
            public string meshName;
            public SkinnedMeshRenderer skinnedMesh;
            public List<AnimationClip> animations;
        }

        public List<MeshAnimations> meshes;
        public int framesPerAnimation = 6;

        [Button("Bake VAT")]
        public void Bake()
        {
            // 1. Находим MaxVertexCount
            int maxVertexCount = 0;
            foreach (var meshAnim in meshes)
            {
                int vc = meshAnim.skinnedMesh.sharedMesh.vertexCount;
                if (vc > maxVertexCount) maxVertexCount = vc;
            }

            // 2. Общие данные
            int totalAnimations = 0;
            foreach (var meshAnim in meshes)
                totalAnimations += meshAnim.animations.Count;

            int texWidth = maxVertexCount;
            int texHeight = framesPerAnimation * totalAnimations;

            Texture2D vatTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false, true);
            vatTex.wrapMode = TextureWrapMode.Clamp;

            Mesh bakedMesh = new Mesh();
            int animOffset = 0;

            foreach (var meshAnim in meshes)
            {
                int vertexCount = meshAnim.skinnedMesh.sharedMesh.vertexCount;

                foreach (var clip in meshAnim.animations)
                {
                    float frameTime = clip.length / framesPerAnimation;

                    for (int f = 0; f < framesPerAnimation; f++)
                    {
                        float time = f * frameTime;
                        clip.SampleAnimation(meshAnim.skinnedMesh.gameObject, time);
                        meshAnim.skinnedMesh.BakeMesh(bakedMesh);
                        Vector3[] verts = bakedMesh.vertices;

                        for (int v = 0; v < maxVertexCount; v++)
                        {
                            Color col = (v < verts.Length) ? new Color(verts[v].x, verts[v].y, verts[v].z, 1) : Color.black;
                            vatTex.SetPixel(v, f + animOffset * framesPerAnimation, col);
                        }
                    }

                    animOffset++;
                }
            }

            vatTex.Apply();
            AssetDatabase.CreateAsset(vatTex, "Assets/_Project/Assets/VertexAnimationTexture/VAT.asset");
            AssetDatabase.SaveAssets();
            Debug.Log($"VAT baked! MaxVertexCount={maxVertexCount}, TotalAnimations={totalAnimations}, Texture={texWidth}x{texHeight}");
        }
    }
}