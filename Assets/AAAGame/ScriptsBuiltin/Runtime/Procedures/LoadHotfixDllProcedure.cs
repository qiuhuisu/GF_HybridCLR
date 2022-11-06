using GameFramework.Event;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using GameFramework.Fsm;
using System;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class LoadHotfixDllProcedure : ProcedureBase
{
    /// <summary>
    /// 全部的预加载热更脚本dll
    /// </summary>
    private string[] hotfixDlls;
    private bool hotfixListIsLoaded;
    private int totalProgress;
    private int loadedProgress;
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        GFBuiltin.Event.Subscribe(LoadHotfixDllEventArgs.EventId, OnLoadHotfixDllCallback);
        PreloadAndInitData();
    }


    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        GFBuiltin.Event.Unsubscribe(LoadHotfixDllEventArgs.EventId, OnLoadHotfixDllCallback);
        base.OnLeave(procedureOwner, isShutdown);
    }


    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        if (!hotfixListIsLoaded)
        {
            return;
        }
        //加载热更新Dll完成,进入热更逻辑
        if (loadedProgress >= totalProgress)
        {
            Log.Info("热更dll加载完成, 开始进入HotfixEntry");
            loadedProgress = -1;
#if !DISABLE_HYBRIDCLR
            var hotfixDll = GFBuiltin.Hotfix.GetHotfixClass("HotfixEntry");
            if (hotfixDll == null)
            {
                Log.Error("获取热更入口类HotfixEntry失败!");
                return;
            }
            hotfixDll.GetMethod("StartHotfixLogic").Invoke(null, new object[] { true });
#else
            HotfixEntry.StartHotfixLogic(false);
#endif
        }
    }

    /// <summary>
    /// 加载热更新dll
    /// </summary>
    private void PreloadAndInitData()
    {
        //显示进度条
        GFBuiltin.BuiltinView.ShowLoadingProgress();
        totalProgress = 0;
        loadedProgress = 0;
        hotfixListIsLoaded = true;

#if !UNITY_EDITOR && !DISABLE_HYBRIDCLR
        hotfixListIsLoaded = false;
        LoadHotfixDlls();
#endif
    }

    private void LoadHotfixDlls()
    {
        Log.Info("开始加载热更新dll");
        var hotfixListFile = UtilityBuiltin.ResPath.GetCombinePath("Assets", ConstBuiltin.HOT_FIX_DLL_DIR, "HotfixFileList.txt");
        if (GFBuiltin.Resource.HasAsset(hotfixListFile) == GameFramework.Resource.HasAssetResult.NotExist)
        {
            Log.Fatal("热更新dll列表文件不存在:{0}", hotfixListFile);
            return;
        }
        GFBuiltin.Resource.LoadAsset(hotfixListFile, new GameFramework.Resource.LoadAssetCallbacks((string assetName, object asset, float duration, object userData) =>
        {
            var textAsset = asset as TextAsset;
            if (textAsset != null)
            {
                hotfixDlls = UtilityBuiltin.Json.ToObject<string[]>(textAsset.text);
                Log.Info("hotfix dll json:{0}", textAsset.text);
                totalProgress = hotfixDlls.Length;
                for (int i = 0; i < hotfixDlls.Length - 1; i++)
                {
                    var dllName = hotfixDlls[i];
                    var dllAsset = UtilityBuiltin.ResPath.GetHotfixDll(dllName);
                    GFBuiltin.Hotfix.LoadHotfixDll(dllAsset, this);
                }
                hotfixListIsLoaded = true;
            }
        }));

    }


    private void OnLoadHotfixDllCallback(object sender, GameEventArgs e)
    {
        var args = e as LoadHotfixDllEventArgs;
        if (args.UserData != this)
        {
            return;
        }
        if (args.Assembly == null)
        {
            Log.Error("加载dll失败:{0}", args.DllName);
            return;
        }

        loadedProgress++;
        GFBuiltin.BuiltinView.SetLoadingProgress(loadedProgress / totalProgress);

        //所有依赖dll加载完成后再加载Hotfix.dll
        if (loadedProgress + 1 == totalProgress)
        {
            var mainDll = UtilityBuiltin.ResPath.GetHotfixDll(hotfixDlls.Last());
            GFBuiltin.Hotfix.LoadHotfixDll(mainDll, this);
        }
    }
}
