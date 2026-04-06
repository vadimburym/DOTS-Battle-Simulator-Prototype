using System;
using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Keys;
using _Project._Code.GameApp.EntryPoints;
using _Project._Code.GameApp.Installers;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.EntitiesExtensions;
using _Project._Code.Infrastructure.LoadingCurtainProvider;
using _Project._Code.Infrastructure.StaticData._Root;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using VContainer;
// ReSharper disable All

namespace _Project._Code.GameApp.GameStates
{
    public sealed class MetaState : IGameState
    {
        GameStateId IGameState.GameStateId => GameStateId.Meta;
        
        private const string META_NAME = "MetaContext";
        private readonly string[] META_ADDRESSABLE_LABELS = { "meta", "preload" };
        private const int META_SCENE_IDX = 1;
        private readonly EntityPoolId[] PrefabsToLoad = { EntityPoolId.Footman };
        
        private readonly ILocalContextService _localContextService;
        private readonly IEcsContext _localEcsContext;
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IAddressableService _addressableService;
        private readonly IEntityPrefabService _entityPrefabService;
        private readonly UIInstallerService _uiInstallerService;
        private readonly ILoadingCurtainProvider _loadingCurtainProvider;
        
        private MetaEntryPoint _entryPoint;
        private bool _isStarted;
        
        public MetaState(
            ILocalContextService localContextService,
            ISceneLoadService sceneLoadService,
            IAddressableService addressableService,
            IEcsContext localEcsContext,
            IEntityPrefabService entityPrefabService,
            UIInstallerService uiInstallerService,
            ILoadingCurtainProvider loadingCurtainProvider)
        {
            _localContextService = localContextService;
            _sceneLoadService = sceneLoadService;
            _addressableService = addressableService;
            _localEcsContext = localEcsContext;
            _entityPrefabService = entityPrefabService;
            _uiInstallerService = uiInstallerService;
            _loadingCurtainProvider = loadingCurtainProvider;
        }
        
        public void Enter() => EnterAsync().Forget();
        private async UniTask EnterAsync()
        {
            await _entityPrefabService.LoadAsync(PrefabsToLoad);
            await _addressableService.LoadObjectsByLabelsAsync(META_ADDRESSABLE_LABELS, Addressables.MergeMode.Intersection);
            await _sceneLoadService.LoadSceneAsync(META_SCENE_IDX);
            await _sceneLoadService.FindFirstComponentInRoots<SubSceneAwaiter>().WaitUntilSubSceneReady();
            var sceneInstaller = _sceneLoadService.FindFirstComponentInRoots<MetaSceneInstaller>();
            
            _localContextService.WarmUp(BootstrapContext.Instance,builder => {
                MetaInstaller.Register(builder);
                sceneInstaller.Register(builder);
                _uiInstallerService.MetaUIInstaller.Register(builder);
                builder.Register<MetaEntryPoint>(Lifetime.Singleton);
            }, META_NAME);
            
            _localEcsContext.WarmUpSystems(_localContextService.Container, builder => {
                MetaInstaller.RegisterEcsSystems(builder);
            });
            
            _entryPoint = _localContextService.Container.Resolve<MetaEntryPoint>();
            _entryPoint.Start();
            GC.Collect();
            _loadingCurtainProvider.Hide();
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
            return;
        }

        public void Dispose()
        {
            _loadingCurtainProvider.Show();
            _isStarted = false;
            _entryPoint.Dispose();
            _localEcsContext.CleanUpSystems();
            _localContextService.CleanUp();
            _entityPrefabService.Unload(PrefabsToLoad);
            _addressableService.ReleaseByLabels(META_ADDRESSABLE_LABELS, Addressables.MergeMode.Intersection);
        }
    }
}