using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using GameFramework.Fsm;
using GameFramework.Event;
using System.IO;
using System.Text;
using System.Globalization;

public class LunchProcedure : ProcedureBase
{
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        this.InitSettings();
        Log.Info("Lunch:初始化游戏设置.");
        ChangeState(procedureOwner, GFBuiltin.Base.EditorResourceMode ? typeof(LoadHotfixDllProcedure) : typeof(CheckAndUpdateProcedure));
    }

    private void InitSettings()
    {
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
        GFBuiltin.Debugger.ActiveWindow = ConstBuiltin.IsDebug;
        GFBuiltin.Debugger.WindowScale = 1.4f;

        //初始化语言
        GameFramework.Localization.Language language;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            language = GFBuiltin.Base.EditorLanguage;
        }
        else
        {
            language = GameFramework.Localization.Language.English;// GFBuiltin.Setting.GetLanguage();
        }

        if (language == GameFramework.Localization.Language.Unspecified)
        {
            language = GFBuiltin.Localization.SystemLanguage;//默认语言跟随用户操作系统语言
        }
        GFBuiltin.Setting.SetLanguage(language, false);
    }
}
