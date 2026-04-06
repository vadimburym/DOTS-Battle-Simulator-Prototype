using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Contracts;
using _Project._Code.Locale;
using UnityEngine;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    public sealed class MetaSceneInstaller : MonoInstaller
    {
        [SerializeField] private TransformProvider _transformProvider;
        [SerializeField] private SceneEntryCameraProvider _sceneEntryCameraProvider;
        
        public override void Register(IContainerBuilder builder)
        {
            builder.RegisterInstance(_transformProvider).As<ITransformProvider>();
            builder.RegisterInstance(_sceneEntryCameraProvider).As<ISceneEntryCameraProvider>();
        }
    }
}