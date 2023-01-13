using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameFramework;
using GameFramework.Event;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine.U2D;

public partial class MenuUIForm : UIFormBase
{
    [SerializeField] bool showLvSwitch = false;
    //[SerializeField] SpriteAtlas spriteAtlas;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        //curLvNumText.transform.parent.gameObject.SetActive(true);
#if UNITY_EDITOR
        curLvNumText.transform.parent.gameObject.SetActive(showLvSwitch);
#else
        curLvNumText.transform.parent.gameObject.SetActive(false);
#endif
    }
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        GF.Event.Subscribe(UserDataChangedEventArgs.EventId, OnUserDataChanged);
        GF.Event.Subscribe(PlayerEventArgs.EventId, OnPlayerEvent);
        SetLevelNumText(GF.DataModel.GetOrCreate<PlayerDataModel>().GAME_LEVEL);

        RefreshMoneyText();
        InitScrollView();
        //SetLevelProgress(0);
        //ShowOfflineBonus();
    }
    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        //scrollView.UpdateAllShownItemSnapData();
        //for (int i = 0; i < scrollView.ShownItemCount; ++i)
        //{
        //    var shownItem = scrollView.GetShownItemByIndex(i);
        //    float scale = Mathf.Lerp(0.8f, 1.2f, 1 - Mathf.Abs(shownItem.DistanceWithViewPortSnapCenter) * 2f / scrollView.ViewPortSize);
        //    var itemNode = shownItem.transform.GetChild(0);
        //    itemNode.localScale = Vector3.one * scale;
        //}
    }
    private void InitScrollView()
    {
        //scrollView.mOnSnapNearestChanged = OnScrollViewSnapNearestChanged;
        //scrollView.mOnSnapItemFinished = OnScrollViewSnapItemFinished;
        //scrollView.InitListView(-1, OnSpawnItemByIdx);
        //scrollView.MovePanelToItemIndex(-2, 0);
        //scrollView.mOnSelectionChanged = OnSelectionChanged;
        //scrollView.mOnValueChanged = OnValueChanged;
        //scrollView.mOnUpdateContent = OnUpdateContent;
        //scrollView.SetItemCount(10);
    }


    protected override void OnClose(bool isShutdown, object userData)
    {
        GF.Event.Unsubscribe(UserDataChangedEventArgs.EventId, OnUserDataChanged);
        GF.Event.Unsubscribe(PlayerEventArgs.EventId, OnPlayerEvent);
        base.OnClose(isShutdown, userData);
    }

    private void OnPlayerEvent(object sender, GameEventArgs e)
    {
        var args = e as PlayerEventArgs;
        if (args.EventType == PlayerEventType.ClaimMoney)
        {
            var parms = args.EventData as Dictionary<string, object>;
            //["ShowFX"] = showFx,
            //["SpawnPoint"] = fxSpawnPos,
            //["StartNum"] = initMoney,
            //["EndNum"] = GF.UserData.MONEY
            bool showFx = (bool)parms["ShowFX"];
            float fxDelay = 0.25f;
            if (showFx)
            {
                var fxSpawnPoint = (Vector3)parms["SpawnPoint"];
                GF.UI.ShowRewardEffect(fxSpawnPoint, moneyText.transform.position, fxDelay);
            }
            int curMoney = (int)parms["StartNum"];

            var doMoneyNum = DOTween.To(() => curMoney, (x) => curMoney = x, GF.DataModel.GetOrCreate<PlayerDataModel>().Coins, 1).SetEase(Ease.Linear);
            doMoneyNum.SetDelay(fxDelay);
            doMoneyNum.onUpdate = () =>
            {
                SetMoneyText(curMoney);
            };
            doMoneyNum.onComplete = () => { RefreshMoneyText(); };
        }
    }
    private void ShowOfflineBonus()
    {
        //if (!GF.UserData.OfflineBonusTrigger) return;
        //GF.UserData.OfflineBonusTrigger = false;
        //var offlineBonus = GF.UserData.GetOfflineBonus();
        //if (offlineBonus.x <= 0) return;

        //var bonusParms = new Dictionary<string, object>
        //{
        //    ["OfflineBonus"] = offlineBonus
        //};
        //GF.UI.ShowDialog(UIViews.OfflineBonusDialog, bonusParms);
    }

    internal async void Hello()
    {
        Log.Info(">>>>>>>>>>>>>>>>>>>MenuUIForm Hello:{0}", Params.Get<VarVector3>("Vector3"));
        string assetName = UtilityBuiltin.ResPath.GetConfigPath("GameConfig");
        GF.Resource.LoadAsset(assetName, new GameFramework.Resource.LoadAssetCallbacks((string assetName, object asset, float duration, object userData) =>
        {
            var textAsset = asset as TextAsset;
            Log.Info("异步加载:{0}成功!", textAsset.name);
        }));
        //以同步加载方式加载资源
        var textAsset = await GF.Resource.LoadAssetAsync<TextAsset>(assetName);
        Log.Info("同步加载:{0}成功!", textAsset.name);
        transform.Find("Mask/Text").GetComponent<Text>().text = "Hello, From Hotfix v5";
    }

    public override void OnButtonClick(object sender, string btId)
    {
        base.OnButtonClick(sender, btId);
        switch (btId)
        {
            case "SHOP":
                GF.UI.OpenUIForm(UIViews.ShopUIForm);
                break;
            case "SETTING":
                GF.UI.ShowDialog(UIViews.SettingDialog);
                Test();
                break;
        }
    }
    private void Test()
    {
        //var timeStmp = Time.realtimeSinceStartup;
        //float t = 1;
        //for (int i = 1; i <= 10000000; i++)
        //{
        //    t = (i + Mathf.Sin(i) * Mathf.Pow(i, 2)) / 1.2345f;
        //    t = t * Mathf.PI;
        //}
        //timeStmp = Time.realtimeSinceStartup - timeStmp;
        //Log.Info("计算结果:{0}, 用时:{1}", t, timeStmp);
    }

    public void SetLevelProgress(float progress)
    {
        levelProgress.value = progress;
    }
    public float GetLevelProgress()
    {
        return levelProgress.value;
    }
    private void OnUserDataChanged(object sender, GameEventArgs e)
    {
        var args = e as UserDataChangedEventArgs;
        switch (args.Type)
        {
            case UserDataType.MONEY:
                RefreshMoneyText();
                break;
            case UserDataType.GAME_LEVEL:
                SetLevelNumText((int)args.Value);
                break;
        }
    }
    internal void SetLevelNumText(int id)
    {
        levelText.text = id.ToString();
        var lvTb = GF.DataTable.GetDataTable<LevelTable>();
        int nextLvId = Const.RepeatLevel ? id + 1 : Mathf.Min(id + 1, lvTb.MaxIdDataRow.Id);
        nextLevelText.text = nextLvId.ToString();
        curLvNumText.text = id.ToString();
    }

    private void RefreshMoneyText()
    {
        var playerDm = GF.DataModel.GetOrCreate<PlayerDataModel>();
        SetMoneyText(playerDm.Coins);
    }
    private void SetMoneyText(int money)
    {
        moneyText.text = UtilityBuiltin.Valuer.ToCoins(money);
    }
    public void SwitchLevel(int dir)
    {
        GF.DataModel.GetOrCreate<PlayerDataModel>().GAME_LEVEL += dir;
        var menuProcedure = GF.Procedure.CurrentProcedure as MenuProcedure;
        if (null != menuProcedure)
        {
            menuProcedure.ShowLevel();
        }
    }
}
