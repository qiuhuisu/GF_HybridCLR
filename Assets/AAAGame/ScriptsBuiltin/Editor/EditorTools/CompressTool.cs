using UnityEditor.U2D;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TinifyAPI;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace GameFramework.Editor
{
    //public class TextureSettings : IReference
    //{
    //    public TextureImporterType? TextureType;
    //    public SpriteMeshType? MeshType;
    //    public bool? AlphaIsTransparency;
    //    public bool? Readable;
    //    public bool? GenerateMipMaps;
    //    public WrapMode? WrapMode;
    //    public FilterMode? FilterMode;
    //    public bool? overrideForTarget;
    //    public int? MaxSize;
    //    public TextureImporterFormat? TexFormat;
    //    public int? CompresserQuality;

    //    public void Clear()
    //    {
    //        TextureType = null;
    //        MeshType = null;
    //        AlphaIsTransparency = null;
    //        Readable = null;
    //        GenerateMipMaps = null;
    //        WrapMode = null;
    //        FilterMode = null;
    //        overrideForTarget = null;
    //        MaxSize = null;
    //        TexFormat = null;
    //        CompresserQuality = null;
    //    }
    //}
    public class AtlasSettings : IReference
    {
        public bool? includeInBuild = null;
        public bool? allowRotation = null;
        public bool? tightPacking = null;
        public bool? alphaDilation = null;
        public int? padding = null;
        public bool? readWrite = null;
        public bool? mipMaps = null;
        public bool? sRGB = null;
        public FilterMode? filterMode = null;
        public int? maxTexSize = null;
        public TextureImporterFormat? texFormat = null;
        public int? compressQuality = null;
        public virtual void Clear()
        {
            includeInBuild = null;
            allowRotation = null;
            tightPacking = null;
            alphaDilation = null;
            padding = null;
            readWrite = null;
            mipMaps = null;
            sRGB = null;
            filterMode = null;
            maxTexSize = null;
            texFormat = null;
            compressQuality = null;
        }
    }
    public class AtlasVariantSettings : AtlasSettings
    {
        public float variantScale = 0.5f;
        public override void Clear()
        {
            base.Clear();
            variantScale = 0.5f;
        }
        public static AtlasVariantSettings CreateFrom(AtlasSettings atlasSettings, float scale = 1f)
        {
            var settings = ReferencePool.Acquire<AtlasVariantSettings>();
            settings.includeInBuild = atlasSettings.includeInBuild;
            settings.allowRotation = atlasSettings.allowRotation;
            settings.tightPacking = atlasSettings.tightPacking;
            settings.alphaDilation = atlasSettings.alphaDilation;
            settings.padding = atlasSettings.padding;
            settings.readWrite = atlasSettings.readWrite;
            settings.mipMaps = atlasSettings.mipMaps;
            settings.sRGB = atlasSettings.sRGB;
            settings.filterMode = atlasSettings.filterMode;
            settings.maxTexSize = atlasSettings.maxTexSize;
            settings.texFormat = atlasSettings.texFormat;
            settings.compressQuality = atlasSettings.compressQuality;
            settings.variantScale = scale;
            return settings;
        }
    }
    public class CompressTool
    {
#if UNITY_EDITOR_WIN
        const string pngquantTool = "Tools/CompressImageTools/pngquant_win/pngquant.exe";
#elif UNITY_EDITOR_OSX
        const string pngquantTool = "Tools/CompressImageTools/pngquant_mac/pngquant";
#endif
        /// <summary>
        /// 使用TinyPng在线压缩,支持png,jpg,webp
        /// </summary>
        public static async Task<bool> CompressOnlineAsync(string imgFileName, string outputFileName, string tinypngKey)
        {
            if (string.IsNullOrWhiteSpace(tinypngKey))
            {
                return false;
            }
            Tinify.Key = tinypngKey;
            var srcImg = TinifyAPI.Tinify.FromFile(imgFileName);
            await srcImg.ToFile(outputFileName);
            return srcImg.IsCompletedSuccessfully;
        }

        /// <summary>
        /// 使用pngquant离线压缩,只支持png
        /// </summary>
        public static bool CompressImageOffline(string imgFileName, string outputFileName)
        {
            var fileExt = Path.GetExtension(imgFileName).ToLower();
            switch (fileExt)
            {
                case ".png":
                    return CompressPngOffline(imgFileName, outputFileName);
                case ".jpg":
                    return CompressJpgOffline(imgFileName, outputFileName);
            }
            return false;
        }
        /// <summary>
        /// 按比例缩放图片尺寸
        /// </summary>
        /// <param name="imgFileName"></param>
        /// <param name="outputFileName"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static bool ResizeImage(string imgFileName, string outputFileName, float scale)
        {
            using (var img = SixLabors.ImageSharp.Image.Load(imgFileName))
            {
                int scaleWidth = (int)(img.Width * scale);
                int scaleHeight = (int)(img.Height * scale);
                img.Mutate(x => x.Resize(scaleWidth, scaleHeight));
                img.Save(outputFileName);
            }
            return true;
        }
        /// <summary>
        /// 设置图片尺寸
        /// </summary>
        /// <param name="imgFileName"></param>
        /// <param name="outputFileName"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static bool ResizeImage(string imgFileName, string outputFileName, int width, int height)
        {
            using (var img = SixLabors.ImageSharp.Image.Load(imgFileName))
            {
                img.Mutate(x => x.Resize(width, height));
                img.Save(outputFileName);
            }
            return true;
        }
        /// <summary>
        /// 使用ImageSharp压缩jpg图片
        /// </summary>
        /// <param name="imgFileName"></param>
        /// <param name="outputFileName"></param>
        /// <returns></returns>
        private static bool CompressJpgOffline(string imgFileName, string outputFileName)
        {
            using (var img = SixLabors.ImageSharp.Image.Load(imgFileName))
            {
                var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder()
                {
                    Quality = (int)AppBuildSettings.Instance.CompressImgToolQualityLv
                };
                using (var outputStream = new FileStream(outputFileName, FileMode.Create))
                {
                    img.Save(outputStream, encoder);
                }

            }

            return true;
        }
        /// <summary>
        /// 使用pngquant压缩png图片
        /// </summary>
        /// <param name="imgFileName"></param>
        /// <param name="outputFileName"></param>
        /// <returns></returns>
        private static bool CompressPngOffline(string imgFileName, string outputFileName)
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
        /// 创建图集
        /// </summary>
        /// <param name="atlasFilePath"></param>
        /// <param name="settings"></param>
        /// <param name="objectsForPack"></param>
        /// <param name="createAtlasVariant"></param>
        /// <param name="atlasVariantScale"></param>
        /// <returns></returns>
        public static SpriteAtlas CreateAtlas(string atlasFilePath, AtlasSettings settings, UnityEngine.Object[] objectsForPack, bool createAtlasVariant = false, float atlasVariantScale = 1f)
        {
            var atlas = new SpriteAtlas();
            atlas.SetIncludeInBuild(settings.includeInBuild ?? true);
            atlas.Add(objectsForPack);

            AssetDatabase.CreateAsset(atlas, atlasFilePath);

            var atlasAsset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFilePath);
            if (createAtlasVariant)
            {
                CreateAtlasVariant(atlasAsset, AtlasVariantSettings.CreateFrom(settings, atlasVariantScale));
            }
            return atlasAsset;
        }
        /// <summary>
        /// 根据图集对象生成图集变体
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static SpriteAtlas CreateAtlasVariant(SpriteAtlas atlas, AtlasVariantSettings settings)
        {
            if (atlas == null || atlas.isVariant) return atlas;
            var atlasFileName = AssetDatabase.GetAssetPath(atlas);
            if (string.IsNullOrEmpty(atlasFileName))
            {
                Debug.LogError($"atlas '{atlas.name}' is not a asset file.");
                return null;
            }

            var atlasVariant = UtilityBuiltin.ResPath.GetCombinePath(Path.GetDirectoryName(atlasFileName), $"{Path.GetFileNameWithoutExtension(atlasFileName)}_Variant{Path.GetExtension(atlasFileName)}");
            SpriteAtlas varAtlas;
            if (File.Exists(atlasVariant))
            {
                varAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasVariant);
            }
            else
            {
                var newAtlas = new SpriteAtlas();
                EditorUtility.CopySerialized(atlas, newAtlas);
                AssetDatabase.CreateAsset(newAtlas, atlasVariant);
                varAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasVariant);
            }
            atlas.SetIncludeInBuild(false);
            var atlasSettings = atlas.GetPackingSettings();
            if (settings.allowRotation != null) atlasSettings.enableRotation = settings.allowRotation.Value;
            if (settings.tightPacking != null) atlasSettings.enableTightPacking = settings.tightPacking.Value;
            if (settings.alphaDilation != null) atlasSettings.enableAlphaDilation = settings.alphaDilation.Value;
            if (settings.padding != null) atlasSettings.padding = settings.padding.Value;
            atlas.SetPackingSettings(atlasSettings);

            var atlasTexSettings = atlas.GetTextureSettings();
            if (settings.readWrite != null) atlasTexSettings.readable = settings.readWrite.Value;
            if (settings.mipMaps != null) atlasTexSettings.generateMipMaps = settings.mipMaps.Value;
            if (settings.sRGB != null) atlasTexSettings.sRGB = settings.sRGB.Value;
            if (settings.filterMode != null) atlasTexSettings.filterMode = settings.filterMode.Value;
            atlas.SetTextureSettings(atlasTexSettings);

            if (settings.maxTexSize != null)
            {
                var platformSettings = atlas.GetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
                platformSettings.overridden = true;
                platformSettings.maxTextureSize = settings.maxTexSize.Value;
                atlas.SetPlatformSettings(platformSettings);
            }
            EditorUtility.SetDirty(atlas);

            varAtlas.SetIsVariant(true);
            varAtlas.SetMasterAtlas(atlas);
            varAtlas.SetIncludeInBuild(true);
            varAtlas.SetVariantScale(settings.variantScale);

            bool hasChanged = settings.texFormat != null || settings.compressQuality != null;
            if (hasChanged)
            {
                var pSettings = varAtlas.GetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
                pSettings.overridden = true;
                if (settings.texFormat != null) pSettings.format = settings.texFormat.Value;
                if (settings.compressQuality != null) pSettings.compressionQuality = settings.compressQuality.Value;
                varAtlas.SetPlatformSettings(pSettings);
                EditorUtility.SetDirty(varAtlas);
            }
            AssetDatabase.SaveAssetIfDirty(varAtlas);
            return varAtlas;
        }
        /// <summary>
        /// 根据Atlas文件名为Atlas生成Atlas变体(Atlas Variant)
        /// </summary>
        /// <param name="atlasFile"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static SpriteAtlas CreateAtlasVariant(string atlasFile, AtlasVariantSettings settings)
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFile);

            return CreateAtlasVariant(atlas, settings);
        }

        /// <summary>
        /// 批量重新打包图集
        /// </summary>
        /// <param name="spriteAtlas"></param>
        public static void PackAtlases(SpriteAtlas[] spriteAtlas)
        {
            SpriteAtlasUtility.PackAtlases(spriteAtlas, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}

