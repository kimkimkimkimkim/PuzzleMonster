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
}
