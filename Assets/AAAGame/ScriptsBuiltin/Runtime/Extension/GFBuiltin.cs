using GameFramework;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class GFBuiltin : MonoBehaviour
{
    public static GFBuiltin Instance { get; private set; }
    public static BaseComponent Base { get; private set; }
    public static ConfigComponent Config { get; private set; }
    public static DataNodeComponent DataNode { get; private set; }
    public static DataTableComponent DataTable { get; private set; }
    public static DebuggerComponent Debugger { get; private set; }
    public static DownloadComponent Download { get; private set; }
    public static EntityComponent Entity { get; private set; }
    public static EventComponent Event { get; private set; }
    public static FsmComponent Fsm { get; private set; }
    public static FileSystemComponent FileSystem { get; private set; }
    public static LocalizationComponent Localization { get; private set; }
    public static NetworkComponent Network { get; private set; }
    public static ProcedureComponent Procedure { get; private set; }
    public static ResourceComponent Resource { get; private set; }
    public static SceneComponent Scene { get; private set; }
    public static SettingComponent Setting { get; private set; }
    public static SoundComponent Sound { get; private set; }
    public static UIComponent UI { get; private set; }
    public static WebRequestComponent WebRequest { get; private set; }
    public static BuiltinViewComponent BuiltinView { get; private set; }
    public static HotFixComponent Hotfix { get; private set; }
    public static Camera UICamera { get; private set; }
    public static ScreenFitMode CanvasFitMode { get; private set; }
    public Vector2 ScreenWorldSize { get; private set; }
    private Canvas canvasRoot;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        GFBuiltin.Base = GameEntry.GetComponent<BaseComponent>();
        GFBuiltin.Config = GameEntry.GetComponent<ConfigComponent>();
        GFBuiltin.DataNode = GameEntry.GetComponent<DataNodeComponent>();
        GFBuiltin.DataTable = GameEntry.GetComponent<DataTableComponent>();
        GFBuiltin.Debugger = GameEntry.GetComponent<DebuggerComponent>();
        GFBuiltin.Download = GameEntry.GetComponent<DownloadComponent>();
        GFBuiltin.Entity = GameEntry.GetComponent<EntityComponent>();
        GFBuiltin.Event = GameEntry.GetComponent<EventComponent>();
        GFBuiltin.Fsm = GameEntry.GetComponent<FsmComponent>();
        GFBuiltin.Localization = GameEntry.GetComponent<LocalizationComponent>();
        GFBuiltin.Network = GameEntry.GetComponent<NetworkComponent>();
        GFBuiltin.Resource = GameEntry.GetComponent<ResourceComponent>();
        GFBuiltin.FileSystem = GameEntry.GetComponent<FileSystemComponent>();
        GFBuiltin.Scene = GameEntry.GetComponent<SceneComponent>();
        GFBuiltin.Setting = GameEntry.GetComponent<SettingComponent>();
        GFBuiltin.Sound = GameEntry.GetComponent<SoundComponent>();
        GFBuiltin.UI = GameEntry.GetComponent<UIComponent>();
        GFBuiltin.WebRequest = GameEntry.GetComponent<WebRequestComponent>();
        GFBuiltin.BuiltinView = GameEntry.GetComponent<BuiltinViewComponent>();
        GFBuiltin.Hotfix = GameEntry.GetComponent<HotFixComponent>();

        canvasRoot = GFBuiltin.UI.transform.Find("UICanvasRoot").GetComponent<Canvas>();
        GFBuiltin.UICamera = canvasRoot.worldCamera;

        UpdateCanvasScaler();
    }
    public void UpdateCanvasScaler()
    {
        var screenFitter = GFBuiltin.UICamera.GetComponent<ScreenSizeFitter>();
        var screenFitMode = Screen.width / (float)Screen.height > screenFitter.Ratio ? ScreenFitMode.Height : ScreenFitMode.Width;
        screenFitter.SetFilterMode(screenFitMode);
        CanvasFitMode = screenFitter.UIFitMode;

        CanvasScaler canvasScaler = canvasRoot.GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(screenFitter.designWidth, screenFitter.designHeight);
        this.ScreenWorldSize = GFBuiltin.UICamera.ViewportToWorldPoint(Vector3.one);
        //Log.Info(this.ScreenWorldSize);
        //Log.Info(GFBuiltin.UICamera.ViewportToWorldPoint(new Vector3(1, 1, 0)));
    }
    public Vector2 GetCanvasSize()
    {
        var rect = canvasRoot.GetComponent<RectTransform>();
        return rect.sizeDelta;
    }
    public Vector2 World2ScreenPoint(Camera cam, Vector3 worldPoint)
    {
        var rect = canvasRoot.GetComponent<RectTransform>();
        Vector2 sPoint = cam.WorldToViewportPoint(worldPoint) * rect.sizeDelta;
        return sPoint - rect.sizeDelta * 0.5f;
    }

    /// <summary>
    /// 退出或重启
    /// </summary>
    /// <param name="type"></param>
    public static void Shutdown(ShutdownType type)
    {
        GameEntry.Shutdown(type);
    }

}