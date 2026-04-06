using UnityEngine;

namespace _Project._Code.Locale
{
    public interface ISceneEntryCameraProvider
    {
        Transform GetEntryReference();
        void DisposeReference();
    }
}