#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using HybridCLR;
using System.IO;
using GameFramework;
using HybridCLR.Editor;
public class BuildAppListener : IPostprocessBuildWithReport, IPreprocessBuildWithReport, IPostBuildPlayerScriptDLLs
{
    public int callbackOrder => 100;

    public void OnPostBuildPlayerScriptDLLs(BuildReport report)
    {
        //Debug.LogFormat("OnPostBuildPlayerScriptDLLs:{0}", report.name);

    }

    public void OnPostprocessBuild(BuildReport report)
    {
        BuildTarget target = report.summary.platform;
        //CompileDllHelper.CompileDll(target);
        var hotfixDllDir = UtilityBuiltin.ResPath.GetCombinePath(Application.dataPath, ConstBuiltin.HOT_FIX_DLL_DIR);
        try
        {
            if (!Directory.Exists(hotfixDllDir))
            {
                Directory.CreateDirectory(hotfixDllDir);
            }
            else
            {
                var dllFils = Directory.GetFiles(hotfixDllDir, "*.dll.bytes");
                for (int i = dllFils.Length - 1; i >= 0; i--)
                {
                    File.Delete(dllFils[i]);
                }
            }
            MyGameTools.CopyHotfixDllTo(target, hotfixDllDir);
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("生成热更新dll文件失败:{0}", e.Message);
            throw;
        }

    }

    public void OnPreprocessBuild(BuildReport report)
    {
        HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
        Debug.Log("-----------HybridCLR生成桥接函数,AOT泛型补充等---------");
    }
    
}
#endif