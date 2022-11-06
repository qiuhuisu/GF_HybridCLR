using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
/// <summary>
/// 热更逻辑入口
/// </summary>
public class HotfixEntry
{
    public static void StartHotfixLogic(bool enableHotfix)
    {
        Log.Info("Hotfix Enable:{0}", enableHotfix);
        GFBuiltin.Fsm.DestroyFsm<IProcedureManager>();
        var fsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
        var procManager = GameFrameworkEntry.GetModule<IProcedureManager>();
        //手动把热更新程序集的流程添加进来
        ProcedureBase[] procedures = new ProcedureBase[]
        {
            new PreloadProcedure(),
            new ChangeSceneProcedure(),
            new MenuProcedure(),
            new GameProcedure(),
            new GameOverProcedure()
        };
        procManager.Initialize(fsmManager, procedures);
        procManager.StartProcedure<PreloadProcedure>();
    }
}
