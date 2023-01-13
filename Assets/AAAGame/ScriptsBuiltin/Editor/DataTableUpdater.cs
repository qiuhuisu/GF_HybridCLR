#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static partial class DataTableUpdater
{
    static string[] tableFileChangedList;
    static string[] configFileChangedList;

    static bool isInitialized = false;
    static AppConfigs appConfigs = null;
    [InitializeOnLoadMethod]
    private static async void Init()
    {
        if (isInitialized) return;
        EditorApplication.update += OnUpdate;
        tableFileChangedList = new string[0];
        configFileChangedList = new string[0];
        var tbWatcher = new FileSystemWatcher(ConstEditor.DataTableExcelPath, "*.xlsx");
        tbWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
        tbWatcher.EnableRaisingEvents = true;
        var fileChangedCb = new FileSystemEventHandler(OnDataTableChanged);
        var fileRenameCb = new RenamedEventHandler(OnDataTableChanged);
        tbWatcher.Changed += fileChangedCb;
        tbWatcher.Deleted += fileChangedCb;
        tbWatcher.Renamed += fileRenameCb;

        var cfgWatcher = new FileSystemWatcher(ConstEditor.ConfigExcelPath, "*.xlsx");
        cfgWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
        cfgWatcher.EnableRaisingEvents = true;
        var cfgFileChangedCb = new FileSystemEventHandler(OnConfigChanged);
        var cfgFileRenameCb = new RenamedEventHandler(OnConfigChanged);
        cfgWatcher.Changed += cfgFileChangedCb;
        cfgWatcher.Deleted += cfgFileChangedCb;
        cfgWatcher.Renamed += cfgFileRenameCb;
        appConfigs = await AppConfigs.GetInstanceSync();
        isInitialized = true;
    }


    private static void OnUpdate()
    {
        if (!isInitialized) return;

        if (tableFileChangedList != null && tableFileChangedList.Length > 0)
        {
            var changedFiles = GetMainExcelFiles(appConfigs.DataTables, tableFileChangedList);
            MyGameTools.RefreshAllDataTable(changedFiles);
            if (ArrayUtility.Contains(changedFiles, Path.GetFileNameWithoutExtension(ConstEditor.UITableExcel)))
            {
                MyGameTools.GenerateUIViewScript();
            }
            if (ArrayUtility.Contains(changedFiles, Path.GetFileNameWithoutExtension(ConstEditor.EntityGroupTableExcel)) ||
                    ArrayUtility.Contains(changedFiles, Path.GetFileNameWithoutExtension(ConstEditor.SoundGroupTableExcel)) ||
                    ArrayUtility.Contains(changedFiles, Path.GetFileNameWithoutExtension(ConstEditor.UIGroupTableExcel)) ||
                    ArrayUtility.Contains(changedFiles, Path.GetFileNameWithoutExtension(ConstEditor.EntityGroupTableExcel)))
            {
                MyGameTools.GenerateGroupEnumScript();
            }
            foreach (var item in changedFiles)
            {
                Debug.LogFormat("-----------------自动刷新DataTable:{0}-----------------", item);
            }
            ArrayUtility.Clear(ref tableFileChangedList);
        }
        if (configFileChangedList != null && configFileChangedList.Length > 0)
        {
            var changedFiles = GetMainExcelFiles(appConfigs.Configs, configFileChangedList);
            MyGameTools.RefreshAllConfig(changedFiles);
            foreach (var item in changedFiles)
            {
                Debug.LogFormat("-----------------自动刷新Config:{0}-----------------", item);
            }
            ArrayUtility.Clear(ref configFileChangedList);
        }
    }
    private static string[] GetMainExcelFiles(string[] files, string[] changedFiles)
    {
        string[] result = new string[0];
        foreach (var changedFile in changedFiles)
        {
            foreach (var mainName in files)
            {
                var changedFileName = changedFile;
                if (changedFileName.CompareTo(mainName) == 0 || changedFileName.StartsWith(mainName + "_"))
                {
                    if (ArrayUtility.Contains(result, mainName))
                    {
                        break;
                    }
                    ArrayUtility.Add(ref result, mainName);
                }
            }
        }
        return result;
    }
    private static void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        ArrayUtility.Add(ref configFileChangedList, Path.GetFileNameWithoutExtension(e.Name));
    }
    private static void OnDataTableChanged(object sender, FileSystemEventArgs e)
    {
        ArrayUtility.Add(ref tableFileChangedList, Path.GetFileNameWithoutExtension(e.Name));
    }
}
#endif