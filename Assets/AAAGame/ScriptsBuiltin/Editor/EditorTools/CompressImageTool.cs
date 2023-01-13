using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using TinifyAPI;
using GameFramework;

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
    List<UnityEngine.Object> srcItemList;
    ReorderableList tinypngKeyScrollList;
    Vector2 tinypngScrollListPos;

    int selectOjbWinId = "CompressImageTool".GetHashCode();
    private bool settingFoldout = true;

    public static CompressImageTool Open()
    {
        var win = EditorWindow.GetWindow<CompressImageTool>("图片压缩工具");
        win.Show();
        return win;
    }
    private void OnEnable()
    {
        dragAreaContent = new GUIContent("拖拽到此区域可添加文件");
        centerLabelStyle = new GUIStyle();
        centerLabelStyle.alignment = TextAnchor.MiddleCenter;
        centerLabelStyle.fontSize = 25;
        centerLabelStyle.normal.textColor = Color.gray;

        srcItemList = new List<UnityEngine.Object>();
        srcScrollList = new ReorderableList(srcItemList, typeof(UnityEngine.Object), true, true, true, true);
        srcScrollList.drawHeaderCallback = DrawScrollListHeader;
        srcScrollList.onAddCallback = AddItem;
        srcScrollList.drawElementCallback = DrawItems;
        srcScrollList.elementHeight = EditorGUIUtility.singleLineHeight;

        tinypngKeyScrollList = new ReorderableList(AppBuildSettings.Instance.CompressImgToolKeys, typeof(string), true, true, true, true);
        tinypngKeyScrollList.drawHeaderCallback = DrawTinypngKeyScrollListHeader;
        tinypngKeyScrollList.drawElementCallback = DrawTinypngKeyItem;
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
        if (EditorGUI.LinkButton(rect, "添加TinyPng Keys:"))
        {
            Application.OpenURL("https://tinify.com/dashboard/api");
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(10);
        srcScrollPos = EditorGUILayout.BeginScrollView(srcScrollPos);
        srcScrollList.DoLayoutList();
        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        if (settingFoldout = EditorGUILayout.Foldout(settingFoldout, "展开设置项:"))
        {
            DrawSettingsPanel();
        }
        DrawDropArea();
        EditorGUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("开始压缩", GUILayout.Height(30)))
            {
                StartCompress();
            }
            if (GUILayout.Button("备份图片", GUILayout.Height(30)))
            {
                BackupImages();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
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
            AppBuildSettings.Instance.CompressImgToolCoverRaw = EditorGUILayout.ToggleLeft("覆盖源文件", AppBuildSettings.Instance.CompressImgToolCoverRaw, GUILayout.Width(100));
            EditorGUI.BeginDisabledGroup(AppBuildSettings.Instance.CompressImgToolCoverRaw);
            {
                EditorGUILayout.SelectableLabel(AppBuildSettings.Instance.CompressImgToolOutputDir, EditorStyles.selectionRect, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
                if (GUILayout.Button("选择输出路径", GUILayout.Width(120)))
                {
                    var backupPath = EditorUtilityExtension.OpenRelativeFolderPanel("选择图片输出路径", AppBuildSettings.Instance.CompressImgToolOutputDir);
                    AppBuildSettings.Instance.CompressImgToolOutputDir = backupPath;
                    AppBuildSettings.Save();
                    GUIUtility.ExitGUI();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUILayout.LabelField("备份路径:", GUILayout.Width(100));
            EditorGUILayout.SelectableLabel(AppBuildSettings.Instance.CompressImgToolBackupDir, EditorStyles.selectionRect, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("选择备份路径", GUILayout.Width(120)))
            {
                var backupPath = EditorUtilityExtension.OpenRelativeFolderPanel("选择图片备份路径", AppBuildSettings.Instance.CompressImgToolBackupDir);

                AppBuildSettings.Instance.CompressImgToolBackupDir = backupPath;
                AppBuildSettings.Save();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void BackupImages()
    {
        //TODO 备份文件
        throw new NotImplementedException();
    }

    private void StartCompress()
    {
        if (AppBuildSettings.Instance.CompressImgToolCoverRaw && string.IsNullOrWhiteSpace(AppBuildSettings.Instance.CompressImgToolOutputDir))
        {
            EditorUtility.DisplayDialog("错误", "图片输出路径无效!", "OK");
            return;
        }
        var imgList = GetAllImages();
        //CompressImages(imgList);
        foreach (var item in imgList)
        {
            Debug.Log(item);
        }
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

        int clickBtIdx = EditorUtility.DisplayDialogComplex("请确认", Utility.Text.Format("共 {0} 张图片待压缩, 是否开始压缩?", imgList), "开始压缩", "取消", null);
        if (clickBtIdx != 0)
        {
            //用户取消压缩
            return;
        }

        TinifyAPI.Tinify.Key = firstKey;
        imgList.Reverse();

        var rootPath = Directory.GetParent(Application.dataPath).FullName;

        if (AppBuildSettings.Instance.CompressImgToolCoverRaw)
        {
            for (int i = imgList.Count - 1; i >= 0; i--)
            {
                var imgFileName = imgList[i];
                var srcImg = TinifyAPI.Tinify.FromFile(imgFileName);
                await srcImg.ToFile(imgFileName);
                if (srcImg.IsCompletedSuccessfully)
                {
                    imgList.RemoveAt(i);
                }
            }
            OnCompressCompleted(imgList);
        }
        else
        {
            string desPath = Path.Combine(rootPath, AppBuildSettings.Instance.CompressImgToolOutputDir);

        }
    }

    private void OnCompressCompleted(List<string> imgList)
    {
        if (imgList.Count <= 0)
        {
            EditorUtility.DisplayDialog("压缩完成!", "全部文件已压缩完成", "OK");
            GUIUtility.ExitGUI();
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
    /// 获取当前添加的所有图片(相对项目路径)
    /// </summary>
    /// <returns></returns>
    private List<string> GetAllImages()
    {
        List<string> images = new List<string>();
        foreach (var item in srcItemList)
        {
            var itmTp = CheckItemType(item);
            if (itmTp == ItemType.Image)
            {
                string imgFileName = AssetDatabase.GetAssetPath(item);
                if (images.Contains(imgFileName)) continue;
                images.Add(imgFileName);
            }
            else if (itmTp == ItemType.Folder)
            {
                string imgFolder = AssetDatabase.GetAssetPath(item);
                if (Directory.Exists(imgFolder))
                {
                    var allImgFiles = Directory.GetFiles(imgFolder, "*.*", SearchOption.AllDirectories).Where(fileName => ArrayUtility.Contains(SupportImgTypes, Path.GetExtension(fileName)) && !images.Contains(fileName));
                    images.AddRange(allImgFiles);
                }
            }
        }
        return images;
    }

    private void DrawDropArea()
    {
        var dragRect = EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.LabelField(dragAreaContent, centerLabelStyle, GUILayout.Height(100));
            if (dragRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                else if (Event.current.type == EventType.DragExited && dragRect.Contains(Event.current.mousePosition))
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
        if (obj == null || srcItemList.Contains(obj)) return;

        srcItemList.Add(obj);
    }
    private ItemType CheckItemType(UnityEngine.Object item)
    {
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
        var item = srcItemList[index];
        if (item != null)
        {
            EditorGUI.LabelField(rect, EditorGUIUtility.ObjectContent(item, item.GetType()));
        }
        else
        {
            EditorGUI.LabelField(rect, "missing asset");
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
