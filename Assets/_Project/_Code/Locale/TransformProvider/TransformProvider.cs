using System;
using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Locale
{
    public sealed class TransformProvider : MonoBehaviour, ITransformProvider
    {
        [SerializeField] private TransformInfo[] _transforms;
        
        public Transform GetTransform(TransformId name)
        {
            for (int i = 0; i < _transforms.Length; i++)
            {
                if (_transforms[i].Name == name)
                    return _transforms[i].Transform;
            }
            return null;
        }

        public bool TryGetTransform(TransformId name, out Transform transform)
        {
            for (int i = 0; i < _transforms.Length; i++)
            {
                if (_transforms[i].Name == name)
                {
                    transform = _transforms[i].Transform;
                    return true;
                }
            }
            transform = null;
            return false;
        }
        
        [Serializable]
        private struct TransformInfo
        {
            public TransformId Name;
            public Transform Transform;
        }
    }
}