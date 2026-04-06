using UnityEngine;
using VContainer;

namespace _Project._Code.Core.Abstractions
{
    public abstract class MonoInstaller : MonoBehaviour
    {
        public abstract void Register(IContainerBuilder builder);
    }
}