using System;
using System.Collections.Generic;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure.StaticData;
using _Project._Code.Infrastructure.StaticData.AI;
using _Project._Code.Infrastructure.StaticData.Units;
using _Project.Code.EditorTools;
using _Project.Code.EditorTools.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class GameManager : OdinMenuEditorWindow
{
    private const string CONFIGS_PATH = "Assets/_Project/Configs/";
    
    private readonly List<IDisposable> _disposables = new List<IDisposable>();
    private readonly Dictionary<Type, IEditorStaticData> _staticData = new();
    
    [MenuItem("Tools/GameManager",priority =-9999)]
    public static void OpenWindow()
    {
        GetWindow<GameManager>().Show();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Dispose();
    }

    private void Dispose()
    {
        for (int i = 0; i < _disposables.Count; i++)
            _disposables[i].Dispose();
        _disposables.Clear();
    }
    
    protected override void OnBeginDrawEditors()
    {
        base.OnBeginDrawEditors();
        OdinMenuTreeSelection selected = this.MenuTree.Selection;
        SirenixEditorGUI.BeginHorizontalToolbar();
        {
            GUILayout.FlexibleSpace();

            if (SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                var selectedAsset = selected.SelectedValue;
                foreach (var type in _staticData.Keys)
                {
                    if (selectedAsset.GetType() == type)
                    {
                        var asset = (ScriptableObject)selectedAsset;
                        string path = AssetDatabase.GetAssetPath(asset);
                        var staticData =  _staticData[type];
                        
                        staticData.RemoveConfig(asset);
                        EditorUtility.SetDirty((ScriptableObject)staticData);
                
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        _staticData.Clear();
        Dispose();
        var tree = new OdinMenuTree();
        SetupInitSettings(tree);
        
        AddFolderPage<BehaviourTreeStaticData>(tree, "StaticData", "AI", "AI");
        AddFolderPage<UnitsStaticData>(tree, "StaticData", "Units", "Units");
        AddScriptableObjectPage<SettingsPipeline>(tree, "Settings","_Main");
        AddFolderPage<WidgetStaticData>(tree, "StaticData","UI Widgets", "UI");
        
        SetupPreviewIcons(tree);
        return tree;
    }

    private void AddFolderPage(OdinMenuTree tree, string pageName, params string[] paths)
    {
        foreach (var path in paths)
        {
            tree.AddAllAssetsAtPath(pageName,
                CONFIGS_PATH + path,
                typeof(ScriptableObject),
                includeSubDirectories: true,
                flattenSubDirectories: false);
        }
    }
    
    private void AddFolderPage<TMain>(OdinMenuTree tree, string mainPath, string pageName, params string[] paths)
        where TMain : ScriptableObject
    {
        
        var main = LoadFirstAssetInFolder<TMain>(CONFIGS_PATH + mainPath);
        if (main == null)
        {
            Debug.LogError($"{typeof(TMain)} not found at {CONFIGS_PATH + mainPath}");
            return;
        }
        var rootItems = tree.AddObjectAtPath(pageName, main);
        foreach (var path in paths)
        {
            tree.AddAllAssetsAtPath(
                pageName,
                CONFIGS_PATH + path,
                typeof(ScriptableObject),
                includeSubDirectories: true,
                flattenSubDirectories: false);
        }
    }
    
    private void AddScriptableObjectPage<TConfig>(OdinMenuTree tree, string pageName, string soPath)
        where TConfig : ScriptableObject
    {
        tree.Add(pageName, new ScriptableObjectPage<TConfig>(
            LoadStaticData<TConfig>(CONFIGS_PATH + soPath)));
    }
    
    private static T LoadFirstAssetInFolder<T>(string folderPath) where T : ScriptableObject
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });
        if (guids.Length == 0)
            return null;
        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
    
    private void AddStaticDataPage<TStaticData, TConfig>(OdinMenuTree tree, string pageName, string staticDataPath, string configsPath)
        where TStaticData : ScriptableObject, IEditorStaticData
        where TConfig : ScriptableObject
    {
        var staticData = LoadStaticData<TStaticData>(CONFIGS_PATH + staticDataPath);
        var page = new StaticDataPage<TStaticData, TConfig>(staticData, CONFIGS_PATH + configsPath);
        tree.Add(pageName, page);
        _disposables.Add(page);
        _staticData.Add(typeof(TConfig), staticData);
        tree.AddAllAssetsAtPath(pageName, CONFIGS_PATH + configsPath, typeof(TConfig));
    }
    
    private void SetupInitSettings(OdinMenuTree tree)
    {
        tree.DefaultMenuStyle = new OdinMenuStyle()
        {
            Height = 30,
            Offset = 16.00f,
            IndentAmount = -6.00f,
            IconSize = 26.00f,
            IconOffset = 0.00f,
            NotSelectedIconAlpha = 0.85f,
            IconPadding = 6.00f,
            TriangleSize = 16.00f,
            TrianglePadding = 0.00f,
            AlignTriangleLeft = true,
            Borders = true,
            BorderPadding = 0.00f,
            BorderAlpha = 0.32f,
            SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
            SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
        };
        tree.Add("Menu Settings", tree.DefaultMenuStyle);
    }
    
    private void SetupPreviewIcons(OdinMenuTree tree)
    {
        foreach (var item in tree.EnumerateTree())
        {
            if (item.Value is IEditorDataPreview dataPreview &&
                dataPreview.EditorPreview != null)
            {
                Sprite sprite = dataPreview.EditorPreview;
                item.IconGetter = () =>
                {
                    var tex = AssetPreview.GetAssetPreview(sprite);
                    if (tex == null)
                        tex = AssetPreview.GetMiniThumbnail(sprite);
                    return tex;
                };
            }
        }
    }
    
    private T LoadStaticData<T>(string PATH) where T : ScriptableObject
    {
        var guids = AssetDatabase.FindAssets(
            $"t:{typeof(T)}",
            new[] { PATH });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        Debug.LogError($"{typeof(T)} not found in {PATH}");
        return null;
    }
}