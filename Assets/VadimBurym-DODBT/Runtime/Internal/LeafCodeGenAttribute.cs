using System;

namespace VadimBurym.DodBehaviourTree
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class LeafCodeGenAttribute : Attribute
    {
        public byte Id;

        public LeafCodeGenAttribute(byte id)
        {
            Id = id;
        }
    }
}
