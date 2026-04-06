using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project._Code.Infrastructure.Audio
{
    public sealed class AudioSourcePool : MonoBehaviour
    {
        private readonly List<AudioSourceAdapter> _poolObjects = new();
        
        [SerializeField] private AudioSourceAdapter _audioSourcePrefab;
        [SerializeField] private Transform _poolRoot;
        [SerializeField] private int _initCount;
        
        private void Awake()
        {
            for (int i = 0; i < _initCount; i++)
            {
                var source = Object.Instantiate(_audioSourcePrefab, _poolRoot);
                _poolObjects.Add(source);
            }
        }
        
        public AudioSourceAdapter GetAudioSource()
        {
            if (TryGetFreeAudioSource(out var source))
            {
                source.SetParent(_poolRoot, true);
                return source;
            }
            else
            {
                source = Object.Instantiate(_audioSourcePrefab, _poolRoot);
                _poolObjects.Add(source);
                return source;
            }
        }

        private bool TryGetFreeAudioSource(out AudioSourceAdapter source)
        {
            for (int i = 0; i < _poolObjects.Count; i++)
            {
                var item = _poolObjects[i];
                if (!item.IsPlaying)
                {
                    source = item;
                    return true;
                }
            }
            source = null;
            return false;
        }
    }
}