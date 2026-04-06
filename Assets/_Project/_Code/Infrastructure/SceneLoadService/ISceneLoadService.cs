using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project._Code.Infrastructure
{
    public interface ISceneLoadService
    {
        IReadOnlyList<GameObject> RootGameObjects { get; }
        Scene CurrentScene { get; }
        UniTask<Scene> LoadSceneAsync(int SceneBuildIDX);
        T FindFirstComponentInRoots<T>() where T : Component;
    }
}