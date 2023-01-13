using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;
using System;
using GameFramework.Resource;
using System.Linq;
#if !DISABLE_HYBRIDCLR
using HybridCLR;
#endif

public class HotFixComponent : GameFrameworkComponent
{
#if !DISABLE_HYBRIDCLR
    [SerializeField] HomologousImageMode mHomologousImageMode = HomologousImageMode.SuperSet;
    /// <summary>
    /// �����ȸ��ļ�
    /// </summary>
    /// <param name="dllAssetName"></param>
    /// <param name="userData"></param>
    public void LoadHotfixDll(string dllAssetName, object userData)
    {
        GFBuiltin.Resource.LoadAsset(dllAssetName, typeof(TextAsset), new LoadAssetCallbacks(OnLoadDllSuccess, OnLoadDllFail), userData);
    }
    /// <summary>
    /// ���ز���ʼ��Ԫ����
    /// </summary>
    /// <param name="dllAssetName"></param>
    /// <param name="loadCallback"></param>
    public void LoadMetadataForAOTAssembly(string dllAssetName, GameFrameworkAction<string, int> loadCallback)
    {
        GFBuiltin.Resource.LoadAsset(dllAssetName, new LoadAssetCallbacks((assetName, asset, duration, userData) =>
        {
            var textAsset = asset as TextAsset;
            if (textAsset == null) loadCallback.Invoke(dllAssetName, (int)LoadImageErrorCode.AOT_ASSEMBLY_NOT_FIND);
            else
            {
                var resultCode = LoadMetadataForAOT(textAsset.bytes);
                loadCallback.Invoke(dllAssetName, (int)resultCode);
            }

        }, (assetName, status, errorMessage, userData) =>
        {
            loadCallback.Invoke(dllAssetName, (int)LoadImageErrorCode.AOT_ASSEMBLY_NOT_FIND);
        }));
    }
    public bool LoadMetadataForAOTAssembly(byte[] dllBytes)
    {
        return LoadMetadataForAOT(dllBytes) == LoadImageErrorCode.OK;
    }
    private void OnLoadDllFail(string assetName, LoadResourceStatus status, string errorMessage, object userData)
    {
        Log.Error("����{0}ʧ��! Error:{1}", assetName, errorMessage);
        GFBuiltin.Event.Fire(this, ReferencePool.Acquire<LoadHotfixDllEventArgs>().Fill(assetName, null, userData));
    }

    private void OnLoadDllSuccess(string assetName, object asset, float duration, object userData)
    {
        var dllTextAsset = asset as TextAsset;
        System.Reflection.Assembly dllAssembly = null;
        if (dllTextAsset != null)
        {
            try
            {
                dllAssembly = System.Reflection.Assembly.Load(dllTextAsset.bytes);
            }
            catch (Exception e)
            {
                Log.Error("Assembly.Load�����ȸ�dllʧ��:{0},Error:{1}", assetName, e.Message);
                throw;
            }

        }

        GFBuiltin.Event.Fire(this, ReferencePool.Acquire<LoadHotfixDllEventArgs>().Fill(assetName, dllAssembly, userData));
    }
    /// <summary>
    /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�
    /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
    /// </summary>
    private LoadImageErrorCode LoadMetadataForAOT(byte[] dllBytes)
    {
        return RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mHomologousImageMode);
    }
#endif
}
