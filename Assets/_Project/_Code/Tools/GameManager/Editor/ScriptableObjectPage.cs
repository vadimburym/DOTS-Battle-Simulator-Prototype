using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Code.EditorTools.Editor
{
    public sealed class ScriptableObjectPage<T> where T : ScriptableObject
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [HideLabel]
        public T _staticData;

        public ScriptableObjectPage(T staticData)
        {
            _staticData = staticData;
        }
    }
}