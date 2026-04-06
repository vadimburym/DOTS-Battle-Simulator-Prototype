using _Project._Code.Core.Keys;

namespace _Project._Code.Core.Abstractions
{
    public interface IGameState
    {
        GameStateId GameStateId { get; }
        void Enter();
        void Tick();
        void LateTick();
        void Dispose();
    }
}