using _Project._Code.Locale;
using UnityEngine;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    public sealed class SceneInstaller : MonoBehaviour
    {
        [SerializeField] private TransformProvider _transformProvider;

        public void Register(IContainerBuilder builder)
        {
            builder.RegisterInstance(_transformProvider).As<ITransformProvider>();
        }
    }
}