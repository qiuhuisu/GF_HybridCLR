using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.IO;

[EditorToolMenu("资源/语言国际化扫描工具", 2)]
public class LocalizationStringEditor : EditorToolBase
{
    private static readonly string LocalizationStrPattern = "Localization.GetText\\(\"([^\"]+)\"";
    Vector2 scrollViewPos1;
    Vector2 scrollViewPos2;
    public override string ToolName => "语言国际化工具";

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("内置国际化文本:");
        GUILayout.Label("热更国际化文本:");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        scrollViewPos1 = EditorGUILayout.BeginScrollView(scrollViewPos1);

        EditorGUILayout.EndScrollView();

        scrollViewPos2 = EditorGUILayout.BeginScrollView(scrollViewPos2);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rescan All"))
        {
            
        }
        if (GUILayout.Button("Save All"))
        {

        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    /// <summary>
    /// 扫描全部国际化语言Key
    /// </summary>
    void ScanAllLocalizationString()
    {
        Dictionary<string, string> lanMap = new Dictionary<string, string>();
        ScanPrefabLocalizationString(lanMap);
        ScanCodeLocalizationString(lanMap);
    }
    /// <summary>
    /// 扫描Prefab中的国际化语言
    /// </summary>
    private void ScanPrefabLocalizationString(Dictionary<string, string> lanMap)
    {
        string[] dirs = { "Assets/AAAGame/Prefabs" };
        var assetGUIDs = AssetDatabase.FindAssets("t:Prefab", dirs);
        
        List<UnityGameFramework.Runtime.UIStringKey> keyList = new List<UnityGameFramework.Runtime.UIStringKey>();
        int totalCount = assetGUIDs.Length;
        for (int i = 0; i < totalCount; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            if(EditorUtility.DisplayCancelableProgressBar($"扫描进度({i}/{totalCount})", path, i / (float)totalCount))
            {
                break;
            }
            var pfb = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            keyList.Clear();
            pfb.GetComponentsInChildren<UnityGameFramework.Runtime.UIStringKey>(true, keyList);
            foreach (var newKey in keyList)
            {
                if (lanMap.ContainsKey(newKey.Key)) continue;
                lanMap.Add(newKey.Key, "");
            }
        }
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 扫描代码中的国际化语言
    /// </summary>
    private void ScanCodeLocalizationString(Dictionary<string, string> lanMap)
    {
        ;
        var builtinKeys = ScanLocalizationStringInScripts(Path.GetDirectoryName(ConstEditor.BuiltinAssembly));//扫描内置程序集代码
        var hotfixKeys = ScanLocalizationStringInScripts(Path.GetDirectoryName(ConstEditor.HotfixAssembly));//扫描热更程序集代码
        foreach (var item in builtinKeys)
        {
            if (lanMap.ContainsKey(item)) continue;
            lanMap.Add(item, "");
        }
        foreach (var item in hotfixKeys)
        {
            if (lanMap.ContainsKey(item)) continue;
            lanMap.Add(item, "");
        }
    }

    /// <summary>
    /// 扫面代码中的多语言文字
    /// </summary>
    /// <param name="扫描文件夹"></param>
    /// <returns></returns>
    private List<string> ScanLocalizationStringInScripts(string dirName)
    {
        var scriptGuidArr = AssetDatabase.FindAssets("t:Script", new string[] { dirName });
        List<string> result = new List<string>();
        foreach (var scriptGuid in scriptGuidArr)
        {
            var scriptName = AssetDatabase.GUIDToAssetPath(scriptGuid);
            var codeText = File.ReadAllText(scriptName);
            var matches = Regex.Matches(codeText, LocalizationStrPattern);
            foreach (Match match in matches)
            {
                if (!match.Success)
                {
                    continue;
                }
                var lanKey = match.Result("$1");
                if (!result.Contains(lanKey))
                {
                    result.Add(lanKey);
                }
            }
        }
        return result;
    }
}
