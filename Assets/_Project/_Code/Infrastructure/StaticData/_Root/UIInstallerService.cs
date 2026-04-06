using _Project._Code.GameApp.Installers;
using UnityEngine;

namespace _Project._Code.Infrastructure.StaticData._Root
{
    [CreateAssetMenu(fileName = nameof(UIInstallerService), menuName = "_Project/Infrastructure/New UIInstallerService")]
    public sealed class UIInstallerService : ScriptableObject
    {
        public UIGameplayInstaller GameplayInstaller;
        public UIMetaInstaller MetaInstaller;
    }
}