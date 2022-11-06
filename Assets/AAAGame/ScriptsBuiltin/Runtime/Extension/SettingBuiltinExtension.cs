using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public static class SettingBuiltinExtension
{
    /// <summary>
    /// 设置语言
    /// </summary>
    /// <param name="com"></param>
    /// <param name="lan"></param>
    public static void SetLanguage(this SettingComponent com, GameFramework.Localization.Language lan, bool saveSetting = true)
    {
        switch (lan)
        {
            case GameFramework.Localization.Language.ChineseSimplified:
                GFBuiltin.Localization.Language = lan;
                break;
            case GameFramework.Localization.Language.ChineseTraditional:
                GFBuiltin.Localization.Language = lan;
                break;
            default:
                GFBuiltin.Localization.Language = GameFramework.Localization.Language.English;
                break;
        }
        if (saveSetting)
        {
            GFBuiltin.Setting.SetString(ConstBuiltin.Setting.Language, lan.ToString());
        }
    }
    /// <summary>
    /// 设置A/B测试组
    /// </summary>
    /// <param name="com"></param>
    /// <param name="groupName"></param>
    public static void SetABTestGroup(this SettingComponent com, string groupName)
    {
        com.SetString(ConstBuiltin.Setting.ABTestGroup, groupName);
    }
    /// <summary>
    /// 获取A/B测试组
    /// </summary>
    /// <param name="com"></param>
    /// <returns></returns>
    public static string GetABTestGroup(this SettingComponent com)
    {
        return com.GetString(ConstBuiltin.Setting.ABTestGroup);
    }
}
