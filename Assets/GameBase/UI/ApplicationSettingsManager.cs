namespace GameBase {
    public class ApplicationSettingsManager : SingletonMonoBehaviour<ApplicationSettingsManager> {
        /// <summary>
        /// デバッグモードか否か
        /// </summary>
        public bool isDebugMode;

        /// <summary>
        /// テスト広告モードか否か
        /// </summary>
        public bool isTestAdMode;

        /// <summary>
        /// デバッグバトルログモードか否か
        /// デバッグモードならログテキストを取得
        /// </summary>
        public bool isDebugBattleLogMode;
    }
}
