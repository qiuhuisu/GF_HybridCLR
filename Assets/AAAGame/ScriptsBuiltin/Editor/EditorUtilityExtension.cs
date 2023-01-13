using GameFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

public class EditorUtilityExtension
{

    /// <summary>
    /// ѡ����Թ���·���ļ���
    /// </summary>
    /// <param name="title">����</param>
    /// <param name="relativePath">Ĭ�ϴ򿪵�·��(���·��)</param>
    /// <returns></returns>
    public static string OpenRelativeFolderPanel(string title, string relativePath)
    {
        var rootPath = Directory.GetParent(Application.dataPath).FullName;
        var curFullPath = Path.Combine(rootPath, relativePath);
        var selectPath = EditorUtility.OpenFolderPanel(title, curFullPath, curFullPath);

        return string.IsNullOrWhiteSpace(selectPath) ? selectPath : Path.GetRelativePath(rootPath, selectPath);
    }

    /// <summary>
    /// ��UnityEditor�����ļ�ѡ�����
    /// </summary>
    /// <param name="assetTp"></param>
    /// <param name="searchFilter"></param>
    /// <param name="onObjectSelectorClosed"></param>
    /// <param name="objectSelectorID"></param>
    /// <returns></returns>
    public static bool OpenAssetSelector(Type assetTp, string searchFilter = null, Action<UnityEngine.Object> onObjectSelectorClosed = null, int objectSelectorID = 0)
    {
        var objSelector = Utility.Assembly.GetType("UnityEditor.ObjectSelector");
        var objSelectorInst = objSelector?.GetProperty("get", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(objSelector);
        if (objSelectorInst == null) return false;

        var objSelectorInstTp = objSelectorInst.GetType();
        var showFunc = objSelectorInstTp.GetMethod("Show", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new System.Type[] { typeof(UnityEngine.Object), typeof(Type), typeof(UnityEngine.Object), typeof(bool), typeof(List<int>), typeof(Action<UnityEngine.Object>), typeof(Action<UnityEngine.Object>) }, null);
        if (showFunc == null) return false;

        showFunc.Invoke(objSelectorInst, new object[] { null, assetTp, null, false, null, onObjectSelectorClosed, null });
        if (!string.IsNullOrEmpty(searchFilter))
        {
            objSelectorInstTp.GetProperty("searchFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(objSelectorInst, searchFilter);
        }

        objSelectorInstTp.GetField("objectSelectorID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(objSelectorInst, objectSelectorID);

        return true;
    }
}
