using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace _Project._Code.Core.Abstractions
{
    public interface ISaveStrategy
    {
        UniTask<Dictionary<string, string>> LoadRepository();
        UniTask SaveRepository(Dictionary<string, string> repository);
        void DeleteRepository();
    }
}