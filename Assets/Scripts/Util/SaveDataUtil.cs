﻿using System;
using System.Collections;
using System.Collections.Generic;
using GameBase;
using UnityEngine;

public class SaveDataUtil
{
    public class System
    {
        #region Key
        /// <summary>
        /// ユーザー毎に一意に決まるID
        /// </summary>
        private static string CUSTOM_ID_KEY = "system/customId";
        #endregion

        #region DefaultValue
        private static string customIdDefaultValue = Guid.NewGuid().ToString();
        #endregion

        #region Get
        public static string GetCustomId()
        {
            var customId = SaveData.GetString(CUSTOM_ID_KEY,customIdDefaultValue);

            // カスタムIDに関しては取得時に保存も行う
            SetCustomId(customId);

            return customId;
        }
        #endregion

        #region Set
        private static void SetCustomId(string customId)
        {
            SaveData.SetString(CUSTOM_ID_KEY, customId);
            SaveData.Save();
        }
        #endregion
    }

    public class StackableDialog
    {
        #region Key
        /// <summary>
        /// ユーザーログインボーナスIDリスト
        /// </summary>
        private static string USER_LOGIN_BONUS_ID_LIST = "stackableDialog/userLoginBonusIdList";
        #endregion

        #region DefaultValue
        private static List<string> userLoginBonusIdListDefaultValue = new List<string>();
        #endregion

        #region Get
        public static List<string> GetUserLoginBonusIdList()
        {
            var userLoginBonusIdList = SaveData.GetList(USER_LOGIN_BONUS_ID_LIST, userLoginBonusIdListDefaultValue);
            return userLoginBonusIdList;
        }
        #endregion

        #region Set
        private static void SetUserLoginBonusIdList(List<string> userLoginBonusIdList)
        {
            SaveData.SetList(USER_LOGIN_BONUS_ID_LIST, userLoginBonusIdList);
            SaveData.Save();
        }
        #endregion

        #region Add
        public static void AddUserLoginBonusId(string userLoginBonusId)
        {
            var userLoginBonusIdList = SaveData.GetList(USER_LOGIN_BONUS_ID_LIST, userLoginBonusIdListDefaultValue);

            // すでに存在していなければ追加
            if (!userLoginBonusIdList.Contains(userLoginBonusId))
            {
                userLoginBonusIdList.Add(userLoginBonusId);
                SetUserLoginBonusIdList(userLoginBonusIdList);
            }
        }
        #endregion

        #region Remove
        public static void RemoveUserLoginBonusId(string userLoginBonusId)
        {
            var userLoginBonusIdList = SaveData.GetList(USER_LOGIN_BONUS_ID_LIST, userLoginBonusIdListDefaultValue);

            // 削除したときのみ更新
            var removeNum = userLoginBonusIdList.RemoveAll(id => id == userLoginBonusId);
            if(removeNum > 0) SetUserLoginBonusIdList(userLoginBonusIdList);
        }
        #endregion
    }

    public class Battle 
    {
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
        #endregion

        #region DefaultValue
        private static long resumeQuestIdDefaultValue = 0;
        private static string resumeUserMonsterPartyIdDefaultValue = string.Empty;
        private static string resumeUserBattleIdDefaultValue = string.Empty;
        private static List<BattleLogInfo> resumeBattleLogListDefaultValue = new List<BattleLogInfo>();
        #endregion

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
        #endregion

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
        #endregion

        #region Clear
        public static void ClearAllResumeSaveData() {
            SetResumeQuestId(resumeQuestIdDefaultValue);
            SetResumeUserMonsterPartyId(resumeUserMonsterPartyIdDefaultValue);
            SetResumeUserBattleId(resumeUserBattleIdDefaultValue);
            SetResumeBattleLogList(resumeBattleLogListDefaultValue);
        }
        #endregion
    }

    public static void Clear()
    {
        SaveData.Clear();
        SaveData.Save();
    }
}
