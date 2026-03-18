using System.IO;
using UnityEditor;

namespace VadimBurym.DodBehaviourTree.Editor
{
    public static class LeafTemplateCreator
    {
        [MenuItem("Tools/VadimBurym/New Leaf")]
        public static void CreateNewLeaf()
        {
            string folder = "Assets/VadimBurym-DODBT/Local/Leafs";
            Directory.CreateDirectory(folder);
            
            string leafName = "NewLeaf"; 
            string filePath = Path.Combine(folder, $"{leafName}.cs");
            
            int counter = 1;
            while (File.Exists(filePath))
            {
                leafName = $"NewLeaf{counter}";
                filePath = Path.Combine(folder, $"{leafName}.cs");
                counter++;
            }
            
            string template = @"using Unity.Burst;
using Unity.Entities;
using System;

namespace VadimBurym.DodBehaviourTree.Leafs
{
    [Serializable]
    [BurstCompile]
    [LeafCodeGen(0xFF)] //TODO: Implement LeafId (byte)
    public struct NewLeaf : ILeaf
    {
        public LeafData GetCompiledData()
        {
            return new LeafData
            {
                // TODO: fill leaf data
            };
        }
        
        [BurstCompile]
        public static NodeStatus OnTick(ref Entity agent, in LeafData leaf, ref LeafStateElement state, in EmptyContextMock context)
        {
            // TODO: Implement your context and tick logic
            return NodeStatus.Failure;
        }

        [BurstCompile]
        public static void OnEnter(ref Entity agent, in LeafData leaf, ref LeafStateElement state, in EmptyContextMock context)
        {
            // TODO: Implement your context and enter logic
        }

        [BurstCompile]
        public static void OnExit(ref Entity agent, in LeafData leaf, ref LeafStateElement state, in EmptyContextMock context)
        {
            // TODO: Implement your context and exit logic
        }

        [BurstCompile]
        public static void OnAbort(ref Entity agent, in LeafData leaf, ref LeafStateElement state, in EmptyContextMock context)
        {
            // TODO: Implement your context and abort logic
        }
    }
}";
            File.WriteAllText(filePath, template);
            AssetDatabase.Refresh();
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath));
            UnityEngine.Debug.Log($"Leaf template created: {filePath}");
        }
    }
}