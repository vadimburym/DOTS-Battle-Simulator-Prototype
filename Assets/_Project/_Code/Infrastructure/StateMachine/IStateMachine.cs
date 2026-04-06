using _Project._Code.Core.Keys;

namespace _Project._Code.Infrastructure
{
    public interface IStateMachine
    {
        GameStateId CurrentStateId { get; }
        void Enter(GameStateId gameStateId);
        void Tick();
        void LateTick();
        void Dispose();
    }
}