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

    //ͼƬѹ������������
    public string CompressImgToolBackupDir;
    public bool CompressImgToolCoverRaw = false;//ѹ�����ͼƬֱ�Ӹ���ԭ�ļ�
    public string CompressImgToolOutputDir;
    public List<string> CompressImgToolKeys = new List<string>() { "TinyPngKey" };
    public List<UnityEngine.Object> CompressImgToolItemList = new List<UnityEngine.Object>();
    public bool CompressImgToolOffline = true;//����ģʽ; ʹ�ñ���ѹ������pngquantѹ��(��֧��png,������ʽ��Ȼ��tinypng����ѹ��)
    public int CompressImgToolFastLv = 1;  //ȡֵ1-10, ��ֵԽ��ѹ�����ٶ�Խ��,��ѹ���Ȼ���΢����
    public float CompressImgToolQualityLv = 80; //pngquantѹ�������ȼ�,��ֵԽСѹ����ͼƬԽС
    public float CompressImgToolQualityMinLv = 0;
    public string AppBuildDir = "../BuildApp";
}
#endif