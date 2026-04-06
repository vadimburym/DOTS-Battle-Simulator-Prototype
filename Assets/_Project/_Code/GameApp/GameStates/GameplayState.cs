using System;
using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Keys;
using _Project._Code.GameApp.EntryPoints;
using _Project._Code.GameApp.Installers;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.EntitiesExtensions;
using _Project._Code.Infrastructure.StaticData;
using _Project._Code.Infrastructure.StaticData._Root;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using VContainer;
// ReSharper disable All

namespace _Project._Code.GameApp.GameStates
{
    public sealed class GameplayState : IGameState
    {
        public GameStateId GameStateId => GameStateId.Gameplay;
        private const string GAMEPLAY_NAME = "GameplayContext";
        private readonly string[] GAMEPLAY_ADDRESSABLE_LABELS = { "gameplay", "preload" };
        private const int GAMEPLAY_SCENE_IDX = 2;
        private readonly EntityPoolId[] PrefabsToLoad = { EntityPoolId.Footman, EntityPoolId.Orc };
        
        private readonly ILocalContextService _localContextService;
        private readonly IEcsContext _localEcsContext;
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IAddressableService _addressableService;
        private readonly IEntityPrefabService _entityPrefabService;
        private readonly UIInstallerService _uiInstallerService;
        
        private GameplayEntryPoint _entryPoint;
        private bool _isStarted;
        
        public GameplayState(
            ILocalContextService localContextService,
            ISceneLoadService sceneLoadService,
            IAddressableService addressableService,
            IEcsContext localEcsContext,
            IEntityPrefabService entityPrefabService,
            UIInstallerService uiInstallerService)
        {
            _localContextService = localContextService;
            _sceneLoadService = sceneLoadService;
            _addressableService = addressableService;
            _localEcsContext = localEcsContext;
            _entityPrefabService = entityPrefabService;
            _uiInstallerService = uiInstallerService;
        }
        
        public void Enter() => EnterAsync().Forget();
        private async UniTask EnterAsync()
        {
            await _entityPrefabService.LoadAsync(PrefabsToLoad); //TODO в StaticData определить EntityPoolId[] и передать
            await _addressableService.LoadObjectsByLabelsAsync(GAMEPLAY_ADDRESSABLE_LABELS, Addressables.MergeMode.Intersection);
            await _sceneLoadService.LoadSceneAsync(GAMEPLAY_SCENE_IDX);
            await _sceneLoadService.FindFirstComponentInRoots<SubSceneAwaiter>().WaitUntilSubSceneReady();
            var sceneInstaller = _sceneLoadService.FindFirstComponentInRoots<SceneInstaller>();
            
            _localContextService.WarmUp(BootstrapContext.Instance,builder => {
                GameplayInstaller.Register(builder);
                sceneInstaller.Register(builder);
                _uiInstallerService.GameplayInstaller.Register(builder);
                builder.Register<GameplayEntryPoint>(Lifetime.Singleton);
            }, GAMEPLAY_NAME);
            
            _localEcsContext.WarmUpSystems(_localContextService.Container, builder => {
                GameplayInstaller.RegisterEcsSystems(builder);
            });
            
            _entryPoint = _localContextService.Container.Resolve<GameplayEntryPoint>();
            _entryPoint.Start();
            GC.Collect();
            BootstrapContext.Instance.CameraGameObject.SetActive(false);
            _sceneLoadService.SetActiveCurrentScene();
            _isStarted = true;
        }

        public void Tick()
        {
            if (!_isStarted)
                return;
            _entryPoint.Tick();
        }

        public void LateTick()
        {
            if (!_isStarted)
                return;
            _entryPoint.LateTick();
        }
        
        public void Dispose()
        {
            _isStarted = false;
            _entryPoint.Dispose();
            _localEcsContext.CleanUpSystems();
            _localContextService.CleanUp();
        }
    }
}