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
    public List<string> CompressImgToolKeys = new List<string>();
}
#endif