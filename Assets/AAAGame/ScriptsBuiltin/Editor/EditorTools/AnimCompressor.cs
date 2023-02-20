//****************************************************************************
//
//  动画文件(AnimationClip)压缩工具
//
//  Create by jiangcheng_m
//
//  注意:同一个模型的动画文件必须放到同一个文件夹下
//  压缩原理
//  1.分析得到同一个模型被缩放的所有骨骼
//  2.通过1的结果删除动画文件中没有缩放的骨骼的localscale属性和曲线
//  3.删除内容相差万分之一的中间关键帧只保留首位两帧(减少采样频率)
//  4.优化帧属性值精度
//****************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Cci;

namespace CompressTool
{
    public static class SETTING
    {
        public static class FILTER
        {
            //缩放属性剔除(变化误差范围 x 分之一)
            public readonly static int ERR_RANGE_SCALE_PROPERTY = 1000;
            //相同关键帧剔(除误差范围 X 分之一)
            public readonly static int ERR_RANGE_SAME_FRAME = 10000;
        }

        //精度压缩(根据曲线变化坡度压缩,坡度越大精度越高,坡度越小精度越小)
        public static class ACCURACY
        {
            //精度1级 坡度阀值
            public readonly static float THRESHOLD1 = 0;
            //精度2级 坡度阀值
            public readonly static float THRESHOLD2 = 0.1f;
            //精度1级(小数点后3位)
            public readonly static string LEVEL1 = "f3";
            //精度2级(小数点后4位)
            public readonly static string LEVEL2 = "f4";
            //精度3级(小数点后5位)
            public readonly static string LEVEL3 = "f5";
        }
    }


    public class CompressOpt
    {
        public AnimationClip AnimClip { private set; get; }
        public string AnimClipPath { private set; get; }
        private HashSet<string> mScaleBonePaths;
        private Dictionary<string, float> mGradientVals;
        public CompressOpt(AnimationClip animClip, string animClipPath)
        {
            AnimClip = animClip;
            AnimClipPath = animClipPath;
            mGradientVals = new Dictionary<string, float>();
        }

        public void SetScaleBonePaths(HashSet<string> scaleBonePaths)
        {
            mScaleBonePaths = scaleBonePaths;
        }

        private bool Approximately(Keyframe a, Keyframe b)
        {
            return Mathf.Abs(a.value - b.value) * SETTING.FILTER.ERR_RANGE_SAME_FRAME < 1f &&
                Mathf.Abs(a.inTangent - b.inTangent) * SETTING.FILTER.ERR_RANGE_SAME_FRAME < 1f &&
                Mathf.Abs(a.outTangent - b.outTangent) * SETTING.FILTER.ERR_RANGE_SAME_FRAME < 1f &&
                Mathf.Abs(a.inWeight - b.inWeight) * SETTING.FILTER.ERR_RANGE_SAME_FRAME < 1f &&
                Mathf.Abs(a.outWeight - b.outWeight) * SETTING.FILTER.ERR_RANGE_SAME_FRAME < 1f;
        }


        private string GetCurveKey(string path, string propertyName)
        {
            var splits = propertyName.Split('.');
            var name = splits[0];
            return string.Format("{0}/{1}", path, name);
        }

        //获取曲线坡度
        private float GetCurveThreshold(string path, string propertyName)
        {
            var curveKey = GetCurveKey(path, propertyName);
            float threshold = 0;
            mGradientVals.TryGetValue(curveKey, out threshold);
            return threshold;
        }

        //设置曲线坡度
        private void SetCurveThreshold(string path, string propertyName, float threshold)
        {
            var curveKey = GetCurveKey(path, propertyName);
            if (!mGradientVals.ContainsKey(curveKey))
                mGradientVals.Add(curveKey, threshold);
            else
                mGradientVals[curveKey] = threshold;
        }

        //获取曲线压缩精度
        private string GetCompressAccuracy(string path, string propertyName)
        {
            var threshold = GetCurveThreshold(path, propertyName);
            if (threshold <= SETTING.ACCURACY.THRESHOLD1)
                return SETTING.ACCURACY.LEVEL1;
            else if (threshold <= SETTING.ACCURACY.THRESHOLD2)
                return SETTING.ACCURACY.LEVEL2;
            return SETTING.ACCURACY.LEVEL3;
        }

        public void Compress()
        {
            if (AnimClip != null)
            {
                var curveBindings = AnimationUtility.GetCurveBindings(AnimClip);
                for (int i = 0; i < curveBindings.Length; i++)
                {
                    EditorCurveBinding curveBinding = curveBindings[i];
                    float threshold = GetCurveThreshold(curveBinding.path, curveBinding.propertyName);

                    string name = curveBinding.propertyName.ToLower();
                    var curve = AnimationUtility.GetEditorCurve(AnimClip, curveBinding);
                    var keys = curve.keys;
                    if (name.Contains("scale"))
                    {
                        //优化scale曲线
                        if (!mScaleBonePaths.Contains(curveBinding.path))
                        {
                            AnimationUtility.SetEditorCurve(AnimClip, curveBinding, null);
                            continue;
                        }
                    }

                    float bottomVal = 999999;
                    float topVal = -999999;

                    //优化采样点数量
                    List<Keyframe> newFrames = new List<Keyframe>();
                    if (keys.Length > 0)
                    {
                        newFrames.Add(keys[0]);
                        var lastSameFrameIndex = 0;
                        var comparerFrameIndex = 0;
                        for (int j = 1; j < keys.Length; j++)
                        {
                            var curFrame = keys[j];
                            var comparerFrame = keys[comparerFrameIndex];
                            if (Approximately(curFrame, comparerFrame))
                            {
                                lastSameFrameIndex = j;
                            }
                            else
                            {
                                if (lastSameFrameIndex > comparerFrameIndex)
                                    newFrames.Add(keys[lastSameFrameIndex]);
                                newFrames.Add(keys[j]);
                                comparerFrameIndex = j;
                            }
                            bottomVal = Mathf.Min(bottomVal, keys[j].value);
                            topVal = Mathf.Max(topVal, keys[j].value);
                        }

                        if (newFrames.Count == 1)
                            newFrames.Add(keys[keys.Length - 1]);//最少两帧

                        if (newFrames.Count != keys.Length)
                        {
                            curve.keys = newFrames.ToArray();
                            //Debug.LogFormat("{0}=>{1}", keys.Length, newFrames.Count);
                            AnimationUtility.SetEditorCurve(AnimClip, curveBinding, curve);
                        }
                    }

                    SetCurveThreshold(curveBinding.path, curveBinding.propertyName, Mathf.Max(threshold, topVal - bottomVal));
                }

                //优化精度
                AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(AnimClip);
                if (curves != null && curves.Length > 0)
                {
                    for (int i = 0; i < curves.Length; i++)
                    {
                        AnimationClipCurveData curveDate = curves[i];
                        if (curveDate.curve == null || curveDate.curve.keys == null)
                            continue;

                        string accuracy = GetCompressAccuracy(curveDate.path, curveDate.propertyName);
                        Keyframe[] keyFrames = curveDate.curve.keys;
                        for (int j = 0; j < keyFrames.Length; j++)
                        {
                            Keyframe key = keyFrames[j];
                            key.value = float.Parse(key.value.ToString(accuracy));
                            //切线固定精度
                            key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                            key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                            keyFrames[j] = key;
                        }
                        curveDate.curve.keys = keyFrames;
                        AnimClip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
                    }
                }
            }
        }
    }


    public class AnimClipDirectory
    {
        public string Path { get; }
        public List<string> AnimClipPaths { get; private set; }
        public List<CompressOpt> CompressOpts { get; private set; }
        public AnimClipDirectory(string directory)
        {
            Path = directory;
            AnimClipPaths = new List<string>();
            CompressOpts = new List<CompressOpt>();
        }

        public void AddAnimClipPath(string animClipPath)
        {
            AnimClipPaths.Add(animClipPath);
        }

        //分析被缩放的所有骨骼路径
        public void Analyse()
        {
            HashSet<string> scaleBonePaths = new HashSet<string>();
            for (int i = 0; i < AnimClipPaths.Count; i++)
            {
                var assetPath = AnimClipPaths[i];
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                CompressOpts.Add(new CompressOpt(clip, assetPath));
                AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(clip);
                if (curves != null && curves.Length > 0)
                {
                    for (int j = 0; j < curves.Length; j++)
                    {
                        string name = curves[j].propertyName.ToLower();
                        if (name.Contains("scale"))
                        {
                            AnimationClipCurveData curveDate = curves[j];
                            if (curveDate.curve == null || curveDate.curve.keys == null)
                                continue;
                            var keyFrames = curveDate.curve.keys;
                            bool isScaleChanged = false;
                            if (keyFrames.Length > 0)
                            {
                                var frist = keyFrames[0].value;
                                if (Mathf.Abs(frist - 1f) * SETTING.FILTER.ERR_RANGE_SCALE_PROPERTY > 1f) //如果第一帧大小变了
                                    isScaleChanged = true;
                                else
                                {
                                    for (int k = 1; k < keyFrames.Length; k++)
                                    {
                                        if (Mathf.Abs(keyFrames[k].value - frist) * SETTING.FILTER.ERR_RANGE_SCALE_PROPERTY > 1f) //如果差异超过千分之一,则不可删除
                                        {
                                            isScaleChanged = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (isScaleChanged)
                                scaleBonePaths.Add(curves[j].path);
                        }
                    }
                }
            }

            for (int i = 0; i < CompressOpts.Count; i++)
            {
                CompressOpts[i].SetScaleBonePaths(scaleBonePaths);
            }

        }
    }


    public class AnimClipCompressTool
    {
        private enum ProcessType
        {
            Analyse,
            Compress,
            Finish,
        }
        private static ProcessType mCurProcess;
        private static List<AnimClipDirectory> mAnimClipDirectoryList;
        private static List<CompressOpt> mCompressOptList;
        private static int mIndex = 0;

        [MenuItem("Assets/TA/Compress AnimationClip Float", priority = 2002)]
        static void OptimizeFloat()
        {
            var selectObjs = AssetDatabase.FindAssets("t:AnimationClip");
            string pattern = @"(\d+\.\d+)";


            foreach (var item in selectObjs)
            {
                var itmName = AssetDatabase.GUIDToAssetPath(item);
                if (File.GetAttributes(itmName) != FileAttributes.ReadOnly)
                {
                    var allTxt = File.ReadAllText(itmName);
                    // 将匹配到的浮点型数字替换为精确到3位小数的浮点型数字
                    string outputString = Regex.Replace(allTxt, pattern, match =>
                    float.Parse(match.Value).ToString("F3"));
                    File.WriteAllText(itmName, outputString);
                    Debug.LogFormat("----->压缩动画浮点精度:{0}", itmName);
                }
            }
            AssetDatabase.Refresh();
        }
        [MenuItem("Assets/TA/Compress AnimationClip", priority = 2001)]
        public static void Optimize()
        {
            Dictionary<string, AnimClipDirectory> animClipPaths = new Dictionary<string, AnimClipDirectory>();
            var selectObjs = AssetDatabase.FindAssets("t:AnimationClip");// Selection.objects;
            if (selectObjs != null && selectObjs.Length > 0)
            {
                for (int i = 0; i < selectObjs.Length; i++)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(selectObjs[i]);
                    if (File.GetAttributes(assetPath) != FileAttributes.ReadOnly)
                    {
                        GetAllAnimClipPaths(assetPath, ref animClipPaths);

                    }
                }
            }

            mAnimClipDirectoryList = new List<AnimClipDirectory>();
            mAnimClipDirectoryList.AddRange(animClipPaths.Values);
            mCompressOptList = new List<CompressOpt>();

            mIndex = 0;
            mCurProcess = ProcessType.Analyse;

            if (mAnimClipDirectoryList.Count > 0)
                EditorApplication.update = Update;
            else
                EditorUtility.DisplayDialog("Tips", "can not found AnimationClip file!", "ok");
        }

        private static void Update()
        {
            if (mCurProcess == ProcessType.Analyse)
            {
                AnimClipDirectory animClipDirectory = mAnimClipDirectoryList[mIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar(string.Format("正在读取AnimationClip文件夹信息[{0}/{1}])", mIndex, mAnimClipDirectoryList.Count), animClipDirectory.Path, (float)mIndex / (float)mAnimClipDirectoryList.Count);
                if (isCancel)
                    mCurProcess = ProcessType.Compress;
                else
                {
                    animClipDirectory.Analyse();
                    mIndex++;
                    if (mIndex >= mAnimClipDirectoryList.Count)
                    {
                        for (int i = 0; i < mAnimClipDirectoryList.Count; i++)
                            mCompressOptList.AddRange(mAnimClipDirectoryList[i].CompressOpts);

                        if (mCompressOptList.Count > 0)
                            mCurProcess = ProcessType.Compress;
                        else
                            mCurProcess = ProcessType.Finish;
                        mIndex = 0;
                    }
                }
            }
            else if (mCurProcess == ProcessType.Compress)
            {
                CompressOpt compressOpt = mCompressOptList[mIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar(string.Format("正在压缩AnimationClip文件[{0}/{1}]", mIndex, mCompressOptList.Count), compressOpt.AnimClipPath, (float)mIndex / (float)mCompressOptList.Count);
                if (isCancel)
                    mCurProcess = ProcessType.Finish;
                else
                {
                    compressOpt.Compress();
                    mIndex++;
                    if (mIndex >= mCompressOptList.Count)
                        mCurProcess = ProcessType.Finish;
                }
            }
            else if (mCurProcess == ProcessType.Finish)
            {
                mAnimClipDirectoryList = null;
                mCompressOptList = null;
                mIndex = 0;
                EditorUtility.ClearProgressBar();
                Resources.UnloadUnusedAssets();
                GC.Collect();
                AssetDatabase.SaveAssets();
                EditorApplication.update = null;
            }


        }

        private static void GetAllAnimClipPaths(string assetPath, ref Dictionary<string, AnimClipDirectory> animClipPaths)
        {
            if (IsDirectory(assetPath))
            {
                if (!assetPath.Contains(".."))
                {
                    string[] paths = System.IO.Directory.GetFileSystemEntries(assetPath);
                    for (int i = 0; i < paths.Length; i++)
                    {
                        var path = paths[i];
                        if (IsDirectory(path))
                        {
                            GetAllAnimClipPaths(path, ref animClipPaths);
                        }
                        else
                        {
                            if (path.EndsWith(".anim"))
                            {
                                var directoryPath = GetFileDirectoryPath(path);
                                if (!animClipPaths.ContainsKey(directoryPath))
                                    animClipPaths.Add(directoryPath, new AnimClipDirectory(directoryPath));
                                animClipPaths[directoryPath].AddAnimClipPath(path);
                            }
                        }
                    }
                }
            }
            else
            {
                if (assetPath.EndsWith(".anim"))
                {
                    var directoryPath = GetFileDirectoryPath(assetPath);
                    if (!animClipPaths.ContainsKey(directoryPath))
                        animClipPaths.Add(directoryPath, new AnimClipDirectory(directoryPath));
                    animClipPaths[directoryPath].AddAnimClipPath(assetPath);
                }
            }
        }

        private static bool IsDirectory(string assetPath)
        {
            Debug.Log(System.IO.File.GetAttributes(assetPath));
            return System.IO.File.GetAttributes(assetPath) == System.IO.FileAttributes.Directory;
        }

        private static string GetFileDirectoryPath(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            var directoryPath = filePath.Replace(fileName, "");
            return directoryPath;
        }

    }
}