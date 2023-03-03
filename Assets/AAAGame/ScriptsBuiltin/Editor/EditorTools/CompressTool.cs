using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using GameFramework;

namespace GameFramework.Editor
{
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
        public float variantScale = 1f;
        public override void Clear()
        {
            base.Clear();
            variantScale = 1f;
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

