using GameFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GF : GFBuiltin
{
    public static ADComponent AD { get; private set; }
    public static UserDataComponent UserData { get; private set; }
    public static StaticUIComponent StaticUI { get; private set; } //无需异步加载的, 通用UI
    
    private void Start()
    {
        LitJsonExtensions.Register();
        AD = GameEntry.GetComponent<ADComponent>();
        UserData = GameEntry.GetComponent<UserDataComponent>();
        StaticUI = GameEntry.GetComponent<StaticUIComponent>();
    }

    private void OnApplicationQuit()
    {
        OnExitGame();
    }
    private void OnApplicationPause(bool pause)
    {
        //Log.Info("OnApplicationPause:{0}", pause);
        if (Application.isMobilePlatform && pause)
        {
            OnExitGame();
        }
    }
    private void OnExitGame()
    {
        GF.Event.FireNow(this, ReferencePool.Acquire<PlayerEventArgs>().Fill(PlayerEventType.ExitGame));
        var exit_time = DateTime.UtcNow.ToString();
        GF.Setting.SetString(ConstBuiltin.Setting.QuitAppTime, exit_time);
        GF.Setting.Save();
        Log.Info("Exit Time:{0}", exit_time);
    }
}
