using GameFramework.Resource;
using UnityEngine;

[CreateAssetMenu(fileName = "AppSettings", menuName = "ScriptableObject/AppSettings【App内置配置参数】")]
public class AppSettings : ScriptableObject
{
    private static AppSettings mInstance = null;
    public static AppSettings Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = Resources.Load<AppSettings>("AppSettings");
            }
            return mInstance;
        }
    }
    [Tooltip("debug模式,默认显示debug窗口")]
    public bool DebugMode = false;
    [Tooltip("资源模式: 单机/全热更/需要时热更")]
    public ResourceMode ResourceMode = ResourceMode.Package;
}
