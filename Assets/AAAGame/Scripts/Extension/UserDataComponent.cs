
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;
using System;

public class UserDataComponent : GameFrameworkComponent
{
    public int MONEY
    {
        get
        {
            return GF.Setting.GetInt(Const.UserData.MONEY, GF.Config.GetInt("DEFAULT_COINS"));
        }
        set
        {
            int oldNum = MONEY;
            int fixedNum = Mathf.Max(0, value);
            GF.Setting.SetInt(Const.UserData.MONEY, fixedNum);
            FireUserDataChanged(UserDataType.MONEY, oldNum, fixedNum);
        }
    }
    public int AD2MONEY_LV
    {
        get { return GF.Setting.GetInt("AD2MONEY_LV", 0); }
        set
        {
            int oldLv = AD2MONEY_LV;
            int lv = Mathf.Clamp(value, 0, GF.Config.GetInt("AD2MONEY_LV_MAX"));
            GF.Setting.SetInt("AD2MONEY_LV", lv);
            FireUserDataChanged(UserDataType.AD2MONEY_LV, oldLv, lv);
        }
    }
    /// <summary>
    /// 广告的价值
    /// </summary>
    public int AD2MONEY
    {
        get
        {
            int baseNum = GF.Config.GetInt("AD2MONEY_BASE", 0);
            int extNum = GF.Config.GetInt("AD2MONEY_EXT", 0);
            int rewardNum = UtilityBuiltin.Valuer.RoundToInt(baseNum + (extNum + extNum * AD2MONEY_LV) * AD2MONEY_LV * 0.5f);
            return rewardNum;
        }
    }

    /// <summary>
    /// 关卡
    /// </summary>
    public int GAME_LEVEL
    {
        get { return GF.Setting.GetInt(Const.UserData.GAME_LEVEL, 1); }
        set
        {
            var lvTb = GF.DataTable.GetDataTable<LevelTable>();
            int preLvId = GAME_LEVEL;

            int nextLvId = Const.RepeatLevel ? value : Mathf.Clamp(value, lvTb.MinIdDataRow.Id, lvTb.MaxIdDataRow.Id);
            GF.Setting.SetInt(Const.UserData.GAME_LEVEL, nextLvId);
            FireUserDataChanged(UserDataType.GAME_LEVEL, preLvId, nextLvId);
        }
    }


    public bool OfflineBonusTrigger { get; set; }

    public int GetCurrentLevelId()
    {
        
        var lvTb = GF.DataTable.GetDataTable<LevelTable>();
        if (lvTb == null) Log.Fatal("Get LevelTable failed");
        return (GAME_LEVEL - 1) % lvTb.MaxIdDataRow.Id + 1;
    }
    /// <summary>
    /// 触发用户数据改变事件
    /// </summary>
    /// <param name="tp"></param>
    /// <param name="udt"></param>
    private void FireUserDataChanged(UserDataType tp, object oldValue, object value)
    {
        GF.Event.Fire(this, ReferencePool.Acquire<UserDataChangedEventArgs>().Fill(tp, oldValue, value));
    }
    internal string GetCashPrefabName()
    {
        return Utility.Text.Format("Effect/Cash_{0}", GF.UserData.GetMoneyStyleId());
    }
    internal int GetMoneyStyleId()
    {
        //var lvId = GetCurrentLevelId();
        //var colorId = GF.DataTable.GetDataTable<LevelTable>().GetDataRow(lvId).MoneyColorId;
        return 1;
    }

    internal void ClaimMoney(int bonus, bool showFx, Vector3 fxSpawnPos)
    {
        int initMoney = GF.UserData.MONEY;
        GF.UserData.MONEY += bonus;
        GF.Event.Fire(this, ReferencePool.Acquire<PlayerEventArgs>().Fill(PlayerEventType.ClaimMoney, new Dictionary<string, object>
        {
            ["ShowFX"] = showFx,
            ["SpawnPoint"] = fxSpawnPos,
            ["StartNum"] = initMoney
        }));
    }
    public void AddCompleteLevel(int lvId, int starNum)
    {
        string lvIdStr = lvId.ToString();
        var lvDic = GetCompleteLevels();
        if (lvDic.ContainsKey(lvIdStr))
        {
            if (lvDic[lvIdStr] >= starNum)
            {
                return;
            }
            lvDic[lvIdStr] = starNum;
        }
        else
        {
            lvDic.Add(lvIdStr, starNum);
        }
        GF.Setting.SetString("COMPLETE_LEVELS", UtilityBuiltin.Json.ToJson(lvDic));
    }
    public SortedDictionary<string, int> GetCompleteLevels()
    {
        SortedDictionary<string, int> result;
        string jsonStr = GF.Setting.GetString("COMPLETE_LEVELS", string.Empty);
        if (string.IsNullOrWhiteSpace(jsonStr))
        {
            result = new SortedDictionary<string, int>();
        }
        else
        {
            result = LitJson.JsonMapper.ToObject<SortedDictionary<string, int>>(jsonStr);
        }
        return result;
    }
    public void AddOwnCar(int carId)
    {
        //var carTb = GF.DataTable.GetDataTable<CarTable>();
        //var ownCars = GetOwnCars();
        //if (ownCars.Contains(carId) || !carTb.HasDataRow(carId))
        //{
        //    return;
        //}
        //ownCars.Add(carId);
        //GF.Setting.SetString("OWN_CARS", JsonMapper.ToJson(ownCars));
        //GF.Setting.Save();
        //FireUserDataChanged(UserDataType.OWN_CARS, null, null);
    }
    public List<int> GetOwnCars()
    {
        List<int> result = null;
        //string ownCarsJson = GF.Setting.GetString("OWN_CARS", string.Empty);
        //if (string.IsNullOrWhiteSpace(ownCarsJson))
        //{
        //    result = new List<int> { GF.DataTable.GetDataTable<CarTable>().MinIdDataRow.Id };
        //    GF.Setting.SetString("OWN_CARS", JsonMapper.ToJson(result));
        //}
        //else
        //{
        //    result = JsonMapper.ToObject<List<int>>(ownCarsJson);
        //}
        return result;
    }
    internal int GetMultiReward(int coinsNum, int multi = 1)
    {
        int rewardNum = coinsNum * multi;
        return rewardNum;
    }

    /// <summary>
    /// 距离上次打开游戏多少分钟
    /// </summary>
    /// <returns></returns>
    internal double GetLastPlayInterval()
    {
        string timeStr = GF.Setting.GetString(ConstBuiltin.Setting.QuitAppTime, string.Empty);
        DateTime lastTime;
        if (string.IsNullOrWhiteSpace(timeStr))
        {
            lastTime = DateTime.UtcNow;
        }
        else
        {
            lastTime = DateTime.Parse(timeStr);
        }

        var interval = DateTime.UtcNow.Subtract(lastTime).TotalMinutes;
        return interval;
    }

    public void SetIntValue(string key, int value)
    {
        var node = GF.DataNode.GetOrAddNode(key);
        node.SetData<VarInt32>(value);
    }
    public void AddIntValue(string key, int num = 1)
    {
        int value = GetIntValue(key);
        value += num;
        SetIntValue(key, value);
    }
    public int GetIntValue(string key)
    {
        var node = GF.DataNode.GetOrAddNode(key);
        var value = node.GetData<VarInt32>();
        if (value == null)
        {
            return 0;
        }
        else
        {
            return value.Value;
        }
    }

    internal Vector2Int GetOfflineBonus()
    {
        int offlineFactor = GF.Config.GetInt("OfflineFactor");
        int bonus = GF.Config.GetInt("OfflineBonus");
        int bonusMulti = GF.Config.GetInt("OfflineAdBonusMulti");
        int maxBonus = GF.Config.GetInt("MaxOfflineBonus");
        var offlineMinutes = GetOfflineTime().TotalMinutes;
        Vector2Int result = Vector2Int.zero;

        result.x = Mathf.Clamp(bonus * Mathf.FloorToInt((float)offlineMinutes / offlineFactor), 0, maxBonus);
        result.y = Mathf.CeilToInt(result.x * bonusMulti);
        return result;
    }
    internal TimeSpan GetOfflineTime()
    {
        string dTimeStr = GF.Setting.GetString(ConstBuiltin.Setting.QuitAppTime, string.Empty);
        if (string.IsNullOrWhiteSpace(dTimeStr) || !DateTime.TryParse(dTimeStr, out DateTime exitTime))
        {
            return TimeSpan.Zero;
        }
        return System.DateTime.UtcNow.Subtract(exitTime);
    }
    internal bool IsNewDay()
    {
        string dTimeStr = GF.Setting.GetString(ConstBuiltin.Setting.QuitAppTime, string.Empty);
        if (string.IsNullOrWhiteSpace(dTimeStr) || !DateTime.TryParse(dTimeStr, out DateTime dTime))
        {
            return true;
        }

        var today = DateTime.Today;
        return !(today.Year == dTime.Year && today.Month == dTime.Month && today.Day == dTime.Day);
    }

    /// <summary>
    /// 预加载资源完成后 对一些必要的用户数据进行初始化 便于使用
    /// </summary>
    public void InitUserSetting()
    {
        if (IsNewDay())
        {
            GF.Setting.SetInt(Const.UserData.SHOW_RATING_COUNT, 0);
        }
        //设置初始皮肤
        //InitDefaultSkin();
    }
    internal Color GetCarColor(int lvId, int carId)
    {
        int seed = (lvId - 1) * 2 + carId;
        //var colorRows = GF.DataTable.GetDataTable<ColorTable>().GetAllDataRows();
        var resultCol = Color.white;
        //if (seed >= 0)
        //{
        //    seed %= colorRows.Length;
        //    if (ColorUtility.TryParseHtmlString(colorRows[seed].ColorHex, out resultCol))
        //    {
        //        return resultCol;
        //    }
        //}
        //int randomIdx = Utility.Random.GetRandom(0, colorRows.Length);

        //if (ColorUtility.TryParseHtmlString(colorRows[randomIdx].ColorHex, out resultCol))
        //{
        //    return resultCol;
        //}
        return resultCol;
    }
    internal void CheckAndShowRating(float ratio)
    {
        if (GF.UI.HasUIForm(UIViews.StarRateDialog) || GF.Setting.GetBool("RATED_FIVE", false))
        {
            return;
        }

        int show_count = GF.Setting.GetInt(Const.UserData.SHOW_RATING_COUNT, 0);
        if (show_count > 3 || UnityEngine.Random.value > ratio)
        {
            return;
        }

        GF.Setting.SetInt(Const.UserData.SHOW_RATING_COUNT, ++show_count);
        GF.UI.ShowDialog(UIViews.StarRateDialog);
    }
}
