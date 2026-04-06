using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project._Code.Locale
{
    public sealed class SceneEntryCameraProvider : MonoBehaviour, ISceneEntryCameraProvider
    {
        [SerializeField] private Camera _cameraReference;
        
        public Transform GetEntryReference() => _cameraReference.transform;
        
        public void DisposeReference() => Object.Destroy(_cameraReference.gameObject);
    }
}