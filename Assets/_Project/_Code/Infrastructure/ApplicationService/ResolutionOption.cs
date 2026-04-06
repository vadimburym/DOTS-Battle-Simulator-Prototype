using System;
using UnityEngine;

namespace _Project._Code.Infrastructure.ApplicationService
{
    [Serializable]
    public readonly struct ResolutionOption : IEquatable<ResolutionOption>
    {
        public int Width { get; }
        public int Height { get; }
        public uint RefreshRateNumerator { get; }
        public uint RefreshRateDenominator { get; }

        public float RefreshRate =>
            RefreshRateDenominator == 0 ? 0f : (float)RefreshRateNumerator / RefreshRateDenominator;

        public ResolutionOption(int width, int height, uint refreshRateNumerator, uint refreshRateDenominator)
        {
            Width = width;
            Height = height;
            RefreshRateNumerator = refreshRateNumerator;
            RefreshRateDenominator = refreshRateDenominator == 0 ? 1u : refreshRateDenominator;
        }

        public RefreshRate ToRefreshRate()
        {
            return new RefreshRate
            {
                numerator = RefreshRateNumerator,
                denominator = RefreshRateDenominator
            };
        }

        public static ResolutionOption FromUnity(Resolution resolution)
        {
            return new ResolutionOption(
                resolution.width,
                resolution.height,
                resolution.refreshRateRatio.numerator,
                resolution.refreshRateRatio.denominator);
        }

        public static ResolutionOption FromCurrentResolution()
        {
            return FromUnity(Screen.currentResolution);
        }

        public bool Equals(ResolutionOption other)
        {
            return Width == other.Width
                   && Height == other.Height
                   && RefreshRateNumerator == other.RefreshRateNumerator
                   && RefreshRateDenominator == other.RefreshRateDenominator;
        }

        public override bool Equals(object obj) => obj is ResolutionOption other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Width;
                hash = hash * 397 ^ Height;
                hash = hash * 397 ^ (int)RefreshRateNumerator;
                hash = hash * 397 ^ (int)RefreshRateDenominator;
                return hash;
            }
        }

        public override string ToString() => $"{Width}x{Height} @{RefreshRate:0.#}Hz";
    }
}