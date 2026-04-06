using _Project._Code.Core.Contracts;
using _Project._Code.Infrastructure;

namespace _Project._Code.Locale
{
    public sealed class SceneEntryCameraSystem : IInit
    {
        private readonly ISceneEntryCameraProvider _sceneEntryCameraProvider;
        private readonly IMainCameraService _mainCameraService;
        
        public SceneEntryCameraSystem(
            ISceneEntryCameraProvider sceneEntryCameraProvider,
            IMainCameraService mainCameraService)
        {
            _sceneEntryCameraProvider = sceneEntryCameraProvider;
            _mainCameraService = mainCameraService;
        }
        
        public void Init()
        {
            _mainCameraService.SetupSceneEntry(_sceneEntryCameraProvider.GetEntryReference());
            _sceneEntryCameraProvider.DisposeReference();
        }
    }
}