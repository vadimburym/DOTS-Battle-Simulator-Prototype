using System;
using System.Collections.Generic;
using _Project._Code.Core.Contracts;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Locale;
using R3;

namespace _Project._Code.GameApp.EntryPoints
{
    public sealed class GameplayEntryPoint
    {
        private readonly IReadOnlyList<IWarmUp> _warmUp;
        private readonly IReadOnlyList<IInit> _init;
        private readonly IReadOnlyList<ITick> _tick;
        private readonly IReadOnlyList<IPausableTick> _pausableTick;
        private readonly IReadOnlyList<ILateTick> _lateTick;
        private readonly IReadOnlyList<IDispose> _dispose;
        private readonly IGamePauseService _gamePauseService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IEcsContext _ecsContext;

        private IDisposable _disposable; 
        
        public GameplayEntryPoint(
            ISaveLoadService saveLoadService,
            IGamePauseService gamePauseService,
            IEcsContext ecsContext,
            IReadOnlyList<IWarmUp> warmUp,
            IReadOnlyList<IInit> init,
            IReadOnlyList<ITick> tick,
            IReadOnlyList<IPausableTick> pausableTick,
            IReadOnlyList<ILateTick> lateTick,
            IReadOnlyList<IDispose> dispose)
        {
            _saveLoadService = saveLoadService;
            _gamePauseService = gamePauseService;
            _ecsContext = ecsContext;
            _warmUp = warmUp;
            _init = init;
            _tick = tick;
            _pausableTick = pausableTick;
            _lateTick = lateTick;
            _dispose = dispose;
        }

        public void Start()
        {
            for (int i = 0; i < _warmUp.Count; i++)
                _warmUp[i].WarmUp();
            for (int i = 0; i < _init.Count; i++)
                _init[i].Init();
            _saveLoadService.Load();
            _gamePauseService.SetGamePaused(false);
            _disposable = _gamePauseService.IsGamePaused.Subscribe(value => _ecsContext.EnableSystems(!value));
        }

        public void Tick()
        {
            for (int i = 0; i < _tick.Count; i++)
                _tick[i].Tick();
            if (!_gamePauseService.IsGamePaused.CurrentValue)
            {
                for (int i = 0; i < _pausableTick.Count; i++)
                    _pausableTick[i].PausableTick();
            }
        }

        public void LateTick()
        {
            for (int i = 0; i < _lateTick.Count; i++)
                _lateTick[i].LateTick();
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
            for (int i = 0; i < _dispose.Count; i++)
                _dispose[i].Dispose();
        }
    }
}