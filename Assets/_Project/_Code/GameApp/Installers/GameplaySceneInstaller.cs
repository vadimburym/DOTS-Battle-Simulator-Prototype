using _Project._Code.Core.Abstractions;
using _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours;
using _Project._Code.Locale;
using _Project._Code.Locale.EdgeScrollCamera;
using UnityEngine;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    public sealed class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private TransformProvider _transformProvider;
        [SerializeField] private SelectionAreaProvider _selectionAreaProvider;
        [SerializeField] private EdgeScrollCameraProvider _edgeScrollCameraProvider;
        [SerializeField] private SceneEntryCameraProvider _sceneEntryCameraProvider;
        
        public override void Register(IContainerBuilder builder)
        {
            builder.RegisterInstance(_transformProvider).As<ITransformProvider>();
            builder.RegisterInstance(_selectionAreaProvider).As<ISelectionAreaProvider>();
            builder.RegisterInstance(_edgeScrollCameraProvider).As<IEdgeScrollCameraProvider>();
            builder.RegisterInstance(_sceneEntryCameraProvider).As<ISceneEntryCameraProvider>();
        }
    }
}