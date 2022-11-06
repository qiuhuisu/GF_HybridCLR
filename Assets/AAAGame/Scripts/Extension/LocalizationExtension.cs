using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using GameFramework.Localization;
using UnityGameFramework.Runtime;
public static class LocalizationExtension
{
    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    /// <param name="com"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetLocalString(this LocalizationComponent com, string key)
    {
        if (!GF.Localization.HasRawString(key)) return key;
        return com.GetString(key);
    }
    public static string GetLocalString(this LocalizationComponent com, string key, bool toUpper)
    {
        toUpper = toUpper && com.Language == Language.English;
        string result = com.GetLocalString(key);
        return toUpper ? result.ToUpper() : result;
    }
}
