using UnityEngine;

namespace _Project.Code.EditorTools
{
    #if UNITY_EDITOR
    public interface IEditorDataPreview
    {
        Sprite EditorPreview { get; }
    }
    #endif
}