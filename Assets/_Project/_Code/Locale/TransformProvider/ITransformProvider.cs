using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Locale
{
    public interface ITransformProvider
    {
        Transform GetTransform(TransformId name);
        bool TryGetTransform(TransformId name, out Transform transform);
    }
}