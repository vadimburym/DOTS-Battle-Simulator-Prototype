using System.Collections.Generic;
using _Project._Code.GameApp;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public sealed class SceneLoadService : ISceneLoadService
    {
        private const int BOOTSTRAP_IDX = 0;
        
        public IReadOnlyList<GameObject> RootGameObjects => _rootGameObjects;
        public Scene CurrentScene => SceneManager.GetSceneByBuildIndex(_currentSceneBuildIDX);
        
        private readonly List<GameObject> _rootGameObjects = new();
        private int _currentSceneBuildIDX = -1;
        
        public async UniTask<Scene> LoadSceneAsync(int SceneBuildIDX)
        {
            await SceneManager.LoadSceneAsync(SceneBuildIDX, LoadSceneMode.Additive);
#if UNITY_EDITOR
            Debug.Log($"<color=green>Scene Loaded:</color> {SceneManager.GetSceneByBuildIndex(SceneBuildIDX).name}");
#endif
            if (_currentSceneBuildIDX != -1)
            {
                _rootGameObjects.Clear();
                await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(_currentSceneBuildIDX).name);
            }
            var scene = SceneManager.GetSceneByBuildIndex(SceneBuildIDX);
            scene.GetRootGameObjects(_rootGameObjects);
            _currentSceneBuildIDX = SceneBuildIDX;
            SceneManager.SetActiveScene(scene);
            return scene;
        }
        
        public T FindFirstComponentInRoots<T>() where T : Component
        {
            if (_currentSceneBuildIDX == -1)
                return null;
            for (int i = 0; i < _rootGameObjects.Count; i++)
            {
                if (_rootGameObjects[i].TryGetComponent(out T component))
                    return component;
            }
            return null;
        }
    }
}