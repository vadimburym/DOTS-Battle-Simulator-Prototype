using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public interface IMainCameraService
    {
        Transform MainCameraTransform { get; }
        void SetupSceneEntry(Transform sceneEntry);
    }
}