using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Battle;
using PM.Enum.Monster;
using PM.Enum.SortOrder;
using UnityEngine;

public class SaveDataUtil {
    public class System {
        #region Key

        /// <summary>
        /// ユーザー毎に一意に決まるID
        /// </summary>
        private static string CUSTOM_ID_KEY = "system/customId";

        #endregion Key

        #region DefaultValue

        private static string customIdDefaultValue = Guid.NewGuid().ToString();

        #endregion DefaultValue

        #region Get

        public static string GetCustomId() {
            var customId = SaveData.GetString(CUSTOM_ID_KEY, customIdDefaultValue);

            // カスタムIDに関しては取得時に保存も行う
            SetCustomId(customId);

            return customId;
        }

        #endregion Get

        #region Set

        private static void SetCustomId(string customId) {
            SaveData.SetString(CUSTOM_ID_KEY, customId);
            SaveData.Save();
        }

        #endregion Set
    }

    public class Setting {
        #region Key

        /// <summary>
        /// ガチャアニメーションを再生するか否か
        /// </summary>
        private static string IS_PLAY_GACHA_ANIMATION = "setting/isPlayGachaAnimation";

        #endregion Key

        #region DefaultValue

        private static bool isPlayGachaAnimationDefaultValue = true;

        #endregion DefaultValue

        #region Get

        public static bool GetIsPlayGachaAnimation() {
            var isPlayGachaAnimation = SaveData.GetBool(IS_PLAY_GACHA_ANIMATION, isPlayGachaAnimationDefaultValue);
            return isPlayGachaAnimation;
        }

        #endregion Get

        #region Set

        public static void SetIsPlayGachaAnimation(bool isPlayGachaAnimation) {
            SaveData.SetBool(IS_PLAY_GACHA_ANIMATION, isPlayGachaAnimation);
            SaveData.Save();
        }

        #endregion Set
    }

    public class StackableDialog {
        #region Key

        /// <summary>
        /// ユーザーログインボーナスIDリスト
        /// </summary>
        private static string USER_LOGIN_BONUS_ID_LIST = "stackableDialog/userLoginBonusIdList";

        /// <summary>
        /// ユーザーお知らせIDリスト
        /// </summary>
        private static string USER_NEWS_ID_LIST = "stackableDialog/userNewsIdList";

        /// <summary>
        /// お知らせ表示日時リスト
        /// </summary>
        private static string NEWS_OPEN_DATE_LIST(string userNewsId) => $"stackableDialog/newsOpenDateList_{userNewsId}";
        #endregion Key

        #region DefaultValue

        private static List<string> userLoginBonusIdListDefaultValue = new List<string>();

        private static List<string> userNewsIdListDefaultValue = new List<string>();

        private static List<DateTime> newsOpenDateListDefaultValue = new List<DateTime>();
        #endregion DefaultValue

        #region Get

        public static List<string> GetUserLoginBonusIdList() {
            var userLoginBonusIdList = SaveData.GetList(USER_LOGIN_BONUS_ID_LIST, userLoginBonusIdListDefaultValue);
            return userLoginBonusIdList;
        }

        public static List<string> GetUserNewsIdList() {
            var userNewsIdList = SaveData.GetList(USER_NEWS_ID_LIST, userNewsIdListDefaultValue);
            return userNewsIdList;
        }

        public static List<DateTime> GetNewsOpenDateList(string userNewsId) {
            var newsOpenDateList = SaveData.GetList(NEWS_OPEN_DATE_LIST(userNewsId), newsOpenDateListDefaultValue);
            return newsOpenDateList;
        }
        #endregion Get

        #region Set

        private static void SetUserLoginBonusIdList(List<string> userLoginBonusIdList) {
            SaveData.SetList(USER_LOGIN_BONUS_ID_LIST, userLoginBonusIdList);
            SaveData.Save();
        }

        private static void SetUserNewsIdList(List<string> userNewsIdList) {
            SaveData.SetList(USER_NEWS_ID_LIST, userNewsIdList);
            SaveData.Save();
        }

        private static void SetNewsOpenDateList(string userNewsId, List<DateTime> newsOpenDateList) {
            SaveData.SetList(NEWS_OPEN_DATE_LIST(userNewsId), newsOpenDateList);
            SaveData.Save();
        }
        #endregion Set

        #region Add

        public static void AddUserLoginBonusId(string userLoginBonusId) {
            var userLoginBonusIdList = GetUserLoginBonusIdList();

            // すでに存在していなければ追加
            if (!userLoginBonusIdList.Contains(userLoginBonusId)) {
                userLoginBonusIdList.Add(userLoginBonusId);
                SetUserLoginBonusIdList(userLoginBonusIdList);
            }
        }

        public static void AddUserNewsId(string userNewsId) {
            var userNewsIdList = GetUserNewsIdList();

            // すでに存在していなければ追加
            if (!userNewsIdList.Contains(userNewsId)) {
                userNewsIdList.Add(userNewsId);
                SetUserNewsIdList(userNewsIdList);
            }
        }

        public static void AddNewsOpenDate(string userNewsId, DateTime date) {
            var newsOpenDateList = GetNewsOpenDateList(userNewsId);

            newsOpenDateList.Add(date);
            SetNewsOpenDateList(userNewsId, newsOpenDateList);
        }
        #endregion Add

        #region Remove

        public static void RemoveUserLoginBonusId(string userLoginBonusId) {
            var userLoginBonusIdList = SaveData.GetList(USER_LOGIN_BONUS_ID_LIST, userLoginBonusIdListDefaultValue);

            // 削除したときのみ更新
            var removeNum = userLoginBonusIdList.RemoveAll(id => id == userLoginBonusId);
            if (removeNum > 0) SetUserLoginBonusIdList(userLoginBonusIdList);
        }

        public static void RemoveUserNewsId(string userNewsId) {
            var userNewsIdList = SaveData.GetList(USER_NEWS_ID_LIST, userNewsIdListDefaultValue);

            // 削除したときのみ更新
            var removeNum = userNewsIdList.RemoveAll(id => id == userNewsId);
            if (removeNum > 0) SetUserNewsIdList(userNewsIdList);
        }
        #endregion Remove
    }

    public class Battle {
        #region Key

        /// <summary>
        /// 再開用クエストID
        /// </summary>
        private static string RESUME_QUEST_ID = "battle/resumeQuestId";

        /// <summary>
        /// 再開用ユーザーモンスターパーティーID
        /// </summary>
        private static string RESUME_USER_MONSTER_PARTY_ID = "battle/resumeUserMonsterPartyId";

        /// <summary>
        /// 再開用ユーザーバトルID
        /// </summary>
        private static string RESUME_USER_BATTLE_ID = "battle/resumeUserBatleId";

        /// <summary>
        /// 再開用バトルログリスト
        /// </summary>
        private static string RESUME_BATTLE_LOG_LIST = "battle/resumeBattleLogList";

        /// <summary>
        /// バトルスピード
        /// </summary>
        private static string BATTLE_SPEED = "battle/battleSpeed";

        #endregion Key

        #region DefaultValue

        private static long resumeQuestIdDefaultValue = 0;
        private static string resumeUserMonsterPartyIdDefaultValue = string.Empty;
        private static string resumeUserBattleIdDefaultValue = string.Empty;
        private static List<BattleLogInfo> resumeBattleLogListDefaultValue = new List<BattleLogInfo>();
        private static BattleSpeed battleSpeedDefaultValue = BattleSpeed.One;

        #endregion DefaultValue

        #region Get

        public static long GetResumeQuestId() {
            var resumeQuestId = SaveData.GetLong(RESUME_QUEST_ID, resumeQuestIdDefaultValue);
            return resumeQuestId;
        }

        public static string GetResumeUserMonsterPartyId() {
            var resumeUserMonsterPartyId = SaveData.GetString(RESUME_USER_MONSTER_PARTY_ID, resumeUserMonsterPartyIdDefaultValue);
            return resumeUserMonsterPartyId;
        }

        public static string GetResumeUserBattleId() {
            var resumeUserBattleId = SaveData.GetString(RESUME_USER_BATTLE_ID, resumeUserBattleIdDefaultValue);
            return resumeUserBattleId;
        }

        public static List<BattleLogInfo> GetResumeBattleLogList() {
            var resumeBattleLogList = SaveData.GetClassList(RESUME_BATTLE_LOG_LIST, resumeBattleLogListDefaultValue);
            return resumeBattleLogList;
        }

        public static BattleSpeed GetBattleSpeed() {
            var battleSpeed = SaveData.GetInt(BATTLE_SPEED, (int)battleSpeedDefaultValue);
            return (BattleSpeed)battleSpeed;
        }

        #endregion Get

        #region Set

        public static void SetResumeQuestId(long resumeQuestId) {
            SaveData.SetLong(RESUME_QUEST_ID, resumeQuestId);
            SaveData.Save();
        }

        public static void SetResumeUserMonsterPartyId(string resumeUserMonsterPartyId) {
            SaveData.SetString(RESUME_USER_MONSTER_PARTY_ID, resumeUserMonsterPartyId);
            SaveData.Save();
        }

        public static void SetResumeUserBattleId(string resumeUserBattleId) {
            SaveData.SetString(RESUME_USER_BATTLE_ID, resumeUserBattleId);
            SaveData.Save();
        }

        public static void SetResumeBattleLogList(List<BattleLogInfo> resumeBattleLogList) {
            SaveData.SetClassList(RESUME_BATTLE_LOG_LIST, resumeBattleLogList);
            SaveData.Save();
        }

        public static void SetBattleSpeed(BattleSpeed battleSpeed) {
            SaveData.SetInt(BATTLE_SPEED, (int)battleSpeed);
            SaveData.Save();
        }

        #endregion Set

        #region Clear

        public static void ClearAllResumeSaveData() {
            SetResumeQuestId(resumeQuestIdDefaultValue);
            SetResumeUserMonsterPartyId(resumeUserMonsterPartyIdDefaultValue);
            SetResumeUserBattleId(resumeUserBattleIdDefaultValue);
            SetResumeBattleLogList(resumeBattleLogListDefaultValue);
        }

        #endregion Clear
    }

    public class SortOrder {
        #region Key

        /// <summary>
        /// モンスターBOX画面における属性絞り込み値
        /// </summary>
        private static string FILTER_ATTRIBUTE_MONSTER_BOX = "sortOrder/filterAttributeMonsterBox";

        /// <summary>
        /// モンスターBOX画面における並び順タイプ
        /// </summary>
        private static string SORT_ORDER_TYPE_MONSTER_BOX = "sortOrder/sortOrderTypeMonsterBox";

        #endregion Key

        #region DefaultValue

        private static List<int> filterAttributeMonsterBoxDefaultValue = new List<int>();
        private static int sortOrderMonsterBoxDefaultValue = 1;

        #endregion DefaultValue

        #region Get

        public static List<MonsterAttribute> GetFilterAttributeMonsterBox() {
            return SaveData.GetList(FILTER_ATTRIBUTE_MONSTER_BOX, filterAttributeMonsterBoxDefaultValue).Select(i => (MonsterAttribute)i).ToList();
        }

        public static SortOrderTypeMonster GetSortOrderTypeMonsterBox() {
            return (SortOrderTypeMonster)SaveData.GetInt(SORT_ORDER_TYPE_MONSTER_BOX, sortOrderMonsterBoxDefaultValue);
        }

        #endregion Get

        #region Set

        public static void SetFilterAttriuteMonsterBox(List<MonsterAttribute> list) {
            SaveData.SetList(FILTER_ATTRIBUTE_MONSTER_BOX, list.Select(a => (int)a).ToList());
            SaveData.Save();
        }

        public static void SetSortOrderTypeMonster(SortOrderTypeMonster sortOrderType) {
            SaveData.SetInt(SORT_ORDER_TYPE_MONSTER_BOX, (int)sortOrderType);
            SaveData.Save();
        }

        #endregion Set
    }

    public static void Clear() {
        SaveData.Clear();
        SaveData.Save();
    }
}
