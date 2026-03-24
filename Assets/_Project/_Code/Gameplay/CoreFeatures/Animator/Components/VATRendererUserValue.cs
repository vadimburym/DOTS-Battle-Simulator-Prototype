using Unity.Entities;
using Unity.Rendering;

namespace VATDots
{
    [MaterialProperty("unity_RendererUserValuesPropertyEntry")]
    public struct VATRendererUserValue : IComponentData
    {
        public uint Value;
    }
}