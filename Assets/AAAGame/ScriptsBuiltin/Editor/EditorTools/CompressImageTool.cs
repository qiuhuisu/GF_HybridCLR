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
using UnityEditor.U2D;
using UnityEngine.U2D;

[EditorToolMenu("资源/图片资源优化工具", 2)]
public class CompressImageTool : EditorToolBase
{
    public override string ToolName => "图片资源优化工具";
    enum CompressToolMode
    {
        RawFile, //压缩原文件
        UnityAsset, //Unity自带压缩
        Atlas,      //创建图集
        AtlasVariant //图集变体
    }
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
    ReorderableList tinypngKeyScrollList;
    Vector2 tinypngScrollListPos;
    TextureImporterSettings compressSettings;
    TextureImporterPlatformSettings compressPlatformSettings;
    int[] formatValues;
    string[] formatDisplayOptions;
    int selectOjbWinId = "CompressImageTool".GetHashCode();
    private bool settingFoldout = true;

    Dictionary<BuildTarget, TextureImporterFormat[]> texFormatsForPlatforms = new Dictionary<BuildTarget, TextureImporterFormat[]>
    {
        [BuildTarget.Android] = new[] { TextureImporterFormat.ETC2_RGBA8Crunched, TextureImporterFormat.ASTC_6x6 },
        [BuildTarget.StandaloneWindows] = new[] { TextureImporterFormat.DXT5Crunched, TextureImporterFormat.DXT5 },
        [BuildTarget.StandaloneWindows64] = new[] { TextureImporterFormat.DXT5Crunched, TextureImporterFormat.DXT5 }
    };
    //无透明通道的贴图压缩格式
    Dictionary<BuildTarget, TextureImporterFormat> texNoAlphaFormatPlatforms = new Dictionary<BuildTarget, TextureImporterFormat>
    {
        [BuildTarget.Android] = TextureImporterFormat.ETC_RGB4Crunched,
        [BuildTarget.StandaloneWindows] = TextureImporterFormat.DXT1Crunched,
        [BuildTarget.StandaloneWindows64] = TextureImporterFormat.DXT1Crunched,
    };
    Dictionary<BuildTarget, int> texMaxSizePlatforms = new Dictionary<BuildTarget, int>
    {
        [BuildTarget.Android] = 2048,
        [BuildTarget.StandaloneWindows] = 4096,
        [BuildTarget.StandaloneWindows64] = 4096
    };

    string[] tabButtons = { "原文件压缩", "Unity压缩" };
    readonly int[] maxTextureSizeOptionValues = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
    readonly string[] maxTextureSizeDisplayOptions = { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192", "16384" };
    private bool overrideTextureType;
    private bool overrideMeshType;
    private bool overrideAlphaIsTransparency;
    private bool overrideReadable;
    private bool overrideGenerateMipMaps;
    private bool overrideWrapMode;
    private bool overrideFilterMode;
    private bool overrideForTarget;
    private bool overrideMaxSize;
    private bool overrideFormat;
    private bool overrideCompresserQuality;
#if UNITY_EDITOR_WIN
    const string pngquantTool = "Tools/CompressImageTools/pngquant_win/pngquant.exe";

#elif UNITY_EDITOR_OSX
    const string pngquantTool = "Tools/CompressImageTools/pngquant_mac/pngquant";
#endif

    private void OnEnable()
    {
        if (EditorSettings.spritePackerMode != SpritePackerMode.Disabled)
        {
            ArrayUtility.Add(ref tabButtons, "创建图集");
            ArrayUtility.Add(ref tabButtons, "图集变体");
        }
        dragAreaContent = new GUIContent("拖拽到此添加文件/文件夹");
        centerLabelStyle = new GUIStyle();
        centerLabelStyle.alignment = TextAnchor.MiddleCenter;
        centerLabelStyle.fontSize = 25;
        centerLabelStyle.normal.textColor = Color.gray;

        srcScrollList = new ReorderableList(AppBuildSettings.Instance.CompressImgToolItemList, typeof(UnityEngine.Object), true, true, true, true);
        srcScrollList.drawHeaderCallback = DrawScrollListHeader;
        srcScrollList.onAddCallback = AddItem;
        srcScrollList.drawElementCallback = DrawItems;
        srcScrollList.elementHeight = EditorGUIUtility.singleLineHeight;
        SwitchUIPanel((CompressToolMode)AppBuildSettings.Instance.CompressImgMode);
    }
    private void OnDisable()
    {
        SaveSettings();
    }
    private void DrawTinypngKeyItem(Rect rect, int index, bool isActive, bool isFocused)
    {
        AppBuildSettings.Instance.CompressImgToolKeys[index] = EditorGUI.TextField(rect, AppBuildSettings.Instance.CompressImgToolKeys[index]);
    }

    private void DrawTinypngKeyScrollListHeader(Rect rect)
    {
        if (EditorGUI.LinkButton(rect, "添加TinyPng Keys(默认使用第一行Key):\t点击跳转到key获取地址..."))
        {
            Application.OpenURL("https://tinify.com/dashboard/api");
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUI.BeginChangeCheck();
            AppBuildSettings.Instance.CompressImgMode = GUILayout.Toolbar(AppBuildSettings.Instance.CompressImgMode, tabButtons);
            if (EditorGUI.EndChangeCheck())
            {
                SwitchUIPanel((CompressToolMode)AppBuildSettings.Instance.CompressImgMode);
            }
            EditorGUILayout.EndHorizontal();
        }
        srcScrollPos = EditorGUILayout.BeginScrollView(srcScrollPos);
        srcScrollList.DoLayoutList();
        EditorGUILayout.EndScrollView();
        DrawDropArea();
        EditorGUILayout.Space(10);
        if (settingFoldout = EditorGUILayout.Foldout(settingFoldout, "展开设置项:"))
        {
            switch ((CompressToolMode)AppBuildSettings.Instance.CompressImgMode)
            {
                case CompressToolMode.RawFile:
                    DrawRawFileModeSettingsPanel();
                    break;
                case CompressToolMode.UnityAsset:
                    DrawUnityAssetModeSettingsPanel();
                    break;
                case CompressToolMode.Atlas:
                    DrawCreateAtlasVariantSettingsPanel();
                    break;
                case CompressToolMode.AtlasVariant:
                    DrawCreateAtlasVariantSettingsPanel();
                    break;
            }
        }
        switch ((CompressToolMode)AppBuildSettings.Instance.CompressImgMode)
        {
            case CompressToolMode.RawFile:
                DrawCompressRawFileButtonsPanel();
                break;
            case CompressToolMode.UnityAsset:
                DrawCompressUnityButtonsPanel();
                break;
            case CompressToolMode.Atlas:
                DrawCreateAtlasButtonsPanel();
                break;
            case CompressToolMode.AtlasVariant:
                DrawCreateAtlasVariantButtonsPanel();
                break;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawCreateAtlasVariantButtonsPanel()
    {

    }

    private void DrawCreateAtlasButtonsPanel()
    {

    }

    private void DrawCreateAtlasVariantSettingsPanel()
    {

    }

    private void DrawCompressUnityButtonsPanel()
    {
        EditorGUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("开始压缩", GUILayout.Height(30)))
            {
                StartCompressUnityAssetMode();
            }
            if (GUILayout.Button("自动压缩格式", GUILayout.Height(30), GUILayout.MaxWidth(150)))
            {
                AutoCompressUnityAssetMode();
            }
            if (GUILayout.Button("保存设置", GUILayout.Height(30), GUILayout.MaxWidth(100)))
            {
                SaveSettings();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    private void AutoCompressAtlas()
    {
        var atlasFiles = GetAllAtlas();
        int totalCount = atlasFiles.Count;
        for (int i = 0; i < totalCount; i++)
        {
            var atlasFile = atlasFiles[i];
            if (EditorUtility.DisplayCancelableProgressBar($"压缩图集({i}/{totalCount})", atlasFile, i / (float)totalCount))
            {
                break;
            }
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFile);
            if (atlas.isVariant) continue;

            var atlasVariant = UtilityBuiltin.ResPath.GetCombinePath(Path.GetDirectoryName(atlasFile), $"{Path.GetFileNameWithoutExtension(atlasFile)}_Variant{Path.GetExtension(atlasFile)}");
            SpriteAtlas varAtlas;
            if (File.Exists(atlasVariant))
            {
                varAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasVariant);
            }
            else
            {
                AssetDatabase.CreateAsset(new SpriteAtlas(), atlasVariant);
                varAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasVariant);
            }
            atlas.SetIncludeInBuild(false);
            var atlasSettings = atlas.GetPackingSettings();
            atlasSettings.padding = 2;
            atlas.SetPackingSettings(atlasSettings);
            var platformSettings = atlas.GetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
            platformSettings.overridden = true;
            platformSettings.format = texFormatsForPlatforms[EditorUserBuildSettings.activeBuildTarget][0];
            atlas.SetPlatformSettings(platformSettings);
            EditorUtility.SetDirty(atlas);

            varAtlas.SetIsVariant(true);
            varAtlas.SetMasterAtlas(atlas);
            varAtlas.SetIncludeInBuild(true);
            varAtlas.SetVariantScale(0.5f);
            var pSettings = varAtlas.GetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
            pSettings.overridden = true;
            pSettings.format = platformSettings.format;
            varAtlas.SetPlatformSettings(pSettings);
            EditorUtility.SetDirty(varAtlas);
            AssetDatabase.SaveAssetIfDirty(varAtlas);
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { varAtlas }, EditorUserBuildSettings.activeBuildTarget);
        }
        EditorUtility.ClearProgressBar();
    }
    private List<string> GetAllAtlas()
    {
        var list = new List<string>();
        foreach (var item in AppBuildSettings.Instance.CompressImgToolItemList)
        {
            var itemPath = Utility.Path.GetRegularPath(AssetDatabase.GetAssetPath(item));
            if (File.Exists(itemPath) && Path.GetExtension(itemPath).ToLower().CompareTo(".spriteatlas") == 0)//图集
            {
                if (!list.Contains(itemPath))
                {
                    list.Add(itemPath);
                }
            }
            if (Directory.Exists(itemPath))
            {
                var guids = AssetDatabase.FindAssets("t:spriteatlas", new string[] { itemPath });
                foreach (var guid in guids)
                {
                    string spAtlasName = Utility.Path.GetRegularPath(AssetDatabase.GUIDToAssetPath(guid));
                    if (!list.Contains(spAtlasName))
                    {
                        list.Add(spAtlasName);
                    }
                }
            }
        }
        list.RemoveAll(x =>
        {
            var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(x);
            return asset == null || asset.isVariant;
        });
        return list;
    }
    private void StartCompressUnityAssetMode()
    {
        var imgList = GetAllImages();
        if (imgList == null || imgList.Count < 1)
        {
            return;
        }
        int totalCount = imgList.Count;
        for (int i = 0; i < totalCount; i++)
        {
            var assetName = imgList[i];
            var texImporter = AssetImporter.GetAtPath(assetName) as TextureImporter;
            if (EditorUtility.DisplayCancelableProgressBar($"压缩进度({i}/{totalCount})", assetName, i / (float)totalCount))
            {
                break;
            }
            if (texImporter == null) continue;
            var texSetting = new TextureImporterSettings();
            texImporter.ReadTextureSettings(texSetting);

            var texPlatformSetting = texImporter.GetPlatformTextureSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
            bool hasChange = false;
            if (overrideTextureType && texSetting.textureType != compressSettings.textureType)
            {
                texSetting.textureType = compressSettings.textureType;
                hasChange = true;
            }
            if (overrideMeshType && texSetting.spriteMeshType != compressSettings.spriteMeshType)
            {
                texSetting.spriteMeshType = compressSettings.spriteMeshType;
                hasChange = true;
            }
            if (overrideAlphaIsTransparency && texSetting.alphaIsTransparency != compressSettings.alphaIsTransparency)
            {
                texSetting.alphaIsTransparency = compressSettings.alphaIsTransparency;
                hasChange = true;
            }
            if (overrideReadable && texSetting.readable != compressSettings.readable)
            {
                texSetting.readable = compressSettings.readable;
                hasChange = true;
            }
            if (overrideGenerateMipMaps && texSetting.mipmapEnabled != compressSettings.mipmapEnabled)
            {
                texSetting.mipmapEnabled = compressSettings.mipmapEnabled;
                hasChange = true;
            }
            if (overrideWrapMode && texSetting.wrapMode != compressSettings.wrapMode)
            {
                texSetting.wrapMode = compressSettings.wrapMode;
                hasChange = true;
            }
            if (overrideFilterMode && texSetting.filterMode != compressSettings.filterMode)
            {
                texSetting.filterMode = compressSettings.filterMode;
                hasChange = true;
            }
            if (overrideForTarget && texPlatformSetting.overridden != compressPlatformSettings.overridden)
            {
                texPlatformSetting.overridden = compressPlatformSettings.overridden;
                hasChange = true;
            }
            if (overrideMaxSize && texPlatformSetting.maxTextureSize != compressPlatformSettings.maxTextureSize)
            {
                texPlatformSetting.maxTextureSize = compressPlatformSettings.maxTextureSize;
                hasChange = true;
            }
            if (overrideFormat && texPlatformSetting.format != compressPlatformSettings.format)
            {
                texPlatformSetting.format = compressPlatformSettings.format;
                hasChange = true;
            }
            if (overrideCompresserQuality && texPlatformSetting.compressionQuality != compressPlatformSettings.compressionQuality)
            {
                texPlatformSetting.compressionQuality = compressPlatformSettings.compressionQuality;
                hasChange = true;
            }
            if (hasChange)
            {
                texImporter.SetTextureSettings(texSetting);
                texImporter.SetPlatformTextureSettings(texPlatformSetting);
                texImporter.SaveAndReimport();
            }
        }
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 自动选择压缩比最大的格式
    /// </summary>
    private void AutoCompressUnityAssetMode()
    {
        int maxTexSize = texMaxSizePlatforms[EditorUserBuildSettings.activeBuildTarget];
        var targetFormats = texFormatsForPlatforms[EditorUserBuildSettings.activeBuildTarget];
        var noAlphaFormat = texNoAlphaFormatPlatforms[EditorUserBuildSettings.activeBuildTarget];
        var getSizeFunc = Utility.Assembly.GetType("UnityEditor.TextureUtil").GetMethod("GetStorageMemorySizeLong", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        var fileList = GetAllImages();
        int totalCount = fileList.Count;
        for (int i = 0; i < totalCount; i++)
        {
            var fileName = fileList[i];
            if (EditorUtility.DisplayCancelableProgressBar($"进度({i}/{totalCount})", fileName, i / (float)totalCount))
            {
                break;
            }
            var texImporter = AssetImporter.GetAtPath(fileName) as TextureImporter;
            TextureImporterSettings texSettings = new TextureImporterSettings();
            texImporter.ReadTextureSettings(texSettings);
            if (texImporter.textureType == TextureImporterType.NormalMap || !texSettings.alphaIsTransparency || Path.GetExtension(fileName).ToLower().CompareTo(".jpg") == 0)
            {
                var platformSettings = texImporter.GetPlatformTextureSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
                platformSettings.overridden = true;
                platformSettings.format = noAlphaFormat;
                if (platformSettings.maxTextureSize > maxTexSize) platformSettings.maxTextureSize = maxTexSize;
                texImporter.SetPlatformTextureSettings(platformSettings);
                texImporter.SaveAndReimport();
                continue;
            }
            long minTexSize = -1;
            TextureImporterFormat? minTexFormat = null;
            foreach (var tFormat in targetFormats)
            {
                var platformSettings = texImporter.GetPlatformTextureSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
                platformSettings.overridden = true;
                platformSettings.format = tFormat;
                if (platformSettings.maxTextureSize > maxTexSize) platformSettings.maxTextureSize = maxTexSize;
                texImporter.SetPlatformTextureSettings(platformSettings);
                texImporter.SaveAndReimport();

                var tex = AssetDatabase.LoadAssetAtPath<Texture>(fileName);
                var texSize = (long)getSizeFunc.Invoke(null, new object[] { tex });
                if (minTexSize < 0)
                {
                    minTexSize = texSize;
                    minTexFormat = tFormat;
                }

                if (texSize < minTexSize)
                {
                    minTexSize = texSize;
                    minTexFormat = tFormat;
                }
            }
            if (minTexFormat != null)
            {
                Debug.Log($"---------:贴图:{fileName}, 最小格式:{minTexFormat.Value}");
                var platformSettings = texImporter.GetPlatformTextureSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
                if (platformSettings.format != minTexFormat.Value)
                {
                    platformSettings.format = minTexFormat.Value;
                    texImporter.SetPlatformTextureSettings(platformSettings);
                    texImporter.SaveAndReimport();
                }
            }
        }
        EditorUtility.ClearProgressBar();
    }
    private void DrawUnityAssetModeSettingsPanel()
    {
        //Texture Type
        EditorGUILayout.BeginHorizontal();
        {
            overrideTextureType = EditorGUILayout.ToggleLeft("Texture Type", overrideTextureType, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideTextureType);
            {
                compressSettings.textureType = (TextureImporterType)EditorGUILayout.EnumPopup(compressSettings.textureType);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Sprite Mesh Type
        EditorGUILayout.BeginHorizontal();
        {
            overrideMeshType = EditorGUILayout.ToggleLeft("Mesh Type", overrideMeshType, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideMeshType);
            {
                compressSettings.spriteMeshType = (SpriteMeshType)EditorGUILayout.EnumPopup(compressSettings.spriteMeshType);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Alpha Is Transparency
        EditorGUILayout.BeginHorizontal();
        {
            overrideAlphaIsTransparency = EditorGUILayout.ToggleLeft("Alpha Is Transparency", overrideAlphaIsTransparency, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideAlphaIsTransparency);
            {
                compressSettings.alphaIsTransparency = EditorGUILayout.ToggleLeft("Enable", compressSettings.alphaIsTransparency);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Read/Write
        EditorGUILayout.BeginHorizontal();
        {
            overrideReadable = EditorGUILayout.ToggleLeft("Read/Write", overrideReadable, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideReadable);
            {
                compressSettings.readable = EditorGUILayout.ToggleLeft("Enable", compressSettings.readable);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Generate Mip Maps
        EditorGUILayout.BeginHorizontal();
        {
            overrideGenerateMipMaps = EditorGUILayout.ToggleLeft("Generate Mip Maps", overrideGenerateMipMaps, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideGenerateMipMaps);
            {
                compressSettings.mipmapEnabled = EditorGUILayout.ToggleLeft("Enable", compressSettings.mipmapEnabled);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Wrap Mode
        EditorGUILayout.BeginHorizontal();
        {
            overrideWrapMode = EditorGUILayout.ToggleLeft("Wrap Mode", overrideWrapMode, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideWrapMode);
            {
                compressSettings.wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup(compressSettings.wrapMode);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Filter Mode
        EditorGUILayout.BeginHorizontal();
        {
            overrideFilterMode = EditorGUILayout.ToggleLeft("Filter Mode", overrideFilterMode, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideFilterMode);
            {
                compressSettings.filterMode = (UnityEngine.FilterMode)EditorGUILayout.EnumPopup(compressSettings.filterMode);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        //override for current platform
        EditorGUILayout.BeginHorizontal();
        {
            overrideForTarget = EditorGUILayout.ToggleLeft($"Override For {EditorUserBuildSettings.activeBuildTarget}", overrideForTarget, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideForTarget);
            {
                compressPlatformSettings.overridden = EditorGUILayout.ToggleLeft("Enable", compressPlatformSettings.overridden);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Max Size
        EditorGUILayout.BeginHorizontal();
        {
            overrideMaxSize = EditorGUILayout.ToggleLeft("Max Size", overrideMaxSize, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideMaxSize);
            {
                compressPlatformSettings.maxTextureSize = EditorGUILayout.IntPopup(compressPlatformSettings.maxTextureSize, maxTextureSizeDisplayOptions, maxTextureSizeOptionValues);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Format
        EditorGUILayout.BeginHorizontal();
        {
            overrideFormat = EditorGUILayout.ToggleLeft("Format", overrideFormat, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideFormat);
            {
                compressPlatformSettings.format = (TextureImporterFormat)EditorGUILayout.IntPopup((int)compressPlatformSettings.format, formatDisplayOptions, formatValues);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        //Compresser Quality
        EditorGUILayout.BeginHorizontal();
        {
            overrideCompresserQuality = EditorGUILayout.ToggleLeft("Compresser Quality", overrideCompresserQuality, GUILayout.Width(150));
            EditorGUI.BeginDisabledGroup(!overrideCompresserQuality);
            {
                compressPlatformSettings.compressionQuality = EditorGUILayout.IntSlider(compressPlatformSettings.compressionQuality, 0, 100);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawCompressRawFileButtonsPanel()
    {
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
    }

    private void SwitchUIPanel(CompressToolMode mCompressMode)
    {
        switch (mCompressMode)
        {
            case CompressToolMode.RawFile:
                {
                    tinypngKeyScrollList = new ReorderableList(AppBuildSettings.Instance.CompressImgToolKeys, typeof(string), true, true, true, true);
                    tinypngKeyScrollList.drawHeaderCallback = DrawTinypngKeyScrollListHeader;
                    tinypngKeyScrollList.drawElementCallback = DrawTinypngKeyItem;
                }
                break;
            case CompressToolMode.UnityAsset:
                {
                    compressSettings = new TextureImporterSettings();
                    compressPlatformSettings = new TextureImporterPlatformSettings();

                    var getOptionsFunc = Utility.Assembly.GetType("UnityEditor.TextureImportValidFormats").GetMethod("GetPlatformTextureFormatValuesAndStrings", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var paramsObjs = new object[] { TextureImporterType.Sprite, EditorUserBuildSettings.activeBuildTarget, null, null };
                    getOptionsFunc.Invoke(null, paramsObjs);

                    formatValues = paramsObjs[2] as int[];
                    formatDisplayOptions = paramsObjs[3] as string[];
                }
                break;
        }
    }

    private void DrawRawFileModeSettingsPanel()
    {
        tinypngScrollListPos = EditorGUILayout.BeginScrollView(tinypngScrollListPos, GUILayout.Height(110));
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
                EditorGUILayout.MinMaxSlider(Utility.Text.Format("压缩质量({0}%-{1}%)", (int)AppBuildSettings.Instance.CompressImgToolQualityMinLv, (int)AppBuildSettings.Instance.CompressImgToolQualityLv), ref AppBuildSettings.Instance.CompressImgToolQualityMinLv, ref AppBuildSettings.Instance.CompressImgToolQualityLv, 0, 100);

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

    private void SaveSettings()
    {
        AppBuildSettings.Save();
    }

    private void RecoveryImages()
    {
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var backupRoot = UtilityBuiltin.ResPath.GetCombinePath(projectRoot, AppBuildSettings.Instance.CompressImgToolBackupDir);
        if (!Directory.Exists(backupRoot))
        {
            EditorUtility.DisplayDialog("提示", $"备份路径不存在:{backupRoot}", "OK");
            return;
        }
        var backupItems = Directory.GetDirectories(backupRoot, "*", SearchOption.TopDirectoryOnly);
        if (backupItems.Length < 1)
        {
            EditorUtility.DisplayDialog("提示", "没有备份记录", "OK");
            return;
        }
        var contents = new GUIContent[backupItems.Length];

        for (int i = 0; i < backupItems.Length; i++)
        {
            var item = Path.GetRelativePath(backupRoot, backupItems[i]);
            contents[i] = new GUIContent(item);
        }
        var dialogRect = new Rect(Event.current.mousePosition, Vector2.zero);

        EditorUtility.DisplayCustomMenu(dialogRect, contents, -1, (object userData, string[] options, int selected) =>
        {
            string backupName = options[selected];
            if (0 != EditorUtility.DisplayDialogComplex("还原备份", $"是否还原此备份:[{backupName}]?", "还原备份", "取消", null))
            {
                return;
            }
            var recoveryDir = UtilityBuiltin.ResPath.GetCombinePath(backupRoot, backupName);
            var imgList = GetAllImagesByDir(recoveryDir, recoveryDir);
            CopyFilesTo(imgList, recoveryDir, projectRoot);
        }, null);
    }

    private void CopyFilesTo(List<string> imgList, string srcRoot, string desRoot)
    {
        int totalCount = imgList.Count;
        int successCount = 0;
        for (int i = 0; i < totalCount; i++)
        {
            var item = imgList[i];
            var desFile = UtilityBuiltin.ResPath.GetCombinePath(desRoot, item);
            var desFileDir = Path.GetDirectoryName(desFile);
            if (!Directory.Exists(desFileDir))
            {
                Directory.CreateDirectory(desFileDir);
            }
            var srcFile = UtilityBuiltin.ResPath.GetCombinePath(srcRoot, item);
            if (!EditorUtility.DisplayCancelableProgressBar("还原进度", $"还原文件:{item}", i / (float)totalCount))
            {
                try
                {
                    File.Copy(srcFile, desFile, true);
                    successCount++;
                }
                catch (Exception e)
                {
                    Debug.LogWarningFormat("--------还原文件{0}失败:{1}", srcFile, e.Message);
                }
            }
            else
            {
                break;
            }
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("还原备份结束", $"共 {totalCount} 张图片{Environment.NewLine}成功还原 {successCount} 张{Environment.NewLine}还原失败 {totalCount - successCount} 张", "OK");
        AssetDatabase.Refresh();
    }

    private void BackupImages()
    {
        var itmList = GetAllImages();
        int totalImgCount = itmList.Count;
        if (0 != EditorUtility.DisplayDialogComplex("提示", $"确认开始备份已选 {totalImgCount} 张图片吗?", "确定备份", "取消", null))
        {
            return;
        }
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var backupDir = UtilityBuiltin.ResPath.GetCombinePath(projectRoot, AppBuildSettings.Instance.CompressImgToolBackupDir);

        if (string.IsNullOrWhiteSpace(AppBuildSettings.Instance.CompressImgToolBackupDir))
        {
            EditorUtility.DisplayDialog("错误", $"当前选择的备份路径无效:{Environment.NewLine}{AppBuildSettings.Instance.CompressImgToolBackupDir}", "OK");
            return;
        }
        var backupPath = UtilityBuiltin.ResPath.GetCombinePath(backupDir, DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));

        int successCount = 0;
        for (int i = 0; i < itmList.Count; i++)
        {
            var imgFile = itmList[i];
            var srcImg = Path.GetFullPath(imgFile, projectRoot);
            var desImg = Path.GetFullPath(imgFile, backupPath);
            try
            {
                if (EditorUtility.DisplayCancelableProgressBar($"备份进度({i}/{totalImgCount})", $"正在备份:{Environment.NewLine}{imgFile}", i / (float)totalImgCount))
                {
                    break;
                }
                string desFilePath = Path.GetDirectoryName(desImg);
                if (!Directory.Exists(desFilePath))
                {
                    Directory.CreateDirectory(desFilePath);
                }
                File.Copy(srcImg, desImg, true);
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("---------备份图片{0}失败:{1}", imgFile, e.Message);
            }
        }

        EditorUtility.ClearProgressBar();

        if (0 == EditorUtility.DisplayDialogComplex("备份结束", $"共 {totalImgCount} 张图片{Environment.NewLine}成功备份  {successCount} 张{Environment.NewLine}备份失败 {totalImgCount - successCount} 张", "打开备份目录", "关闭", null))
        {
            EditorUtility.RevealInFinder(backupPath);
            GUIUtility.ExitGUI();
        }
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
        if (imgList.Count < 1) return;
        if (AppBuildSettings.Instance.CompressImgToolKeys != null && AppBuildSettings.Instance.CompressImgToolKeys.Count > 0 && !string.IsNullOrWhiteSpace(AppBuildSettings.Instance.CompressImgToolKeys[0]))
        {
            TinifyAPI.Tinify.Key = AppBuildSettings.Instance.CompressImgToolKeys[0];
        }

        if (!AppBuildSettings.Instance.CompressImgToolOffline && string.IsNullOrWhiteSpace(TinifyAPI.Tinify.Key))
        {
            EditorUtility.DisplayDialog("错误", "TinyPng Key无效,可前往tinypng.com获取.", "OK");
            return;
        }
        int clickBtIdx = EditorUtility.DisplayDialogComplex("请确认", Utility.Text.Format("共 {0} 张图片待压缩, 是否开始压缩?", imgList.Count), "开始压缩", "取消", null);
        if (clickBtIdx != 0)
        {
            //用户取消压缩
            return;
        }

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
            var imgFileName = Utility.Path.GetRegularPath(Path.GetFullPath(imgName, rootPath));
            var outputFileName = Utility.Path.GetRegularPath(Path.GetFullPath(imgName, outputPath));
            var outputFilePath = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outputFilePath))
            {
                Directory.CreateDirectory(outputFilePath);
            }
            if (EditorUtility.DisplayCancelableProgressBar(Utility.Text.Format("压缩进度({0}/{1})", totalCount - imgList.Count, totalCount), Utility.Text.Format("正在压缩:{0}", imgName), (totalCount - i) / (float)totalCount))
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
        var clickBtIdx = EditorUtility.DisplayDialogComplex("警告", Utility.Text.Format("有 {0} 张图片压缩失败, 是否继续压缩?", imgList.Count), "继续压缩", "取消", null);
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
        if (string.IsNullOrWhiteSpace(TinifyAPI.Tinify.Key))
        {
            return false;
        }

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
        strBuilder.AppendFormat(" --force --quality {0}-{1}", (int)AppBuildSettings.Instance.CompressImgToolQualityMinLv, (int)AppBuildSettings.Instance.CompressImgToolQualityLv);
        strBuilder.AppendFormat(" --speed {0}", AppBuildSettings.Instance.CompressImgToolFastLv);
        strBuilder.AppendFormat(" --output \"{0}\"", outputFileName);
        strBuilder.AppendFormat(" -- \"{0}\"", imgFileName);

        var proceInfo = new System.Diagnostics.ProcessStartInfo(pngquant, strBuilder.ToString());
        proceInfo.CreateNoWindow = true;
        proceInfo.UseShellExecute = false;
        bool success;
        using (var proce = System.Diagnostics.Process.Start(proceInfo))
        {
            proce.WaitForExit();
            success = proce.ExitCode == 0;
            if (!success)
            {
                Debug.LogWarningFormat("离线压缩图片:{0}失败,ExitCode:{1}", imgFileName, proce.ExitCode);
            }
        }
        return success;
    }
    /// <summary>
    /// 获取当前添加的所有图片(相对项目路径)
    /// </summary>
    /// <returns></returns>
    private List<string> GetAllImages()
    {
        List<string> images = new List<string>();
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
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
                string imgFolder = Path.GetFullPath(AssetDatabase.GetAssetPath(item), projectRoot);

                if (Directory.Exists(imgFolder))
                {
                    var allImgFiles = GetAllImagesByDir(imgFolder, projectRoot);
                    images.AddRange(allImgFiles);
                }
            }
        }

        return images.Distinct().ToList();//把结果去重处理
    }
    /// <summary>
    /// 获取绝对路径下的所有图片, 图片路径相对于baseFolder
    /// </summary>
    /// <param name="imgFolder"></param>
    /// <param name="baseFolder"></param>
    /// <returns></returns>
    private List<string> GetAllImagesByDir(string imgFolder, string baseFolder)
    {
        var images = new List<string>();
        if (!string.IsNullOrWhiteSpace(imgFolder) && Directory.Exists(imgFolder))
        {
            var allFiles = Directory.GetFiles(imgFolder, "*.*", SearchOption.AllDirectories);
            foreach (var item in allFiles)
            {
                var fileName = Utility.Path.GetRegularPath(Path.GetRelativePath(baseFolder, item));
                if (CheckSupportImageFile(fileName) && !images.Contains(fileName))
                {
                    images.Add(fileName);
                }
            }
        }
        return images;
    }
    private void DrawDropArea()
    {
        var dragRect = EditorGUILayout.BeginVertical("box");
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(dragAreaContent, centerLabelStyle, GUILayout.MinHeight(200));
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
            GUILayout.FlexibleSpace();
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
    private bool CheckSupportImageFile(string fileName)
    {
        switch ((CompressToolMode)AppBuildSettings.Instance.CompressImgMode)
        {
            case CompressToolMode.RawFile:
                {
                    var ext = Path.GetExtension(fileName).ToLower();
                    if (ArrayUtility.Contains(SupportImgTypes, ext))
                    {
                        return true;
                    }
                }
                break;
            case CompressToolMode.UnityAsset:
                {
                    var assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileName);
                    if (assetObj != null)
                    {
                        var itemTp = assetObj.GetType();
                        return itemTp == typeof(Sprite) || itemTp == typeof(Texture) || itemTp == typeof(Texture2D);
                    }

                }
                break;
        }
        return false;
    }
    private ItemType CheckItemType(UnityEngine.Object item)
    {
        if (item == null) return ItemType.NoSupport;
        var name = AssetDatabase.GetAssetPath(item);
        if ((File.GetAttributes(name) & FileAttributes.Directory) == FileAttributes.Directory)
        {
            return ItemType.Folder;
        }
        if (CheckSupportImageFile(name)) return ItemType.Image;

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
        GUI.Label(rect, "添加要处理的文件或文件夹:");
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
