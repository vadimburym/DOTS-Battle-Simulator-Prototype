using System;
using Sirenix.OdinInspector;

namespace _Project._Code.Tools.OdinExtensions
{
    public class ColoredFoldoutGroupAttribute : PropertyGroupAttribute
    {
        public float R, G, B, A;

        public ColoredFoldoutGroupAttribute(string path)
            : base(path)
        {
        }

        public ColoredFoldoutGroupAttribute(string path, float r, float g, float b, float a = 1f)
            : base(path)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var otherAttr = (ColoredFoldoutGroupAttribute)other;

            this.R = Math.Max(otherAttr.R, this.R);
            this.G = Math.Max(otherAttr.G, this.G);
            this.B = Math.Max(otherAttr.B, this.B);
            this.A = Math.Max(otherAttr.A, this.A);
        }
    }
}