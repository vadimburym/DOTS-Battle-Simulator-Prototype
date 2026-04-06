using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace _Project.Code.EditorTools.Editor
{
    public sealed class StaticDataPage<TStaticData, TConfig> : IDisposable 
        where TStaticData : ScriptableObject, IEditorStaticData
        where TConfig : ScriptableObject
    {
        [Title("Static Data")]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [HideLabel]
        [TabGroup("Pipeline")]
        public TStaticData _staticData;
        private readonly string PATH;
            
        public StaticDataPage(TStaticData staticData, string PATH)
        {
            _staticData = staticData;
            this.PATH = PATH;
            Config = ScriptableObject.CreateInstance<TConfig>();
            ConfigName = "New Config";
        }
        
        [TabGroup("New Data")]
        public string ConfigName;
        
        [TabGroup("New Data")]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public TConfig Config;
        
        [TabGroup("New Data")]
        [Button(ButtonSizes.Large), GUIColor("green")]
        public void CreateNewData()
        {
            string path = $"{PATH}/{ConfigName}.asset";

            AssetDatabase.CreateAsset(Config, path);
            AssetDatabase.SaveAssets();
            
            if (!_staticData.ContainsConfig(Config))
            {
                _staticData.AddConfig(Config);
                EditorUtility.SetDirty((ScriptableObject)_staticData);
                AssetDatabase.SaveAssets();
            }
            
            Config = ScriptableObject.CreateInstance<TConfig>();
        }

        void IDisposable.Dispose()
        {
            UnityEngine.Object.DestroyImmediate(Config);
        }
    }
}