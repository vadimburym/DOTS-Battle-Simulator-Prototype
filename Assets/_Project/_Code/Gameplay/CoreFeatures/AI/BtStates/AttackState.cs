using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct AttackState : IComponentData
    {
        public Entity Owner;
        public Entity Target;
        public float RemainingTime;
    }
}