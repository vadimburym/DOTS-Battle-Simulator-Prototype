using Cysharp.Threading.Tasks;

namespace _Project._Code.Infrastructure
{
    public interface ISaveRepository
    {
        UniTask Load();
        UniTask Save();
        void SetData<T>(T data);
        bool TryGetData<T>(out T data);
        void Delete();
    }
}