using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
/// <summary>
/// �ȸ��߼����
/// </summary>
public class HotfixEntry
{
    public static void StartHotfixLogic(bool enableHotfix)
    {
        Log.Info("Hotfix Enable:{0}", enableHotfix);
        GFBuiltin.Fsm.DestroyFsm<IProcedureManager>();
        var fsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
        var procManager = GameFrameworkEntry.GetModule<IProcedureManager>();
        //�ֶ����ȸ��³��򼯵�������ӽ���
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
