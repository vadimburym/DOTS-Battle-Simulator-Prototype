using _Project._Code.Core.Abstractions;
using UnityEngine;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    [CreateAssetMenu(fileName = "UIMetaInstaller", menuName = "_Project/Installers/new UIMetaInstaller")]
    public sealed class UIMetaInstaller : ScriptableObjectInstaller
    {
        public override void Register(IContainerBuilder builder)
        {
        }
    }
}