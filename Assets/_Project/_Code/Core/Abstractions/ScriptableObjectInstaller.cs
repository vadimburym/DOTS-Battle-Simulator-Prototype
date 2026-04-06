using UnityEngine;
using VContainer;

namespace _Project._Code.Core.Abstractions
{
    public abstract class ScriptableObjectInstaller : ScriptableObject
    {
        public abstract void Register(IContainerBuilder builder);
    }
}