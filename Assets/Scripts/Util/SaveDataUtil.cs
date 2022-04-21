using System;
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

    public static void Clear()
    {
        SaveData.Clear();
        SaveData.Save();
    }
}
