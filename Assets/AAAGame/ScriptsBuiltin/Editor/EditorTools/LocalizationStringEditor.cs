using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[EditorToolMenu("资源/语言国际化扫描工具", 2)]
public class LocalizationStringEditor : EditorToolBase
{
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

    public static void FindLocalizationString()
    {
        EditorUtility.DisplayProgressBar("Progress", "Find Localization String...", 0);
        string[] dirs = { "Assets/AAAGame/Prefabs/UI" };
        var asstIds = AssetDatabase.FindAssets("t:Prefab", dirs);
        int count = 0;
        List<string> str_list = new List<string>();
        for (int i = 0; i < asstIds.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(asstIds[i]);
            var pfb = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (Text item in pfb.GetComponentsInChildren<Text>(true))
            {
                var str = item.text;
                //str.Replace(@"\n", @"\\n");
                str = Regex.Replace(str, @"\n", @"\n");

                if (string.IsNullOrWhiteSpace(str))
                {
                    continue;
                }
                if (!str_list.Contains(str))
                {
                    str_list.Add(str);
                }
            }
            count++;
            EditorUtility.DisplayProgressBar("Find Class", pfb.name, count / (float)asstIds.Length);
        }
        string content = string.Empty;
        foreach (var item in str_list)
        {
            content += string.Format("\"{0}\":\"{0}\",\n", item);
        }
        System.IO.File.WriteAllText(Application.dataPath + "/Localization.txt", content);
        EditorUtility.ClearProgressBar();
    }
}
