using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;
using UnityGameFramework.Editor.ResourceTools;
using System;
using System.Collections.Generic;
using GameFramework;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[UnityEditor.InitializeOnLoad]
public static class EditorToolbarExtension
{
    private static GUIContent switchSceneBtContent;
    private static GUIContent buildBtContent;
    private static GUIContent appConfigBtContent;
    private static GUIContent toolsDropBtContent;

    //Toolbar栏工具箱下拉列表
    private static List<Type> editorToolList;
    private static List<string> sceneAssetList;
    static EditorToolbarExtension()
    {
        editorToolList = new List<Type>();
        var curPlatformIcon = Utility.Assembly.GetType("UnityEditor.Networking.PlayerConnection.ConnectionUIHelper").GetMethod("GetIcon", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { EditorUserBuildSettings.activeBuildTarget.ToString() }) as GUIContent;
        switchSceneBtContent = EditorGUIUtility.TrTextContentWithIcon(EditorSceneManager.GetActiveScene().name, "切换场景", "UnityLogo");

        buildBtContent = EditorGUIUtility.TrTextContentWithIcon("Build App/Hotfix", "打新包/打热更", curPlatformIcon.image);
        appConfigBtContent = EditorGUIUtility.TrTextContentWithIcon("App Configs", "配置App运行时所需DataTable/Config/Procedure", "Settings");
        toolsDropBtContent = EditorGUIUtility.TrTextContentWithIcon("Tools", "工具箱", "CustomTool");
        EditorSceneManager.sceneOpened += OnSceneOpened;
        ScanEditorToolClass();

        UnityEditorToolbar.RightToolbarGUI.Add(OnRightToolbarGUI);
        UnityEditorToolbar.LeftToolbarGUI.Add(OnLeftToolbarGUI);
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        switchSceneBtContent.text = scene.name;
    }

    static void ScanEditorToolClass()
    {
        editorToolList.Clear();
        var editorDll = Utility.Assembly.GetAssemblies().First(dll => dll.GetName().Name.CompareTo("Assembly-CSharp-Editor") == 0);
        var allEditorTool = editorDll.GetTypes().Where(tp => (tp.IsClass && !tp.IsAbstract && tp.IsSubclassOf(typeof(EditorToolBase)) && tp.HasAttribute<EditorToolMenuAttribute>()));

        editorToolList.AddRange(allEditorTool);
        editorToolList.Sort((x, y) =>
        {
            int xOrder = x.GetCustomAttribute<EditorToolMenuAttribute>().MenuOrder;
            int yOrder = y.GetCustomAttribute<EditorToolMenuAttribute>().MenuOrder;
            return xOrder.CompareTo(yOrder);
        });
    }
    private static void OnLeftToolbarGUI()
    {
        GUILayout.FlexibleSpace();
        if (EditorGUILayout.DropdownButton(switchSceneBtContent, FocusType.Passive, EditorStyles.toolbarPopup, GUILayout.MaxWidth(150)))
        {
            DrawSwithSceneDropdownMenus();
        }
    }

    private static async void OnRightToolbarGUI()
    {
        if (GUILayout.Button(buildBtContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(125)))
        {
            AppBuildEidtor.Open();
        }
        EditorGUILayout.Space(10);
        if (GUILayout.Button(appConfigBtContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
        {
            var config = await AppConfigs.GetInstanceSync();
            //Selection.activeObject = config;
            EditorUtility.OpenPropertyEditor(config);
        }
        EditorGUILayout.Space(10);
        if (EditorGUILayout.DropdownButton(toolsDropBtContent, FocusType.Passive, GUILayout.MaxWidth(90)))
        {
            DrawEditorToolDropdownMenus();
        }
        GUILayout.FlexibleSpace();
    }
    static void DrawSwithSceneDropdownMenus()
    {
        GenericMenu popMenu = new GenericMenu();
        if (sceneAssetList == null) sceneAssetList = new List<string>();

        var sceneGuids = AssetDatabase.FindAssets("t:Scene", ConstEditor.ScenePath);
        sceneAssetList.Clear();
        for (int i = 0; i < sceneGuids.Length; i++)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            sceneAssetList.Add(scenePath);
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            popMenu.AddItem(new GUIContent(sceneName), false, menuIdx => { SwitchScene((int)menuIdx); }, i);
        }
        popMenu.ShowAsContext();
    }

    private static void SwitchScene(int menuIdx)
    {
        if (menuIdx >= 0 && menuIdx < sceneAssetList.Count)
        {
            var scenePath = sceneAssetList[menuIdx];
            var curScene = EditorSceneManager.GetActiveScene();
            if (curScene != null && curScene.isDirty)
            {
                if (EditorUtility.DisplayDialog("警告", $"当前场景{curScene.name}未保存,是否保存?", "保存", "不保存"))
                {
                    if (!EditorSceneManager.SaveOpenScenes())
                    {
                        return;
                    }
                }
            }
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }

    static void DrawEditorToolDropdownMenus()
    {
        GenericMenu popMenu = new GenericMenu();
        for (int i = 0; i < editorToolList.Count; i++)
        {
            var toolAttr = editorToolList[i].GetCustomAttribute<EditorToolMenuAttribute>();
            popMenu.AddItem(new GUIContent(toolAttr.ToolMenuPath), false, menuIdx => { ClickToolsSubmenu((int)menuIdx); }, i);
        }
        popMenu.ShowAsContext();
    }
    static void ClickToolsSubmenu(int menuIdx)
    {
        var editorTp = editorToolList[menuIdx];
        var win = EditorWindow.GetWindow(editorTp);
        win.Show();
    }
}
