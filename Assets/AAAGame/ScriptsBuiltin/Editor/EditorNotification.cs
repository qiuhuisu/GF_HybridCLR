//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;

//[InitializeOnLoad]
//public class EditorNotification : AssetPostprocessor
//{
//    private static bool isFocused;
//    static EditorNotification()
//    {
//        EditorApplication.update += Update;
//    }

//    private static void Update()
//    {
//        if (isFocused == UnityEditorInternal.InternalEditorUtility.isApplicationActive)
//        {
//            return;
//        }
//        isFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
//        OnEditorFocus(isFocused);
//    }
//    /// <summary>
//    /// Unity���ھ۽�״̬�ı�ص�
//    /// </summary>
//    /// <param name="focus"></param>
//    private static void OnEditorFocus(bool focus)
//    {
//        if (focus)
//        {
//            //Debug.LogFormat("�༭������״̬:{0}", focus);
//            MyGameTools.RefreshAllConfig();
//            MyGameTools.RefreshAllDataTable();
//        }
//    }

//    private void OnPreprocessAsset()
//    {
//        //Debug.Log("Asset���ļ��ı�ʱ�ص�");
//    }
//}
//#endif