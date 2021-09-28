using System;
using PM.Enum.Item;
using PM.Enum.Gacha;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UniRx;
using PM.Enum.Data;
using System.Linq;
using Newtonsoft.Json;

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
        })
            .SelectMany(res => ApplicationContext.UpdateUserDataObservable().Select(_ => res)); ;
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
        })
            .SelectMany(res => ApplicationContext.UpdateUserDataObservable().Select(_ => res)); ;
    }

    /// <summary>
    /// タイトルデータを取得する
    /// </summary>
    public static IObservable<GetTitleDataResult> GetTitleData()
    {
        return SendRequest<GetTitleDataRequest, GetTitleDataResult>(ApiType.GetTitleData, new GetTitleDataRequest());
    }

    /// <summary>
    /// ユーザーデータを全取得する
    /// </summary>
    public static IObservable<UserDataInfo> GetUserData()
    {
        return SendRequest<GetUserDataRequest, GetUserDataResult>(ApiType.GetUserData, new GetUserDataRequest())
            .Select(res => UserDataUtil.GetUserData(res.Data));
    }

    /// <summary>
    /// ユーザーモンスターリストを取得する
    /// </summary>
    public static IObservable<List<UserMonsterInfo>> GetUserMonsterList()
    {
        return SendRequest<GetUserDataRequest, GetUserDataResult>(ApiType.GetUserData, new GetUserDataRequest()
        {
            Keys = new List<string>() { UserDataKey.userMonsterList.ToString() },
        })
            .Select(res =>
            {
                var userMonsterList = new List<UserMonsterInfo>();
                foreach(var kvp in res.Data)
                {
                    if(kvp.Key == UserDataKey.userMonsterList.ToString())
                    {
                        userMonsterList = JsonConvert.DeserializeObject<List<UserMonsterInfo>>(kvp.Value.Value);
                    }
                }
                return userMonsterList;
            });
    }

    /// <summary>
    /// ユーザーデータを更新する
    /// 開発用
    /// </summary>
    public static IObservable<UpdateUserDataResult> UpdateUserData(Dictionary<UserDataKey,object> dict)
    {
        var data = dict.ToDictionary(kvp => kvp.Key.ToString(),kvp => JsonConvert.SerializeObject(kvp.Value));
        return SendRequest<UpdateUserDataRequest, UpdateUserDataResult>(ApiType.UpdateUserData, new UpdateUserDataRequest()
        {
            Data = data,
        });
    }

    /// <summary>
    /// ユーザーにキャラクターデータを追加する
    /// </summary>
    public static IObservable<GrantCharacterToUserResult> GrantCharacterToUser(ItemInstance item)
    {
        return SendRequest<GrantCharacterToUserRequest, GrantCharacterToUserResult>(ApiType.GrantCharacterToUser, new GrantCharacterToUserRequest()
        {
            CharacterName = item.DisplayName,
            ItemId = item.ItemId,
        });
    }
    #endregion

    #region CloudFunction
    /// <summary>
    /// ログイン時に行いたいことを実行する
    /// </summary>
    public static IObservable<LoginApiResponse> Login()
    {
        return SendRequest<LoginApiRequest, LoginApiResponse>(LoginApiInterface.functionName, new LoginApiRequest()
        {

        });
    }

    /// <summary>
    /// ガチャを実行する
    /// </summary>
    public static IObservable<DropItemApiResponse> DropItem(string dropTableId)
    {
        return SendRequest<DropItemApiRequest, DropItemApiResponse>(DropItemApiInterface.functionName, new DropItemApiRequest()
        {
            dropTableName = dropTableId,
        });
    }

    /// <summary>
    /// インベントリのアイテムをユーザーに付与する
    /// </summary>
    public static IObservable<GrantItemsToUserApiResponse> GrantItemsToUser(List<string> itemIdList)
    {
        return SendRequest<GrantItemsToUserApiRequest, GrantItemsToUserApiResponse>(GrantItemsToUserApiInterface.functionName, new GrantItemsToUserApiRequest()
        {
            itemIdList = itemIdList,
        });
    }

    /// <summary>
    /// モンスター強化
    /// </summary>
    public static IObservable<MonsterLevelUpApiResponse> MonsterLevelUp(string userMonsterId, int exp)
    {
        return SendRequest<MonsterLevelUpApiRequest, MonsterLevelUpApiResponse>(MonsterLevelUpApiInterface.functionName, new MonsterLevelUpApiRequest()
        {
            userMonsterId = userMonsterId,
            exp = exp,
        });
    }

    /// <summary>
    /// パーティの編成情報更新
    /// </summary>
    public static IObservable<UpdateUserMonsterFormationApiResponse> UpdateUserMosnterFormation(int partyIndex, List<string> userMosnterIdList) {
        return SendRequest<UpdateUserMonsterFormationApiRequest, UpdateUserMonsterFormationApiResponse>(UpdateUserMonsterFormationApiInterface.functionName, new UpdateUserMonsterFormationApiRequest()
        {
            partyIndex = partyIndex,
            userMonsterIdList = userMosnterIdList,
        });
    }

    /// <summary>
    /// 開発用:インベントリのカスタムデータ更新
    /// </summary>
    public static IObservable<DevelopUpdateUserInventoryCustomDataApiResponse> DevelopUpdateUserInventoryCustomData(string itemInstanceId, Dictionary<string,string> data)
    {
        return SendRequest<DevelopUpdateUserInventoryCustomDataApiRequest, DevelopUpdateUserInventoryCustomDataApiResponse>(DevelopUpdateUserInventoryCustomDataApiInterface.functionName, new DevelopUpdateUserInventoryCustomDataApiRequest()
        {
            itemInstanceId = itemInstanceId,
            data = data,
        });
    }

    /// <summary>
    /// スタミナを消費する
    /// </summary>
    public static IObservable<DevelopConsumeStaminaApiResponse> DevelopConsumeStamina(int consumeStamina)
    {
        return SendRequest<DevelopConsumeStaminaApiRequest, DevelopConsumeStaminaApiResponse>(DevelopConsumeStaminaApiInterface.functionName, new DevelopConsumeStaminaApiRequest()
        {
            consumeStamina = consumeStamina,
        });
    }
    #endregion
}
