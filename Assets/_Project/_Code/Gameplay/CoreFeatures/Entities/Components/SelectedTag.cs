using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct SelectedTag : IComponentData, IEnableableComponent
    {
        public Entity SelectedView;
        public float ShowScale;
    }
}