using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using GameFramework.Localization;
using UnityGameFramework.Runtime;
public static class LocalizationExtension
{
    /// <summary>
    /// 获取本地化字符串,若不存在则直接返回key
    /// </summary>
    /// <param name="com"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetText(this LocalizationComponent com, string key)
    {
        if (!com.HasRawString(key)) return key;
        return com.GetString(key);
    }
    public static string GetText(this LocalizationComponent com, string key, params object[] parms)
    {
        string formatKey = Utility.Text.Format(key, parms);
        return com.GetText(formatKey);
    }
    public static string GetText(this LocalizationComponent com, string key, bool toUpper)
    {
        toUpper = toUpper && com.Language == Language.English;
        string result = com.GetText(key);
        return toUpper ? result.ToUpper() : result;
    }
}
