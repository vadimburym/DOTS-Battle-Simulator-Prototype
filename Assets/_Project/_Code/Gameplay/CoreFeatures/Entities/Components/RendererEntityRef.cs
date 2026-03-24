using Unity.Entities;

namespace VATDots
{
    public struct RendererEntityRef : IComponentData
    {
        public Entity Value;
    }
}