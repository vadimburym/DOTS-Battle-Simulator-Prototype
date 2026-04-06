using System.Collections.Generic;
using _Project._Code.Core.Contracts;

namespace _Project._Code.GameApp.EntryPoints
{
    public sealed class MetaEntryPoint
    {
        private readonly IReadOnlyList<IWarmUp> _warmUp;
        private readonly IReadOnlyList<IInit> _init;
        private readonly IReadOnlyList<ITick> _tick;
        private readonly IReadOnlyList<IDispose> _dispose;
        
        public MetaEntryPoint(
            IReadOnlyList<IWarmUp> warmUp,
            IReadOnlyList<IInit> init,
            IReadOnlyList<ITick> tick,
            IReadOnlyList<IDispose> dispose)
        {
            _warmUp = warmUp;
            _init = init;
            _tick = tick;
            _dispose = dispose;
        }
        
        public void Start()
        {
            for (int i = 0; i < _warmUp.Count; i++)
                _warmUp[i].WarmUp();
            for (int i = 0; i < _init.Count; i++)
                _init[i].Init();
        }

        public void Tick()
        {
            for (int i = 0; i < _tick.Count; i++)
                _tick[i].Tick();
        }
        
        public void Dispose()
        {
            for (int i = 0; i < _dispose.Count; i++)
                _dispose[i].Dispose();
        }
    }
}