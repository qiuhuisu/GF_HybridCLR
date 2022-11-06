using DG.Tweening;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GameOverProcedure : ProcedureBase
{
    IFsm<IProcedureManager> procedure;
    private bool isWin;

    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        this.procedure = procedureOwner;
        isWin = this.procedure.GetData<VarBoolean>("IsWin");

        GF.AD.SendEvent(isWin ? "finish" : "fail", new Dictionary<string, string> { ["levelID"] = GF.UserData.GAME_LEVEL.ToString()});
        if (isWin)
        {
            if (GF.Setting.GetBool(Utility.Text.Format("NEW_LV{0}", GF.UserData.GAME_LEVEL), true) && GF.UserData.GAME_LEVEL <= GF.DataTable.GetDataTable<LevelTable>().MaxIdDataRow.Id)
            {
                GF.AD.SendEvent("firstFinish", new Dictionary<string, string> { ["levelID"] = GF.UserData.GAME_LEVEL.ToString()});
            }
            GF.UserData.GAME_LEVEL++;
        }
        GF.Setting.SetBool(Utility.Text.Format("NEW_LV{0}", GF.UserData.GAME_LEVEL), false);
        ShowGameOverUIForm(2);
    }
    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        if (!isShutdown)
        {
            GF.UI.CloseAllLoadingUIForms();
            GF.UI.CloseAllLoadedUIForms();
            GF.Entity.HideAllLoadingEntities();
            GF.Entity.HideAllLoadedEntities();
        }
        base.OnLeave(procedureOwner, isShutdown);
    }

    private void ShowGameOverUIForm(float delay)
    {
        DOTween.Sequence().AppendInterval(delay).onComplete = () =>
        {
            var gameoverParms = UIParams.Acquire();
            gameoverParms.Set<VarBoolean>("IsWin", isWin);
            GF.UI.OpenUIForm(UIViews.GameOverUIForm, gameoverParms);
        };
    }

    internal void NextLevel()
    {
        ChangeState<MenuProcedure>(procedure);
    }
}
