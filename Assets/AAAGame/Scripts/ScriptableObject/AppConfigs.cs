using UnityEngine;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AppConfigs", menuName = "ScriptableObject/AppConfigs【配置App运行时所需数据表、配置表、流程】")]
public class AppConfigs : ScriptableObject
{
    private static AppConfigs mInstance = null;

    [Header("预加载数据表")]
    [SerializeField] string[] mDataTables;
    public string[] DataTables => mDataTables;


    [Header("预加载配置表")]
    [SerializeField] string[] mConfigs;
    public string[] Configs => mConfigs;

    [Header("已启用流程列表")]
    [SerializeField] string[] mProcedures;
    public string[] Procedures => mProcedures;

    private void Awake()
    {
        mInstance = this;
    }

    public static async Task<AppConfigs> GetInstanceSync()
    {
        var configAsset = UtilityBuiltin.ResPath.GetScriptableAsset("AppConfigs");
        if (mInstance == null)
#if UNITY_EDITOR
            mInstance = await Task.FromResult(AssetDatabase.LoadAssetAtPath<AppConfigs>(configAsset));
#else
            mInstance = await GFBuiltin.Resource.LoadAssetAsync<AppConfigs>(configAsset);
#endif
        return mInstance;
    }

}
