using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public partial class MyGameTools
{
    private static readonly string LocalizationStrPattern = "Localization.GetText\\(\"([^\"]+)\"";

    public static void GenerateLanguageFile()
    {
        var builtinKeys = ScanLocalizationStrings(ConstEditor.BuiltinAssembly);//扫描内置程序集代码
        var hotfixKeys = ScanLocalizationStrings(ConstEditor.HotfixAssembly);//扫描热更程序集代码


    }
    
    /// <summary>
    /// 扫面代码中的多语言文字
    /// </summary>
    /// <param name="asmdefName"></param>
    /// <returns></returns>
    private static List<string> ScanLocalizationStrings(string asmdefName)
    {
        var dirName = Path.GetDirectoryName(asmdefName);
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
