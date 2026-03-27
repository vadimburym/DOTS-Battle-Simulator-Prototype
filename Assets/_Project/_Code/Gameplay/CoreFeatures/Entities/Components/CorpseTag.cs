using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct CorpseTag : IComponentData, IEnableableComponent
    {
        public float Time;
    }
}