using System.Runtime.CompilerServices;
using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure;

namespace VATDots
{
    public static class VATIndexUtils
    {
        private const byte AnimationsLength = 3;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAnimationIndex(UnitId unitId, AnimationId animation)
        {
            return ((byte)unitId - 1) * AnimationsLength + (byte)animation;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAnimationIndex(byte unitId, AnimationId animation)
        {
            return (unitId - 1) * AnimationsLength + (byte)animation;
        }
    }
}