using System.Collections.Generic;
using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public sealed class StateMachine : IStateMachine
    {
        public GameStateId CurrentStateId => _currentState.GameStateId;

        private readonly Dictionary<GameStateId, IGameState> _states = new();
        private IGameState _currentState;

        public StateMachine(
            IReadOnlyList<IGameState> states)
        {
            for (int i = 0; i < states.Count; i++)
                _states.Add(states[i].GameStateId, states[i]);
        }

        public void Enter(GameStateId gameStateId)
        {
            _currentState?.Dispose();
            if (!_states.TryGetValue(gameStateId, out var targetState))
                throw new KeyNotFoundException($"State with id {gameStateId} does not exist");
            _currentState = targetState;
#if UNITY_EDITOR
            Debug.Log($"<color=green>Game State Machine:</color> {targetState.GetType()} <color=green>Entered!</color>");
#endif
            targetState.Enter();
        }

        public void Tick() => _currentState?.Tick();

        public void LateTick() => _currentState?.LateTick();

        public void Dispose()
        {
            _currentState?.Dispose();
            _currentState = null;
        }
    }
}
