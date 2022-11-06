
using System.Collections.Generic;

/// <summary>
/// �ȸ�Const
/// </summary>
public static partial class Const
{
    internal const long DefaultVibrateDuration = 50;//��׿�ֻ���ǿ��
    internal static readonly float SHOW_CLOSE_INTERVAL = 1f;//���ֹرհ�ť���ӳ�

    public static readonly string HORIZONTAL = "Horizontal";
    public static readonly bool RepeatLevel = true;//�Ƿ�ѭ���ؿ�

    internal static class Tags
    {
        public static readonly string Player = "Player";
        public static readonly string AIPlayer = "AIPlayer";
    }
    internal static class UserData
    {
        internal static readonly string MONEY = "UserData.MONEY";
        internal static readonly string GUIDE_ON = "UserData.GUIDE_ON";

        internal static readonly string SHOW_RATING_COUNT = "UserData.SHOW_RATING_COUNT";
        internal static readonly string GAME_LEVEL = "UserData.GAME_LEVEL";
        internal static readonly string CAR_SKIN_ID = "UserData.CAR_SKIN_ID";

        internal static readonly string USER_SPAWN_POINT_TYPE = "UserData.USER_SPAWN_POINT_TYPE";
    }

    public static class UIParmKey
    {
        /// <summary>
        /// �㷵�عرս���
        /// </summary>
        public static readonly string EscapeClose = "EscapeClose";
        /// <summary>
        /// UI�򿪹رն���
        /// </summary>
        public static readonly string OpenAnimType = "OpenAnimType";
        public static readonly string CloseAnimType = "CloseAnimType";
        /// <summary>
        /// UI�㼶
        /// </summary>
        public static readonly string SortOrder = "SortOrder";
        /// <summary>
        /// ��ť�ص�
        /// </summary>
        public static readonly string OnButtonClick = "OnButtonClick";
        public static readonly string OnShow = "OnShow";
        public static readonly string OnHide = "OnHide";
    }
}
