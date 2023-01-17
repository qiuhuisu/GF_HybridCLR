using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class LocalizationStringEditor : EditorWindow
{
    Vector2 scrollViewPos1;
    Vector2 scrollViewPos2;
    //[MenuItem("Game Framework/GameTools/Localization Editor【语言本地化】", false)]
    public static LocalizationStringEditor Open()
    {
        var win = EditorWindow.GetWindow<LocalizationStringEditor>("Localization Editor");
        win.Show();
        return win;
    }
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
}
