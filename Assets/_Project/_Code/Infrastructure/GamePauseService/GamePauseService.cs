using R3;

namespace _Project._Code.Infrastructure
{
    public sealed class GamePauseService : IGamePauseService
    {
        public ReadOnlyReactiveProperty<bool> IsGamePaused => _isGamePaused;
        private readonly ReactiveProperty<bool> _isGamePaused = new();
        
        public void SetGamePaused(bool value) => _isGamePaused.Value = value;
    }
}