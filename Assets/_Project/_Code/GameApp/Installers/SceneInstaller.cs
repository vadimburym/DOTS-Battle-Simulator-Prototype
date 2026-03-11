using _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours;
using _Project._Code.Locale;
using UnityEngine;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    public sealed class SceneInstaller : MonoBehaviour
    {
        [SerializeField] private TransformProvider _transformProvider;
        [SerializeField] private SelectionAreaProvider _selectionAreaProvider;
        
        public void Register(IContainerBuilder builder)
        {
            builder.RegisterInstance(_transformProvider).As<ITransformProvider>();
            builder.RegisterInstance(_selectionAreaProvider).As<ISelectionAreaProvider>();
        }
    }
}