using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace VadimBurym.DodBehaviourTree.Generator
{
    public static class LeafGenerator
    {
        [MenuItem("Tools/VadimBurym/CodeGen Leafs")]
        private static void Generate()
        {
            string folder = "Assets/VadimBurym-DODBT/Local/Generated";
            
            if (AssetDatabase.IsValidFolder(folder))
            {
                var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folder });

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);

                    if (path.EndsWith(".cs"))
                    {
                        AssetDatabase.DeleteAsset(path);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(folder);
            }

            var allLeafs = GetLeafs();
            if (allLeafs.Count == 0)
            {
                Debug.Log("No leafs with LeafCodeGenerationAttribute found.");
                return;
            }

            // Группировка по ContextType (определенному по методу)
            var grouped = allLeafs.GroupBy(l => l.ContextType);

            foreach (var group in grouped)
            {
                GenerateTables(folder, group.Key, group.ToList());
            }

            AssetDatabase.Refresh();
        }

        private static List<LeafInfo> GetLeafs()
        {
            var result = new List<LeafInfo>();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                });

            foreach (var t in types)
            {
                if (!t.IsValueType) continue;

                var attr = t.GetCustomAttribute<LeafCodeGenAttribute>();
                if (attr == null) continue;

                if (attr.Id == 0xFF)
                {
                    Debug.LogError($"{t.FullName}: LeafCodeGeneration has ID = 0xFF. You must assign a valid unique ID.");
                    continue;
                }
                
                // Ищем методы OnTick, OnEnter, OnExit, OnAbort
                var methods = new Dictionary<string, MethodInfo>();
                string[] required = { "OnTick", "OnEnter", "OnExit", "OnAbort" };
                bool valid = true;
                Type contextType = null;

                foreach (var name in required)
                {
                    var m = t.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (m == null)
                    {
                        Debug.LogError($"{t.FullName}: missing static method {name}");
                        valid = false;
                        continue;
                    }

                    // Определяем контекст по 4-му параметру
                    var ps = m.GetParameters();
                    if (ps.Length != 4)
                    {
                        Debug.LogError($"{t.FullName}.{name}: expected 4 parameters, found {ps.Length}");
                        valid = false;
                        continue;
                    }

                    if (name == "OnTick" && m.ReturnType != typeof(NodeStatus))
                    {
                        Debug.LogError($"{t.FullName}.{name}: return type should be NodeStatus, found {m.ReturnType.Name}");
                        valid = false;
                    }

                    if (name != "OnTick" && m.ReturnType != typeof(void))
                    {
                        Debug.LogError($"{t.FullName}.{name}: return type should be void, found {m.ReturnType.Name}");
                        valid = false;
                    }

                    var p0 = ps[0];
                    if (!(p0.ParameterType.IsByRef && !p0.IsIn && p0.ParameterType.GetElementType() == typeof(Entity)))
                    {
                        Debug.LogError($"{t.FullName}.{name}: first parameter must be 'ref Entity'");
                        valid = false;
                    }
                    
                    var p = ps[1];
                    if (!(p.ParameterType.IsByRef && p.IsIn && p.ParameterType.GetElementType() == typeof(LeafData)))
                    {
                        Debug.LogError($"{t.FullName}.{name}: second parameter must be 'in LeafData'");
                        valid = false;
                    }
                    
                    var p2 = ps[2];
                    if (!(p2.ParameterType.IsByRef && !p2.IsIn && p2.ParameterType.GetElementType() == typeof(LeafStateElement)))
                    {
                        Debug.LogError($"{t.FullName}.{name}: third parameter must be 'ref LeafStateElement'");
                        valid = false;
                    }
                    
                    var p3 = ps[3];
                    if (!(p3.ParameterType.IsByRef && p3.IsIn))
                    {
                        Debug.LogError($"{t.FullName}.{name}: fourth parameter (context) should be passed by in");
                        valid = false;
                    }

                    if (contextType == null) contextType = ps[3].ParameterType;
                    else if (contextType != ps[3].ParameterType)
                    {
                        Debug.LogError($"{t.FullName}.{name}: inconsistent context type with other methods");
                        valid = false;
                    }

                    methods[name] = m;
                }

                if (!valid) continue;

                result.Add(new LeafInfo
                {
                    Name = t.Name,
                    Namespace = t.Namespace,
                    Id = attr.Id,
                    ContextType = contextType
                });
            }

            return result;
        }

        private static void GenerateTables(string folder, Type contextType, List<LeafInfo> leaves)
        {
            string ctxName = contextType.Name;
            if (contextType.IsByRef)
                ctxName = contextType.GetElementType().Name;
            var sb = new StringBuilder();

            // Собираем namespace всех листов
            var namespaces = leaves.Select(l => l.Namespace).Where(ns => !string.IsNullOrEmpty(ns)).Distinct();
            foreach (var ns in namespaces)
                sb.AppendLine($"using {ns};");

            // Namespace контекста
            if (!string.IsNullOrEmpty(contextType.Namespace))
                sb.AppendLine($"using {contextType.Namespace};");

            sb.AppendLine("using Unity.Burst;");
            sb.AppendLine("using Unity.Collections;");
            sb.AppendLine();

            sb.AppendLine("namespace VadimBurym.DodBehaviourTree.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    public static class LeafTables_{ctxName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static NativeArray<FunctionPointer<LeafDelegateTick<{ctxName}>>> TickTable;");
            sb.AppendLine($"        public static NativeArray<FunctionPointer<LeafDelegate<{ctxName}>>> EnterTable;");
            sb.AppendLine($"        public static NativeArray<FunctionPointer<LeafDelegate<{ctxName}>>> ExitTable;");
            sb.AppendLine($"        public static NativeArray<FunctionPointer<LeafDelegate<{ctxName}>>> AbortTable;");
            sb.AppendLine();
            sb.AppendLine("        public static void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine($"            TickTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);");
            sb.AppendLine($"            EnterTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);");
            sb.AppendLine($"            ExitTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);");
            sb.AppendLine($"            AbortTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);");

            foreach (var leaf in leaves)
            {
                sb.AppendLine($@"
            TickTable[{leaf.Id}] = BurstCompiler.CompileFunctionPointer<LeafDelegateTick<{ctxName}>>({leaf.Name}.OnTick);
            EnterTable[{leaf.Id}] = BurstCompiler.CompileFunctionPointer<LeafDelegate<{ctxName}>>({leaf.Name}.OnEnter);
            ExitTable[{leaf.Id}] = BurstCompiler.CompileFunctionPointer<LeafDelegate<{ctxName}>>({leaf.Name}.OnExit);
            AbortTable[{leaf.Id}] = BurstCompiler.CompileFunctionPointer<LeafDelegate<{ctxName}>>({leaf.Name}.OnAbort);");
            }

            sb.AppendLine("        }");
            sb.AppendLine(@"
        public static void Dispose()
        {
            if (TickTable.IsCreated) TickTable.Dispose();
            if (EnterTable.IsCreated) EnterTable.Dispose();
            if (ExitTable.IsCreated) ExitTable.Dispose();
            if (AbortTable.IsCreated) AbortTable.Dispose();
        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(Path.Combine(folder, $"LeafTables_{ctxName}.g.cs"), sb.ToString());
        }

        private class LeafInfo
        {
            public string Name;
            public string Namespace;
            public byte Id;
            public Type ContextType;
        }
    }
}