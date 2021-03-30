using System;
using PM.Enum.Item;
using PM.Enum.Gacha;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public partial class ApiConnection
{
    #region ClientApi
    /// <summary>
    /// カスタムIDでのログイン処理
    /// </summary>
    public static IObservable<LoginResult> LoginWithCustomID()
    {
        return SendRequest<LoginWithCustomIDRequest, LoginResult>(ApiType.LoginWithCustomID, new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = SaveDataUtil.System.GetCustomId(),
            CreateAccount = true,
        });
    }

    /// <summary>
    /// プレイヤープロフィールを取得
    /// </summary>
    public static IObservable<GetPlayerProfileResult> GetPlayerProfile()
    {
        return SendRequest<GetPlayerProfileRequest, GetPlayerProfileResult > (ApiType.GetPlayerProfile, new GetPlayerProfileRequest()
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
        });
    }

    /// <summary>
    /// ユーザー名の更新
    /// </summary>
    public static IObservable<UpdateUserTitleDisplayNameResult> UpdateUserTitleDisplayName(string userName)
    {
        return SendRequest<UpdateUserTitleDisplayNameRequest, UpdateUserTitleDisplayNameResult>(ApiType.UpdateUserTitleDisplayName, new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = userName
        });
    }

    /// <summary>
    /// インベントリの取得
    /// </summary>
    public static IObservable<GetUserInventoryResult> GetUserInventory()
    {
        return SendRequest<GetUserInventoryRequest, GetUserInventoryResult>(ApiType.GetUserInventory, new GetUserInventoryRequest());
    }

    /// <summary>
    /// 仮想通貨の追加
    /// </summary>
    /// <returns>The user virtual currency.</returns>
    /// <param name="type">仮想通貨タイプ</param>
    /// <param name="num">追加する量</param>
    public static IObservable<ModifyUserVirtualCurrencyResult> AddUserVirtualCurrency(VirtualCurrencyType type,int num)
    {
        return SendRequest<AddUserVirtualCurrencyRequest, ModifyUserVirtualCurrencyResult>(ApiType.AddUserVirtualCurrency, new AddUserVirtualCurrencyRequest()
        {
            Amount = num,
            VirtualCurrency = type.ToString(),
        });
    }

    /// <summary>
    /// タイトルデータを取得する
    /// </summary>
    public static IObservable<GetTitleDataResult> GetTitleData()
    {
        return SendRequest<GetTitleDataRequest, GetTitleDataResult>(ApiType.GetTitleData, new GetTitleDataRequest());
    }
    #endregion

    #region CloudFunction
    /// <summary>
    /// ガチャを実行する
    /// </summary>
    public static IObservable<List<ItemInstance>> DropItem(DropTableType dropTableType)
    {
        return SendRequest<DropItemApiRequest, List<ItemInstance>>(DropItemApiInterface.functionName, new DropItemApiRequest()
        {
            dropTableName = dropTableType.ToString(),
        });
    }
    #endregion
}
