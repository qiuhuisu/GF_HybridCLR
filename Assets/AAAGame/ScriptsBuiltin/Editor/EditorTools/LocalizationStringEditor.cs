using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class LocalizationStringEditor : EditorWindow
{
    Vector2 scrollViewPos1;
    Vector2 scrollViewPos2;
    [MenuItem("Game Framework/GameTools/Localization Editor�����Ա��ػ���", false)]
    public static void ShowAotDllsConfigEditor()
    {
        var win = EditorWindow.GetWindow<LocalizationStringEditor>("Localization Editor");
        win.Show();
    }
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("���ù��ʻ��ı�:");
        GUILayout.Label("�ȸ����ʻ��ı�:");
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
