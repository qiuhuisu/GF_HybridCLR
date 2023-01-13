#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Procedure;
using UnityEditorInternal;

[CustomEditor(typeof(AppConfigs))]
public class AppConfigsInspector : Editor
{
    enum ConfigDataType
    {
        DataTable,
        Config
    }
    private class ItemData
    {
        public bool isOn;
        public string excelName { get; private set; }

        public ItemData(bool isOn, string dllName)
        {
            this.isOn = isOn;
            this.excelName = dllName;
        }
    }
    private class ScrollViewData
    {

        public ConfigDataType CfgType { get; private set; }
        public Vector2 scrollPos;//记录滚动列表位置
        public string excelDir;
        public string excelOuputDir;
        public List<ItemData> ExcelItems { get; private set; }

        public ScrollViewData(ConfigDataType configTp, string srcDir, string desDir)
        {
            this.CfgType = configTp;
            this.excelDir = srcDir;
            this.excelOuputDir = desDir;
        }
        public void Reload(AppConfigs appConfig)
        {
            if (!Directory.Exists(excelDir) || appConfig == null) return;

            var excels = Directory.GetFiles(excelDir, "*.*", SearchOption.AllDirectories);
            excels = excels.Where(name =>
            {
                var ext = Path.GetExtension(name).ToLower();
                return ext.CompareTo(".xls") == 0 || ext.CompareTo(".xlsx") == 0 || ext.CompareTo(".xlsm") == 0;
            }).ToArray();

            if (ExcelItems == null) ExcelItems = new List<ItemData>();
            ExcelItems.Clear();

            string[] desArr = this.CfgType == ConfigDataType.DataTable ? appConfig.DataTables : appConfig.Configs;
            foreach (var item in excels)
            {
                var excelName = Path.GetFileNameWithoutExtension(item);
                if (excelName.Contains('_')) continue;//过滤AB测试表

                var isOn = ArrayUtility.Contains(desArr, excelName);
                ExcelItems.Add(new ItemData(isOn, excelName));
            }
        }
        public string[] GetSelectedItems()
        {
            var selectedList = ExcelItems.Where(dt => dt.isOn).ToArray();
            string[] resultArr = new string[selectedList.Length];
            for (int i = 0; i < selectedList.Length; i++)
            {
                resultArr[i] = Path.GetFileNameWithoutExtension(selectedList[i].excelName);
            }
            return resultArr;
        }

        internal void SetSelectAll(bool v)
        {
            foreach (var item in ExcelItems)
            {
                item.isOn = v;
            }
        }
    }
    AppConfigs appConfig;
    ScrollViewData[] svDataArr;
    bool procedureFoldout = true;
    Vector2 procedureScrollPos;
    ItemData[] procedures;
    private GUIStyle normalStyle;
    private GUIStyle selectedStyle;
    private void OnEnable()
    {
        appConfig = target as AppConfigs;
        normalStyle = new GUIStyle();
        normalStyle.normal.textColor = Color.white;
        selectedStyle = new GUIStyle();
        selectedStyle.normal.textColor = Color.green;

        svDataArr = new ScrollViewData[2] { new ScrollViewData(ConfigDataType.DataTable, ConstEditor.DataTableExcelPath, ConstEditor.DataTablePath), new ScrollViewData(ConfigDataType.Config, ConstEditor.ConfigExcelPath, ConstEditor.GameConfigPath) };
        ReloadScrollView(appConfig);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();
        serializedObject.Update();
        if (GUILayout.Button("Path Settings [设置DataTable/Config导入/导出路径]"))
        {
            InternalEditorUtility.OpenFileAtLineExternal(UtilityBuiltin.ResPath.GetCombinePath(Application.dataPath, "AAAGame/ScriptsBuiltin/Editor/ConstEditor.cs"), 0);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("DataTables:");

        svDataArr[0].scrollPos = GUILayout.BeginScrollView(svDataArr[0].scrollPos, GUILayout.MaxHeight(300));
        {
            EditorGUI.BeginChangeCheck();
            foreach (var item in svDataArr[0].ExcelItems)
            {
                item.isOn = EditorGUILayout.ToggleLeft(item.excelName, item.isOn, item.isOn ? selectedStyle : normalStyle);
            }
            if (EditorGUI.EndChangeCheck())
            {
                SaveConfig(appConfig);
            }
        }
        GUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("All", GUILayout.Width(50)))
        {
            svDataArr[0].SetSelectAll(true);
            SaveConfig(appConfig);
        }
        if (GUILayout.Button("None", GUILayout.Width(50)))
        {
            svDataArr[0].SetSelectAll(false);
            SaveConfig(appConfig);
        }
        GUILayout.Space(20);

        if (GUILayout.Button("Reveal", GUILayout.Width(70)))
        {
            EditorUtility.RevealInFinder(svDataArr[0].excelDir);
        }
        if (GUILayout.Button("Export", GUILayout.Width(70)))
        {
            MyGameTools.RefreshAllDataTable(appConfig.DataTables);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Configs:");

        svDataArr[1].scrollPos = GUILayout.BeginScrollView(svDataArr[1].scrollPos, GUILayout.MaxHeight(300));
        {
            EditorGUI.BeginChangeCheck();
            foreach (var item in svDataArr[1].ExcelItems)
            {
                item.isOn = EditorGUILayout.ToggleLeft(item.excelName, item.isOn, item.isOn ? selectedStyle : normalStyle);
            }
            if (EditorGUI.EndChangeCheck())
            {
                SaveConfig(appConfig);
            }
        }
        GUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("All", GUILayout.Width(50)))
        {
            svDataArr[1].SetSelectAll(true);
            SaveConfig(appConfig);
        }
        if (GUILayout.Button("None", GUILayout.Width(50)))
        {
            svDataArr[1].SetSelectAll(false);
            SaveConfig(appConfig);
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Reveal", GUILayout.Width(70)))
        {
            EditorUtility.RevealInFinder(svDataArr[1].excelDir);
        }
        if (GUILayout.Button("Export", GUILayout.Width(70)))
        {
            MyGameTools.RefreshAllConfig(appConfig.Configs);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        procedureFoldout = EditorGUILayout.Foldout(procedureFoldout, "Procedures:");// EditorGUILayout.Foldout(procedureFoldout, "Procedures:");
        if (procedureFoldout)
        {
            procedureScrollPos = GUILayout.BeginScrollView(procedureScrollPos, GUILayout.MaxHeight(300));
            {
                EditorGUI.BeginChangeCheck();
                foreach (var item in procedures)
                {
                    item.isOn = EditorGUILayout.ToggleLeft(item.excelName, item.isOn, item.isOn ? selectedStyle : normalStyle);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SaveConfig(appConfig);
                }
            }
            GUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Height(30)))
        {
            ReloadScrollView(appConfig);
        }
        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            SaveConfig(appConfig);
        }
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();
    }
    //private void DrawPathSettingPanel()
    //{
    //    pathPanelFoldout = EditorGUILayout.Foldout(pathPanelFoldout, "Path Settings:");
    //    if (pathPanelFoldout)
    //    {
    //        EditorGUILayout.BeginVertical();
    //        {
    //            EditorGUILayout.BeginHorizontal("box");
    //            {
    //                EditorGUILayout.LabelField("DataTable Excel Path:", EditorStyles.boldLabel, GUILayout.Width(150));
    //                AppBuildSettings.Instance.DataTableExcelPath = EditorGUILayout.TextField(AppBuildSettings.Instance.DataTableExcelPath);
    //                if (GUILayout.Button("Select", GUILayout.Width(80)))
    //                {
    //                    var projectRoot = Directory.GetParent(Application.dataPath).FullName;
    //                    var curFullPath = UtilityBuiltin.ResPath.GetCombinePath(projectRoot, AppBuildSettings.Instance.DataTableExcelPath);
    //                    var sPath = EditorUtility.OpenFolderPanel("Select Path", curFullPath, null);
    //                    AppBuildSettings.Instance.DataTableExcelPath = Path.GetRelativePath(projectRoot, sPath);
    //                    AppBuildSettings.Save();
    //                    GUIUtility.ExitGUI();
    //                }
    //            }
    //            EditorGUILayout.EndHorizontal();

    //            EditorGUILayout.BeginHorizontal("box");
    //            {
    //                EditorGUILayout.LabelField("Config Excel Path:", EditorStyles.boldLabel, GUILayout.Width(150));
    //                AppBuildSettings.Instance.ConfigExcelPath = EditorGUILayout.TextField(AppBuildSettings.Instance.ConfigExcelPath);
    //                if (GUILayout.Button("Select", GUILayout.Width(80)))
    //                {
    //                    var projectRoot = Directory.GetParent(Application.dataPath).FullName;
    //                    var curFullPath = UtilityBuiltin.ResPath.GetCombinePath(projectRoot, AppBuildSettings.Instance.ConfigExcelPath);
    //                    var sPath = EditorUtility.OpenFolderPanel("Select Path", curFullPath, null);
    //                    AppBuildSettings.Instance.ConfigExcelPath = Path.GetRelativePath(projectRoot, sPath);
    //                    AppBuildSettings.Save();
    //                    GUIUtility.ExitGUI();
    //                }
    //            }
    //            EditorGUILayout.EndHorizontal();
    //        }
    //        EditorGUILayout.EndVertical();
    //    }
    //}
    private void SaveConfig(AppConfigs cfg)
    {
        foreach (var svData in svDataArr)
        {
            if (svData.CfgType == ConfigDataType.DataTable)
            {
                cfg.GetType().GetField("mDataTables", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(cfg, svData.GetSelectedItems());
            }
            else if (svData.CfgType == ConfigDataType.Config)
            {
                cfg.GetType().GetField("mConfigs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(cfg, svData.GetSelectedItems());
            }
        }
        string[] selectedProcedures = new string[0];
        foreach (var item in procedures)
        {
            if (item.isOn)
            {
                ArrayUtility.Add(ref selectedProcedures, item.excelName);
            }
        }
        cfg.GetType().GetField("mProcedures", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(cfg, selectedProcedures);
        EditorUtility.SetDirty(cfg);
    }
    private void ReloadScrollView(AppConfigs cfg)
    {
        foreach (var item in svDataArr)
        {
            item.Reload(cfg);
        }

        ReloadProcedures(cfg);
    }
    private void ReloadProcedures(AppConfigs cfg)
    {
        procedures ??= new ItemData[0];
        ArrayUtility.Clear(ref procedures);
        //#if !DISABLE_HYBRIDCLR
        var hotfixDlls = Utility.Assembly.GetAssemblies().Where(dll => HybridCLR.Editor.SettingsUtil.HotUpdateAssemblyNames.Contains(dll.GetName().Name)).ToArray();

        foreach (var item in hotfixDlls)
        {
            var proceClassArr = item.GetTypes().Where(tp => tp.BaseType == typeof(ProcedureBase)).ToArray();
            foreach (var proceClass in proceClassArr)
            {
                var proceName = proceClass.FullName;
                ArrayUtility.Add(ref procedures, new ItemData(cfg.Procedures.Contains(proceName), proceName));
            }
        }
        //#endif
    }
}
#endif