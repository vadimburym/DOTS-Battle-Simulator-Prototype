using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct Selected : IComponentData, IEnableableComponent
    {
        public Entity SelectedView;
        public float ShowScale;
    }
}