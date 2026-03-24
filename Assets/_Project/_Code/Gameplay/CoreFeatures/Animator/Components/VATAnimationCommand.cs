using Unity.Entities;

namespace VATDots
{
    public struct VATAnimationCommand : IComponentData
    {
        public int RequestedClipIndex;
        public float TransitionDuration;
        public float StartNormalizedTime;
        public byte RestartIfSame;
    }
}