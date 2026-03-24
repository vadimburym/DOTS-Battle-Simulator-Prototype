#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VATDots.Editor
{
    [CustomEditor(typeof(VATBaker))]
    public sealed class VATBakerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Bake", EditorStyles.boldLabel);
                if (GUILayout.Button("Bake VAT Atlas", GUILayout.Height(32f)))
                    ((VATBaker)target).Bake();
            }
        }
    }
}
#endif