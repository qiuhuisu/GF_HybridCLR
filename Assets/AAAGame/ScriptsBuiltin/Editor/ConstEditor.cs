#if UNITY_EDITOR
using System.IO;
using UnityEngine;

public class ConstEditor
{
    public const string DataTablePath = "Assets/AAAGame/DataTable";
    public const string GameConfigPath = "Assets/AAAGame/Config";
    public const string UIViewScriptFile = "Assets/AAAGame/Scripts/UI/UIViews.cs";
    public const string UISerializeFieldDir = "Assets/AAAGame/Scripts/UI/UIVariables";//生成UI变量代码目录
    public const string UITableExcel = "UITable.xlsx";
    public const string EntityGroupTableExcel = "EntityGroupTable.xlsx";
    public const string SoundGroupTableExcel = "SoundGroupTable.xlsx";
    public const string UIGroupTableExcel = "UIGroupTable.xlsx";
    public const string ConstGroupScriptFileFullName = "Assets/AAAGame/Scripts/Common/Const.Groups.cs";

    public static readonly string[] PrefabsPath = { "Assets/AAAGame/Prefabs/" };

    public const string DataTableCodePath = "Assets/AAAGame/Scripts/DataTable";
    public const string DataTableCodeTemplate = "Assets/AAAGame/ScriptsBuiltin/Editor/DataTableGenerator/DataTableCodeTemplate/DataTableCodeTemplate.txt";
    public const string BuiltinAssembly = "Assets/AAAGame/ScriptsBuiltin/Runtime/Builtin.Runtime.asmdef";
    public const string HotfixAssembly = "Assets/AAAGame/Scripts/Hotfix.asmdef";

    public const string SharedAssetBundleName = "SharedAssets";//AssetBundle分包共用资源
    public static readonly string[] DefaultLayers = { "UI", "WorldUI"};

    public static string DataTableExcelPath => UtilityBuiltin.ResPath.GetCombinePath(new DirectoryInfo(Application.dataPath).Parent.FullName, "DataTables");
    public static string ConfigExcelPath => UtilityBuiltin.ResPath.GetCombinePath(new DirectoryInfo(Application.dataPath).Parent.FullName, "Configs");
}
#endif