using Cysharp.Threading.Tasks;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

namespace _Project._Code.Infrastructure.EntitiesExtensions
{
    public sealed class SubSceneAwaiter : MonoBehaviour
    {
        [SerializeField] private SubScene _subScene;
        
        public async UniTask WaitUntilSubSceneReady()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var em = world.EntityManager;

            await UniTask.WaitUntil(() =>
            {
                var sceneEntity = SceneSystem.GetSceneEntity(world.Unmanaged, _subScene.SceneGUID);
                if (sceneEntity == Entity.Null)
                    return false;
                if (!SceneSystem.IsSceneLoaded(world.Unmanaged, sceneEntity))
                    return false;
                using var query = em.CreateEntityQuery(typeof(SubSceneReady));
                return !query.IsEmptyIgnoreFilter;
            });
        }
    }
}