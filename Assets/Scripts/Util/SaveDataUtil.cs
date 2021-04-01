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

    public static void Clear()
    {
        SaveData.Clear();
        SaveData.Save();
    }
}
