﻿using System;

public class ConstManager
{
    public class System
    {
        /// <summary>
        /// ユーザー名の最小文字数
        /// </summary>
        public static int userNameMinWordCount = 3;

        /// <summary>
        /// ユーザー名の最大文字数
        /// </summary>
        public static int userNameMaxWordCount = 10;
    }

    public class User
    {
        /// <summary>
        /// 1スタミナ溜まるのに必要な時間[ms]
        /// </summary>
        public static int millSecondsPerStamina = 180000;
    }

    public class Monster
    {
        /// <summary>
        /// 全モンスターの中での最大ステータス値
        /// ゲージの最大値として使用する
        /// </summary>
        public static int MAX_STATUS_VALUE = 5000;

        /// <summary>
        /// 最大レベル
        /// </summary>
        public static int MAX_LEVEL = 100;
    }

    public class Battle
    {
        /// <summary>
        /// 最大パーティーメンバー数
        /// </summary>
        public static int MAX_PARTY_MEMBER_NUM = 6;

        /// <summary>
        /// 最大パーティ数
        /// </summary>
        public static int MAX_PARTY_NUM = 12;

        /// <summary>
        /// 盤面の横のピース数
        /// </summary>
        public static int BOARD_WIDTH = 8;

        /// <summary>
        /// 盤面の縦のピース数
        /// </summary>
        public static int BOARD_HEIGHT = 8;
    }
}
