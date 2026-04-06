using UnityEngine;

namespace _Project.Code.EditorTools
{
    #if UNITY_EDITOR
    public interface IEditorStaticData
    {
        void AddConfig(object config);
        bool ContainsConfig(object config);
        void RemoveConfig(object config);
    }
    #endif
}