using UnityEditor;
using UnityEngine;

public class CreateNewScriptListener : UnityEditor.AssetModificationProcessor
{
    public static void OnWillCreateAsset(string assetPath)
    {
        if (System.IO.Path.GetExtension(assetPath).CompareTo(".meta") == 0)
        {
            var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (assetName.EndsWith(".cs") || assetName.EndsWith(".txt"))
            {
                var fullName = UtilityBuiltin.ResPath.GetCombinePath(System.IO.Path.GetDirectoryName(assetPath), assetName);
                ConvertScriptToUTF8(fullName);
            }
        }
    }
    /// <summary>
    /// ��.cs��.txt�ļ�תΪutf-8
    /// </summary>
    /// <param name="assetPath"></param>
    static void ConvertScriptToUTF8(string assetPath)
    {
        if (!System.IO.File.Exists(assetPath)) return;
        var fileTxt = System.IO.File.ReadAllText(assetPath);
        System.IO.File.WriteAllText(assetPath, fileTxt, System.Text.Encoding.UTF8);
        AssetDatabase.Refresh();
    }
}
