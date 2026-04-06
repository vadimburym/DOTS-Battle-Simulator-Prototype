using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public interface IMainCameraService
    {
        Camera MainCamera { get; }
        Transform MainCameraTransform { get; }
        void SetupSceneEntry(Transform sceneEntry);
    }
}
