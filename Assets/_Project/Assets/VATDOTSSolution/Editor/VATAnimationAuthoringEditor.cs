#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VATDots.Editor
{
    [CustomEditor(typeof(VATAnimationAuthoring))]
    public sealed class VATAnimationAuthoringEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var authoring = (VATAnimationAuthoring)target;
            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("VAT DOTS Tools", EditorStyles.boldLabel);

                int meshIndex = authoring.GetDetectedMeshIndex();
                EditorGUILayout.LabelField("Detected Mesh Index", meshIndex.ToString());

                if (GUILayout.Button("Apply library to shared material"))
                {
                    Undo.RecordObject(authoring, "Apply VAT Library");
                    authoring.ApplyLibraryToSharedMaterial();
                    EditorUtility.SetDirty(authoring);
                }

                DrawClipPopup(authoring, meshIndex);
            }
        }

        private static void DrawClipPopup(VATAnimationAuthoring authoring, int meshIndex)
        {
            if (authoring.library == null)
            {
                EditorGUILayout.HelpBox("Assign VATAnimationLibrary to get clip popup and runtime test controls.", MessageType.Info);
                return;
            }

            if (meshIndex < 0)
            {
                EditorGUILayout.HelpBox("Current mesh is not found in the library. Make sure this renderer uses one of the baked meshes.", MessageType.Warning);
                return;
            }

            int[] clipIndices = authoring.library.GetClipIndicesForMesh(meshIndex);
            if (clipIndices.Length == 0)
            {
                EditorGUILayout.HelpBox("No clips exist in the library for this mesh.", MessageType.Warning);
                return;
            }

            var labels = new List<string>(clipIndices.Length);
            int currentSelection = 0;
            for (int i = 0; i < clipIndices.Length; i++)
            {
                int clipIndex = clipIndices[i];
                labels.Add($"[{clipIndex}] {authoring.library.GetClipDisplayName(clipIndex)}");
                if (clipIndex == authoring.initialClipIndex)
                    currentSelection = i;
            }

            int nextSelection = EditorGUILayout.Popup("Initial Clip", currentSelection, labels.ToArray());
            if (nextSelection != currentSelection)
            {
                Undo.RecordObject(authoring, "Change Initial VAT Clip");
                authoring.initialClipIndex = clipIndices[nextSelection];
                EditorUtility.SetDirty(authoring);
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(4f);
                if (GUILayout.Button("Play selected clip on runtime entity"))
                {
                    bool ok = authoring.RequestPlayClipOnRuntimeEntity(
                        clipIndices[nextSelection],
                        authoring.defaultTransitionDuration,
                        authoring.initialNormalizedTime,
                        restartIfSame: true);

                    if (!ok)
                        Debug.LogWarning("[VAT DOTS] Runtime entity for this authoring object was not found.", authoring);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to send a clip change request to the baked entity from this inspector.", MessageType.None);
            }
        }
    }
}
#endif
