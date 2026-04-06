using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public sealed class MainCameraService : IMainCameraService
    {
        public Transform MainCameraTransform => _mainCamera.transform;
        
        private readonly Camera _mainCamera = Camera.main;

        public void SetupSceneEntry(Transform sceneEntry)
        {
            _mainCamera.transform.position = sceneEntry.position;
            _mainCamera.transform.rotation = sceneEntry.rotation;
        }
    }
}