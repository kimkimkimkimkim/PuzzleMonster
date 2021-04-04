﻿using System;

namespace PM
{
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

            /// <summary>
            /// アイコン画像タイプ
            /// </summary>
            public enum IconImageType
            {
                None,
                Monster,
            }
        }

        namespace Item
        {
            /// <summary>
            /// アイテムタイプ
            /// </summary>
            public enum ItemType
            {
                /// <summary>
                /// 初期値
                /// </summary>
                None = 0,

                /// <summary>
                /// 仮想通貨
                /// </summary>
                VirtualCurrency = 1,

                /// <summary>
                /// モンスター
                /// </summary>
                Monster,
            }

            /// <summary>
            /// 仮想通貨タイプ
            /// </summary>
            public enum VirtualCurrencyType
            {
                /// <summary>
                /// オーブ
                /// </summary>
                OB = 1,

                /// <summary>
                /// コイン
                /// </summary>
                CN = 2,
            }
        }

        namespace Gacha
        {
            /// <summary>
            /// ガチャのドロップテーブルタイプ
            /// </summary>
            public enum DropTableType
            {
                /// <summary>
                /// レア召喚（シングル）
                /// </summary>
                NormalGachaSingle = 1,
            }

            /// <summary>
            /// どのようなモンスターが排出するガチャなのかを示す値
            /// どのタイプのガチャから排出するモンスターなのかを示す値
            /// </summary>
            public enum GachaBoxType
            {
                Normal = 1,
            }
        }

        namespace Data
        {
            /// <summary>
            /// ユーザーデータをPlayFabに保存する時のKey名
            /// UserDataInfoのパラメータ名と同じにする必要がある
            /// </summary>
            public enum UserDataKey
            {
                userMonsterList,
            }
        }
    }
}