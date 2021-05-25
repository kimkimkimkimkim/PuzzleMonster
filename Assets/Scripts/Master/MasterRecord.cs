using System;
using System.Collections.Generic;
using GameBase;
using Newtonsoft.Json;

static class MasterRecord
{
    /// <summary>
    /// マスターの型とその型のCacheRecordsのディクショナリー
    /// </summary>
    private static Dictionary<Type, object> cacheMasterDict = new Dictionary<Type, object>();

    /// <summary>
    /// タイトルデータのキャッシュ
    /// </summary>
    private static Dictionary<string, string> cacheTitleData = new Dictionary<string, string>();

    /// <summary>
    /// 取得したタイトルデータを成形してキャッシュとして保持
    /// </summary>
    /// <param name="titleData">マスター名とその値のJsonのディクショナリー</param>
    public static void SetCacheMasterDict(Dictionary<string,string> titleData)
    {
        cacheTitleData = titleData;

        LoadMasterData<GachaBoxMB>();
        LoadMasterData<MonsterMB>();
        LoadMasterData<GachaBoxDetailMB>();
        LoadMasterData<VirtualCurrencyMB>();
        LoadMasterData<QuestCategoryMB>();
        LoadMasterData<QuestMB>();
        LoadMasterData<PropertyMB>();
        LoadMasterData<MonsterLevelUpTableMB>();
        LoadMasterData<UserRankUpTableMB>();
        LoadMasterData<StaminaMB>();
    }

    /// <summary>
    /// 指定したマスタをキャッシュに追加
    /// </summary>
    private static CachedRecords<T> LoadMasterData<T>() where T : MasterBookBase
    {
        var type = typeof(T);

        // キー(マスタ名)を取得
        var key = TextUtil.GetDescriptionAttribute<T>();
        if (!cacheTitleData.ContainsKey(key)) return null;

        // 指定のマスタデータのjsonを取得
        var json = cacheTitleData[key];
        var data = JsonConvert.DeserializeObject<T[]>(json);

        // キャッシュに追加
        var container = new MasterContentContainer<T>()
        {
            Name = key,
            BookList = data,
        };
        var cacheRecords = new CachedRecords<T>(container);
        cacheMasterDict.Add(type, cacheRecords);
        return cacheRecords;
    }

    /// <summary>
    /// 指定したマスタのデータを返します
    /// </summary>
    public static CachedRecords<T> GetMasterOf<T>() where T : MasterBookBase
    {
        var type = typeof(T);

        if (!cacheMasterDict.ContainsKey(type))
        {
            return LoadMasterData<T>();
        }
        else
        {
            return (CachedRecords<T>)cacheMasterDict[type];
        }
    }
}
