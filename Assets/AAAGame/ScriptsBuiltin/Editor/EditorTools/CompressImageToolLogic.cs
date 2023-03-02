using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;

public class CompressImageToolLogic
{
    public static void GenerateAtlasVariant(string atlasFile, TextureImporterFormat format)
    {
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFile);
        if (atlas == null || atlas.isVariant) return;

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
        platformSettings.format = format;
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
    }
    public static void PackAtlases(SpriteAtlas[] spriteAtlas)
    {
        SpriteAtlasUtility.PackAtlases(spriteAtlas, EditorUserBuildSettings.activeBuildTarget);
    }
    public static void GenerateAtlasVariant(List<string> atlasFiles, TextureImporterFormat format)
    {
        int totalCount = atlasFiles.Count;
        for (int i = 0; i < totalCount; i++)
        {
            var atlasFile = atlasFiles[i];
            GenerateAtlasVariant(atlasFile, format);
        }
        EditorUtility.ClearProgressBar();
    }
}
