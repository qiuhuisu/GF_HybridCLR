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

[UnityEditor.InitializeOnLoad]
public static class EditorToolbarExtension
{
    private static GUIContent buildBtContent;
    private static GUIContent appConfigBtContent;
    private static GUIContent toolsDropBtContent;

    //Toolbar栏工具箱下拉列表
    private static List<Type> editorToolList;
    static EditorToolbarExtension()
    {
        editorToolList = new List<Type>();
        buildBtContent = EditorGUIUtility.TrTextContentWithIcon("Build App/Hotfix", "打新包/打热更", "UnityLogo");
        appConfigBtContent = EditorGUIUtility.TrTextContentWithIcon("App Configs", "配置App运行时所需DataTable/Config/Procedure", "Settings");
        toolsDropBtContent = EditorGUIUtility.TrTextContentWithIcon("Tools", "工具箱", "CustomTool");
        ScanEditorToolClass();

        UnityEditorToolbar.RightToolbarGUI.Add(OnRightToolbarGUI);
        UnityEditorToolbar.LeftToolbarGUI.Add(OnLeftToolbarGUI);
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
