using System;

namespace Enum
{
    namespace Battle
    {
        /// <summary>
        /// ドロップタイプ
        /// </summary>
        public enum DropType
        {
            /// <summary>
            /// 通常
            /// </summary>
            Normal = 0,

            /// <summary>
            /// お邪魔
            /// </summary>
            Disturb = 1,
        }

        /// <summary>
        /// コマンドの方向
        /// </summary>
        public enum Direction
        {
            Up,
            Down,
            UpperRight,
            LowerRight,
            UpperLeft,
            LowerLeft,
        }

        /// <summary>
        /// 勝敗判定
        /// </summary>
        public enum WinOrLose
        {
            None,
            Win,
            Lose,
        }
    }

    namespace UI
    {
        /// <summary>
        /// ダイアログのレスポンス種類
        /// </summary>
        public enum DialogResponseType
        {
            None,
            Yes,
            No,
        }

        /// <summary>
        /// コモンダイアログタイプ
        /// </summary>
        public enum CommonDialogType
        {
            NoAndYes,
            YesOnly,
        }

        /// <summary>
        /// フッタータイプ
        /// </summary>
        [Serializable]
        public enum FooterType
        {
            Home = 0,
            Monster = 1,
            Gacha = 2,
            Shop = 3,
        }
    }
}