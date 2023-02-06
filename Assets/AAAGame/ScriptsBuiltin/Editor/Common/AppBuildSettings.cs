#if UNITY_EDITOR
using UnityEngine;
using HybridCLR.Editor;
using GameFramework.Resource;
using System.Collections.Generic;

[FilePath("ProjectSettings/AppBuildSettings.asset")]
public class AppBuildSettings : HybridCLR.Editor.ScriptableSingleton<AppBuildSettings>
{
    public string UpdatePrefixUri;
    public string ApplicableGameVersion;
    public bool ForceUpdateApp = false;
    public string AppUpdateUrl;
    public string AppUpdateDesc;
    public bool RevealFolder = false;

    //Android Build Settings
    public bool AndroidUseKeystore;
    public string AndroidKeystoreName;
    public string KeystorePass;
    public string AndroidKeyAliasName;
    public string KeyAliasPass;

    public bool DevelopmentBuild = false;
    public bool BuildForGooglePlay = false;

    //图片压缩工具设置项
    public string CompressImgToolBackupDir;
    public bool CompressImgToolCoverRaw = false;//压缩后的图片直接覆盖原文件
    public string CompressImgToolOutputDir;
    public List<string> CompressImgToolKeys = new List<string>() { "TinyPngKey" };
    public List<UnityEngine.Object> CompressImgToolItemList = new List<UnityEngine.Object>();
    public bool CompressImgToolOffline = true;//离线模式; 使用本地压缩工具pngquant压缩(仅支持png,其它格式依然走tinypng在线压缩)
    public int CompressImgToolFastLv = 1;  //取值1-10, 数值越大压缩的速度越快,但压缩比会稍微降低
    public float CompressImgToolQualityLv = 80; //pngquant压缩质量等级,数值越小压缩后图片越小
    public float CompressImgToolQualityMinLv = 0;
    public string AppBuildDir = "../BuildApp";
}
#endif