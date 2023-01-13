using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
internal enum ConfigEditorMode
{
    StripLinkConfig,
    AotDllConfig
}
public class StripLinkConfigEditor : EditorWindow
{
    private class ItemData
    {
        public bool isOn;
        public string dllName;
        public ItemData(bool isOn, string dllName)
        {
            this.isOn = isOn;
            this.dllName = dllName;
        }
    }
    private Vector2 scrollPosition;
    private string[] selectedDllList;
    private List<ItemData> dataList;
    private GUIStyle normalStyle;
    private GUIStyle selectedStyle;

    ConfigEditorMode mode;
    private void OnEnable()
    {
        normalStyle = new GUIStyle();
        normalStyle.normal.textColor = Color.white;

        selectedStyle = new GUIStyle();
        selectedStyle.normal.textColor = Color.green;
        dataList = new List<ItemData>();
        RefreshListData();
    }
    internal void SetEditorMode(ConfigEditorMode mode)
    {
        this.mode = mode;
        RefreshListData();
    }
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        if (dataList.Count <= 0)
        {
            EditorGUILayout.HelpBox("未找到程序集,请先Build项目以生成程序集.", MessageType.Warning);
        }
        else
        {
            switch (mode)
            {
                case ConfigEditorMode.StripLinkConfig:
                    EditorGUILayout.HelpBox("勾选需要添加到Link.xml的程序集,然后点击保存生效.", MessageType.Info);
                    break;
                case ConfigEditorMode.AotDllConfig:
                    EditorGUILayout.HelpBox("勾选需要添加到AOT元数据补充的dll,然后点击保存生效.", MessageType.Info);
                    break;
            }
        }
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
        for (int i = 0; i < dataList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            var item = dataList[i];
            item.isOn = EditorGUILayout.ToggleLeft(item.dllName, item.isOn, item.isOn ? selectedStyle : normalStyle);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All", GUILayout.Width(100)))
        {
            SelectAll(true);
        }
        if (GUILayout.Button("Cancel All", GUILayout.Width(100)))
        {
            SelectAll(false);
        }
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reload", GUILayout.Width(120)))
        {
            RefreshListData();
        }
        if (GUILayout.Button("Save", GUILayout.Width(120)))
        {
            switch (mode)
            {
                case ConfigEditorMode.StripLinkConfig:
                    if (MyGameTools.Save2LinkFile(GetCurrentSelectedList()))
                    {
                        EditorUtility.DisplayDialog("Strip LinkConfig Editor", "Update link.xml success!", "OK");
                    }
                    break;
                case ConfigEditorMode.AotDllConfig:
                    if (MyGameTools.Save2AotDllList(GetCurrentSelectedList()))
                    {
                        EditorUtility.DisplayDialog("AOT dlls Editor", "Update AOT dll List success!", "OK");
                    }
                    break;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    private void SelectAll(bool isOn)
    {
        foreach (var item in dataList)
        {
            item.isOn = isOn;
        }
    }
    private string[] GetCurrentSelectedList()
    {
        List<string> result = new List<string>();
        foreach (var item in dataList)
        {
            if (item.isOn)
            {
                result.Add(item.dllName);
            }
        }
        return result.ToArray();
    }
    private void RefreshListData()
    {
        dataList.Clear();

        switch (mode)
        {
            case ConfigEditorMode.StripLinkConfig:
                selectedDllList = MyGameTools.GetSelectedAssemblyDlls();
                break;
            case ConfigEditorMode.AotDllConfig:
                selectedDllList = MyGameTools.GetSelectedAotDlls();
                break;
        }
        foreach (var item in MyGameTools.GetProjectAssemblyDlls())
        {
            dataList.Add(new ItemData(IsInSelectedList(item), item));
        }
    }
    private bool IsInSelectedList(string dllName)
    {
        return ArrayUtility.Contains(selectedDllList, dllName);
    }
}
