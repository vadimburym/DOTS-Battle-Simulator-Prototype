using R3;

namespace _Project._Code.Infrastructure
{
    public interface IGamePauseService
    {
        ReadOnlyReactiveProperty<bool> IsGamePaused { get; }
        void SetGamePaused(bool value);
    }
}