using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using TinifyAPI;
using GameFramework;
using System.Threading.Tasks;
using System.Text;

public class CompressImageTool : EditorWindow
{
    enum ItemType
    {
        NoSupport,
        Image,//图片文件
        Folder//文件夹
    }
    readonly string[] SupportImgTypes = { ".png", ".jpg", ".webp" };
    GUIContent dragAreaContent;
    GUIStyle centerLabelStyle;
    ReorderableList srcScrollList;
    Vector2 srcScrollPos;
    //List<UnityEngine.Object> srcItemList;
    ReorderableList tinypngKeyScrollList;
    Vector2 tinypngScrollListPos;

    int selectOjbWinId = "CompressImageTool".GetHashCode();
    private bool settingFoldout = true;
#if UNITY_EDITOR_WIN
    const string pngquantTool = "Tools/CompressImageTools/pngquant_win/pngquant.exe";
#elif UNITY_EDITOR_OSX
    const string pngquantTool = "Tools/CompressImageTools/pngquant_mac/pngquant";
#endif

    public static CompressImageTool Open()
    {
        var win = EditorWindow.GetWindow<CompressImageTool>("图片压缩工具");
        win.Show();
        return win;
    }
    private void OnEnable()
    {
        dragAreaContent = new GUIContent("拖拽到此添加图片文件/文件夹");
        centerLabelStyle = new GUIStyle();
        centerLabelStyle.alignment = TextAnchor.MiddleCenter;
        centerLabelStyle.fontSize = 25;
        centerLabelStyle.normal.textColor = Color.gray;

        //srcItemList = new List<UnityEngine.Object>();
        srcScrollList = new ReorderableList(AppBuildSettings.Instance.CompressImgToolItemList, typeof(UnityEngine.Object), true, true, true, true);
        srcScrollList.drawHeaderCallback = DrawScrollListHeader;
        srcScrollList.onAddCallback = AddItem;
        srcScrollList.drawElementCallback = DrawItems;
        srcScrollList.elementHeight = EditorGUIUtility.singleLineHeight;

        tinypngKeyScrollList = new ReorderableList(AppBuildSettings.Instance.CompressImgToolKeys, typeof(string), true, true, true, true);
        tinypngKeyScrollList.drawHeaderCallback = DrawTinypngKeyScrollListHeader;
        tinypngKeyScrollList.drawElementCallback = DrawTinypngKeyItem;
    }
    private void OnDisable()
    {
        AppBuildSettings.Save();
    }
    private void DrawTinypngKeyItem(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorGUI.BeginChangeCheck();
        {
            AppBuildSettings.Instance.CompressImgToolKeys[index] = EditorGUI.TextField(rect, AppBuildSettings.Instance.CompressImgToolKeys[index]);
            if (EditorGUI.EndChangeCheck())
            {
                AppBuildSettings.Save();
            }
        }
    }

    private void DrawTinypngKeyScrollListHeader(Rect rect)
    {
        if (EditorGUI.LinkButton(rect, "添加TinyPng Keys:\t点击跳转到key获取地址..."))
        {
            Application.OpenURL("https://tinify.com/dashboard/api");
        }
    }

    private void OnGUI()
    {
        var rect = EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(10);
        srcScrollPos = EditorGUILayout.BeginScrollView(srcScrollPos);
        srcScrollList.DoLayoutList();
        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        DrawDropArea();
        EditorGUILayout.Space(10);
        if (settingFoldout = EditorGUILayout.Foldout(settingFoldout, "展开设置项:"))
        {
            DrawSettingsPanel();
        }
        EditorGUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("开始压缩", GUILayout.Height(30)))
            {
                StartCompress();
            }
            if (GUILayout.Button("备份图片", GUILayout.Height(30), GUILayout.MaxWidth(100)))
            {
                BackupImages();
            }
            if (GUILayout.Button("还原备份", GUILayout.Height(30), GUILayout.MaxWidth(100)))
            {
                RecoveryImages();
            }
            if (GUILayout.Button("保存设置", GUILayout.Height(30), GUILayout.MaxWidth(100)))
            {
                SaveSettings();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void SaveSettings()
    {
        AppBuildSettings.Save();
    }

    private void RecoveryImages()
    {
        throw new NotImplementedException();
    }

    private void DrawSettingsPanel()
    {
        tinypngScrollListPos = EditorGUILayout.BeginScrollView(tinypngScrollListPos, GUILayout.Height(100));
        {
            tinypngKeyScrollList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.BeginHorizontal("box");
        {
            AppBuildSettings.Instance.CompressImgToolOffline = EditorGUILayout.ToggleLeft("离线压缩png", AppBuildSettings.Instance.CompressImgToolOffline, GUILayout.Width(100));
            AppBuildSettings.Instance.CompressImgToolCoverRaw = EditorGUILayout.ToggleLeft("覆盖原图片", AppBuildSettings.Instance.CompressImgToolCoverRaw, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUI.BeginDisabledGroup(!AppBuildSettings.Instance.CompressImgToolOffline);
            {
                EditorGUILayout.MinMaxSlider(Utility.Text.Format("压缩质量({0:N0}%-{1:N0}%)", AppBuildSettings.Instance.CompressImgToolMinLv, AppBuildSettings.Instance.CompressImgToolMaxLv), ref AppBuildSettings.Instance.CompressImgToolMinLv, ref AppBuildSettings.Instance.CompressImgToolMaxLv, 0, 100);

                AppBuildSettings.Instance.CompressImgToolFastLv = EditorGUILayout.IntSlider(Utility.Text.Format("快压等级({0})", AppBuildSettings.Instance.CompressImgToolFastLv), AppBuildSettings.Instance.CompressImgToolFastLv, 1, 10);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUI.BeginDisabledGroup(AppBuildSettings.Instance.CompressImgToolCoverRaw);
            {
                EditorGUILayout.LabelField("输出路径:", GUILayout.Width(80));
                EditorGUILayout.SelectableLabel(AppBuildSettings.Instance.CompressImgToolOutputDir, EditorStyles.selectionRect, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
                if (GUILayout.Button("选择", GUILayout.Width(80)))
                {
                    var backupPath = EditorUtilityExtension.OpenRelativeFolderPanel("选择图片输出路径", AppBuildSettings.Instance.CompressImgToolOutputDir);
                    AppBuildSettings.Instance.CompressImgToolOutputDir = backupPath;
                    AppBuildSettings.Save();
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("打开", GUILayout.Width(80)))
                {
                    EditorUtility.RevealInFinder(Path.Combine(Directory.GetParent(Application.dataPath).FullName, AppBuildSettings.Instance.CompressImgToolOutputDir));
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUILayout.LabelField("备份路径:", GUILayout.Width(80));
            EditorGUILayout.SelectableLabel(AppBuildSettings.Instance.CompressImgToolBackupDir, EditorStyles.selectionRect, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("选择", GUILayout.Width(80)))
            {
                var backupPath = EditorUtilityExtension.OpenRelativeFolderPanel("选择备份路径", AppBuildSettings.Instance.CompressImgToolBackupDir);

                AppBuildSettings.Instance.CompressImgToolBackupDir = backupPath;
                AppBuildSettings.Save();
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button("打开", GUILayout.Width(80)))
            {
                EditorUtility.RevealInFinder(Path.Combine(Directory.GetParent(Application.dataPath).FullName, AppBuildSettings.Instance.CompressImgToolBackupDir));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void BackupImages()
    {
        //TODO 备份图片
    }

    private void StartCompress()
    {
        if (AppBuildSettings.Instance.CompressImgToolCoverRaw && string.IsNullOrWhiteSpace(AppBuildSettings.Instance.CompressImgToolOutputDir))
        {
            EditorUtility.DisplayDialog("错误", "图片输出路径无效!", "OK");
            return;
        }
        var imgList = GetAllImages();
        CompressImages(imgList);
    }

    private async void CompressImages(List<string> imgList)
    {
        if (AppBuildSettings.Instance.CompressImgToolKeys == null || AppBuildSettings.Instance.CompressImgToolKeys.Count <= 0)
        {
            EditorUtility.DisplayDialog("错误", "请填写TinyPng Key", "OK");
            GUIUtility.ExitGUI();
            return;
        }
        string firstKey = AppBuildSettings.Instance.CompressImgToolKeys[0];
        if (string.IsNullOrWhiteSpace(firstKey))
        {
            EditorUtility.DisplayDialog("错误", "TinyPng首行Key无效", "OK");
            GUIUtility.ExitGUI();
            return;
        }
        if (imgList.Count < 1) return;

        int clickBtIdx = EditorUtility.DisplayDialogComplex("请确认", Utility.Text.Format("共 {0} 张图片待压缩, 是否开始压缩?", imgList.Count), "开始压缩", "取消", null);
        if (clickBtIdx != 0)
        {
            //用户取消压缩
            return;
        }

        TinifyAPI.Tinify.Key = firstKey;
        imgList.Reverse();

        var rootPath = Directory.GetParent(Application.dataPath).FullName;
        string outputPath;
        if (AppBuildSettings.Instance.CompressImgToolCoverRaw)
        {
            outputPath = rootPath;
        }
        else
        {
            outputPath = Path.GetFullPath(AppBuildSettings.Instance.CompressImgToolOutputDir, rootPath);
        }

        if (!Directory.Exists(outputPath))
        {
            try
            {
                Directory.CreateDirectory(outputPath);
            }
            catch (Exception)
            {
                EditorUtility.DisplayDialog("错误", Utility.Text.Format("创建路径失败,请检查路径是否有效:{0}", outputPath), "OK");
                return;
            }
        }

        int totalCount = imgList.Count;
        for (int i = totalCount - 1; i >= 0; i--)
        {
            var imgName = imgList[i];
            var imgFileName = Path.GetFullPath(imgName, rootPath);
            var outputFileName = Path.GetFullPath(imgName, outputPath);
            var outputFilePath = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outputFilePath))
            {
                Directory.CreateDirectory(outputFilePath);
            }
            if (EditorUtility.DisplayCancelableProgressBar(Utility.Text.Format("压缩进度({0}/{1})", totalCount - imgList.Count, totalCount), Utility.Text.Format("正在压缩:{0}", imgName), 1 - imgList.Count / (float)totalCount))
            {
                break;
            }
            if (AppBuildSettings.Instance.CompressImgToolOffline && Path.GetExtension(imgName).ToLower().CompareTo(".png") == 0)
            {
                if (CompressOffline(imgFileName, outputFileName))
                {
                    imgList.RemoveAt(i);
                }
            }
            else
            {
                if (await CompressOnlineAsync(imgFileName, outputFileName))
                {
                    imgList.RemoveAt(i);
                }
            }
        }
        EditorUtility.ClearProgressBar();
        OnCompressCompleted(imgList);
    }

    private void OnCompressCompleted(List<string> imgList)
    {
        AssetDatabase.Refresh();
        if (imgList.Count <= 0)
        {
            EditorUtility.DisplayDialog("压缩完成!", "全部文件已压缩完成", "OK");
            return;
        }
        //提示是否再次压缩所有失败的图片
        var clickBtIdx = EditorUtility.DisplayDialogComplex("警告", Utility.Text.Format("有 {0} 张图片压缩失败, 是否继续压缩?"), "继续压缩", "取消", null);
        if (clickBtIdx == 0)
        {
            CompressImages(imgList);
        }
    }
    /// <summary>
    /// 使用TinyPng在线压缩,支持png,jpg,webp
    /// </summary>
    private async Task<bool> CompressOnlineAsync(string imgFileName, string outputFileName)
    {
        var srcImg = TinifyAPI.Tinify.FromFile(imgFileName);
        await srcImg.ToFile(outputFileName);
        return srcImg.IsCompletedSuccessfully;
    }
    /// <summary>
    /// 使用pngquant离线压缩,只支持png
    /// </summary>
    private bool CompressOffline(string imgFileName, string outputFileName)
    {
        string pngquant = Path.Combine(Directory.GetParent(Application.dataPath).FullName, pngquantTool);

        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendFormat(" --force --quality {0}-{1}", AppBuildSettings.Instance.CompressImgToolMinLv, AppBuildSettings.Instance.CompressImgToolMaxLv);
        strBuilder.AppendFormat(" --speed {0}", AppBuildSettings.Instance.CompressImgToolFastLv);
        strBuilder.AppendFormat(" --output {0}", outputFileName);
        strBuilder.AppendFormat(" -- {0}", imgFileName);
        var procInfo = new System.Diagnostics.ProcessStartInfo(pngquant, strBuilder.ToString());
        procInfo.CreateNoWindow = true;
        System.Diagnostics.Process.Start(procInfo).WaitForExit();
        Debug.Log("-------Log-------");
        foreach (var item in procInfo.Verbs)
        {
            Debug.Log(item);
        }
        return true;
    }
    /// <summary>
    /// 获取当前添加的所有图片(相对项目路径)
    /// </summary>
    /// <returns></returns>
    private List<string> GetAllImages()
    {
        List<string> images = new List<string>();
        foreach (var item in AppBuildSettings.Instance.CompressImgToolItemList)
        {
            var itmTp = CheckItemType(item);
            if (itmTp == ItemType.Image)
            {
                string imgFileName = Utility.Path.GetRegularPath(AssetDatabase.GetAssetPath(item));
                if (images.Contains(imgFileName)) continue;
                images.Add(imgFileName);
            }
            else if (itmTp == ItemType.Folder)
            {
                string imgFolder = AssetDatabase.GetAssetPath(item);
                if (Directory.Exists(imgFolder))
                {
                    var allImgFiles = Directory.GetFiles(imgFolder, "*.*", SearchOption.AllDirectories).Where(fileName =>
                    {
                        fileName = Utility.Path.GetRegularPath(fileName);
                        return ArrayUtility.Contains(SupportImgTypes, Path.GetExtension(fileName)) && !images.Contains(fileName);
                    });

                    images.AddRange(allImgFiles);
                }
            }
        }

        return images.Distinct().ToList();//把结果去重处理
    }

    private void DrawDropArea()
    {
        var dragRect = EditorGUILayout.BeginVertical(EditorStyles.selectionRect);
        {
            EditorGUILayout.LabelField(dragAreaContent, centerLabelStyle, GUILayout.Height(100));
            if (dragRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                else if (Event.current.type == EventType.DragExited)
                {
                    if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                    {
                        OnItemsDrop(DragAndDrop.objectReferences);
                    }

                }
            }

            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 拖拽松手
    /// </summary>
    /// <param name="objectReferences"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnItemsDrop(UnityEngine.Object[] objectReferences)
    {
        foreach (var item in objectReferences)
        {
            if (CheckItemType(item) == ItemType.NoSupport)
            {
                Debug.LogWarningFormat("添加失败! 不支持的文件格式:{0}", AssetDatabase.GetAssetPath(item));
                continue;
            }
            AddItem(item);
        }
    }
    private void AddItem(UnityEngine.Object obj)
    {
        if (obj == null || AppBuildSettings.Instance.CompressImgToolItemList.Contains(obj)) return;

        AppBuildSettings.Instance.CompressImgToolItemList.Add(obj);
    }
    private ItemType CheckItemType(UnityEngine.Object item)
    {
        if (item == null) return ItemType.NoSupport;
        var name = AssetDatabase.GetAssetPath(item);
        if ((File.GetAttributes(name) & FileAttributes.Directory) == FileAttributes.Directory)
        {
            return ItemType.Folder;
        }
        var ext = Path.GetExtension(name).ToLower();
        if (ArrayUtility.Contains(SupportImgTypes, ext))
        {
            return ItemType.Image;
        }

        return ItemType.NoSupport;
    }
    private void DrawItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        var item = AppBuildSettings.Instance.CompressImgToolItemList[index];
        if (item != null)
        {
            EditorGUI.LabelField(rect, EditorGUIUtility.ObjectContent(item, item.GetType()));
        }
        else
        {
            EditorGUI.LabelField(rect, "Missing Asset");
        }
    }

    private void DrawScrollListHeader(Rect rect)
    {
        GUI.Label(rect, "添加要压缩的图片或图片目录:");
    }
    private void OnSelectAsset(UnityEngine.Object obj)
    {
        AddItem(obj);
    }

    private void AddItem(ReorderableList list)
    {
        var openSuccess = EditorUtilityExtension.OpenAssetSelector(typeof(UnityEngine.Object), "t:sprite t:texture2d t:folder", OnSelectAsset, selectOjbWinId);
    }
}
