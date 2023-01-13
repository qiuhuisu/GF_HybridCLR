using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;
using UnityGameFramework.Editor.ResourceTools;
[UnityEditor.InitializeOnLoad]
public static class EditorToolbarExtension
{
    private static GUIContent buildBtContent;
    private static GUIContent appConfigBtContent;
    private static GUIContent toolsDropBtContent;

    //Toolbar栏工具箱下拉列表
    private static string[] toolNameList =
    {
        "资源/图片压缩工具",
        "语言国际化扫描工具"
    };
    static EditorToolbarExtension()
    {
        buildBtContent = EditorGUIUtility.TrTextContentWithIcon("Build App/Hotfix", "打新包/打热更", "UnityLogo");
        appConfigBtContent = EditorGUIUtility.TrTextContentWithIcon("App Configs", "配置App运行时所需DataTable/Config/Procedure", "Settings");
        toolsDropBtContent = EditorGUIUtility.TrTextContentWithIcon("Tools", "工具箱", "CustomTool");

        UnityEditorToolbar.RightToolbarGUI.Add(OnRightToolbarGUI);
        UnityEditorToolbar.LeftToolbarGUI.Add(OnLeftToolbarGUI);
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
            GenericMenu popMenu = new GenericMenu();
            for (int i = 0; i < toolNameList.Length; i++)
            {
                popMenu.AddItem(new GUIContent(toolNameList[i]), false, menuIdx => { ClickToolsSubmenu((int)menuIdx); }, i);
            }
            popMenu.ShowAsContext();
        }
        GUILayout.FlexibleSpace();
    }
    static void ClickToolsSubmenu(int menuIdx)
    {
        switch (menuIdx)
        {
            case 0: //TinyPng图片压缩工具
                CompressImageTool.Open();
                break;
            case 1: //语言国际化扫描工具
                Debug.LogWarning("工具正在开发中...");
                break;
        }
    }
}
