using GameFramework;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace UnityGameFramework.Editor
{
    public sealed class AssetBuildHandler : IBuildEventHandler
    {
        private VersionInfo outputVersionInfo = null;
        public bool ContinueOnFailure
        {
            get
            {
                return false;
            }
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {

        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {

        }



        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            //打包完成后把文件复制到StreamingAssets目录
            string targetPath = string.Empty;
            bool copyToStreamingAssets = false;
            if (outputPackageSelected)
            {
                targetPath = outputPackagePath;
                copyToStreamingAssets = true;
            }
            else if (outputPackedSelected)
            {
                targetPath = outputPackedPath;
                copyToStreamingAssets = true;
            }
            else if (outputFullSelected)
            {
                targetPath = outputFullPath;
            }
            if (string.IsNullOrEmpty(targetPath))
            {
                Debug.LogErrorFormat("targetPath is null.");
                return;
            }
            if (copyToStreamingAssets)
            {
                string[] fileNames = Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories);
                string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
                foreach (string fileName in fileNames)
                {
                    var abAssetName = fileName.Substring(targetPath.Length);
                    string destFileName = Path.Combine(streamingAssetsPath, abAssetName);
                    FileInfo destFileInfo = new FileInfo(destFileName);
                    if (!destFileInfo.Directory.Exists)
                    {
                        destFileInfo.Directory.Create();
                    }
                    File.Copy(fileName, destFileName);
                }
            }
            
            if (isSuccess)
            {
                if (outputFullSelected || outputFullSelected)
                {
                    EditorUtility.RevealInFinder(targetPath);
                }
            }
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion, BuildAssetBundleOptions buildAssetBundleOptions, bool zip, string outputDirectory, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {

        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            MyGameTools.RefreshABDependencyAssets();
#if !DISABLE_HYBRIDCLR
            MyGameTools.CompileTargetDll();
#endif
            string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
            if (Directory.Exists(streamingAssetsPath))
            {
                Directory.Delete(streamingAssetsPath, true);
                //string[] fileNames = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);
                //foreach (string fileName in fileNames)
                //{
                //    if (fileName.Contains(".gitkeep"))
                //    {
                //        continue;
                //    }

                //    File.Delete(fileName);
                //}
            }
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            if (outputVersionInfo != null && (outputFullSelected || outputPackedSelected))
            {
                outputVersionInfo.InternalResourceVersion = internalResourceVersion;
                outputVersionInfo.ApplicableGameVersion = applicableGameVersion;
                string targetPath = outputFullSelected ? outputFullPath : outputPackedPath;
                //string versionFileName = string.Format("{0}_{1}", platforms.ToString(), ConstBuiltin.VersionFile);
                string outputFileName = UtilityBuiltin.ResPath.GetCombinePath(targetPath, platforms.ToString(), ConstBuiltin.VersionFile);
                try
                {
                    File.WriteAllText(outputFileName, LitJson.JsonMapper.ToJson(outputVersionInfo));
                    Debug.LogFormat("成功生成资源信息文件:{0}", outputFileName);
                }
                catch (System.Exception e)
                {
                    Debug.LogFormat("生成资源信息文件失败:{0}", e.Message);
                    throw;
                }

            }
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            outputVersionInfo = new VersionInfo()
            {
                UpdatePrefixUri = UtilityBuiltin.ResPath.GetCombinePath(ConstBuiltin.DefaultHotFixUrl, platform.ToString()),
                VersionListHashCode = versionListHashCode,
                VersionListLength = versionListLength,
                VersionListCompressedHashCode = versionListCompressedHashCode,
                VersionListCompressedLength = versionListCompressedLength,
            };
        }
    }
}
