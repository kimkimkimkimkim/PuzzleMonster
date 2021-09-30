using System;
using PlayFab.ClientModels;
using UniRx;
using System.Linq;

public static class ApplicationContext
{
    public static PlayerProfileModel playerProfile { get; set; } = new PlayerProfileModel();
    public static UserVirtualCurrencyInfo userVirtualCurrency { get; set; } = new UserVirtualCurrencyInfo();
    public static UserInventoryInfo userInventory { get; set; } = new UserInventoryInfo();
    public static UserDataInfo userData { get; private set; } = new UserDataInfo();

    /// <summary>
    /// アプリ起動時処理
    /// </summary>
    public static IObservable<Unit> EstablishSession()
    {
        return ApiConnection.LoginWithCustomID()
            .SelectMany(res =>
            {
                if (res.NewlyCreated)
                {
                    // アカウントを新規作成した場合は初回ログイン処理を実行
                    return ApiConnection.FirstLogin().AsUnitObservable();
                }
                else
                {
                    return Observable.ReturnUnit();
                }
            })
            .SelectMany(_ => ApiConnection.Login())
            .SelectMany(_ => ApiConnection.GetTitleData().Do(res =>
            {
                // マスタの取得と保存
                MasterRecord.SetCacheMasterDict(res.Data);
            }))
            .SelectMany(_ => ApiConnection.GetPlayerProfile().Do(res =>
            {
                // ユーザープロフィールの同期
                playerProfile = res.PlayerProfile;
            }))
            .SelectMany(_ => UpdateUserDataObservable()) // ユーザーデータの更新
            .AsUnitObservable();
    }

    /// <summary>
    /// ユーザーデータ情報キャッシュを更新する
    /// </summary>
    public static void UpdateUserData(UserDataInfo userData)
    {
        ApplicationContext.userData = userData;
    }

    /// <summary>
    /// ユーザーデータを更新
    /// ユーザーデータが更新されるタイミングでは毎回これを呼ぶ
    /// </summary>
    public static IObservable<Unit> UpdateUserDataObservable()
    {
        // 一旦初期化
        userData = new UserDataInfo();

        return ApiConnection.GetUserData()
            .Do(res =>
            {
                // ユーザーデータの更新
                UpdateUserData(res);
            })
            .SelectMany(_ => ApiConnection.GetUserInventory().Do(res =>
            {
                // 仮想通貨情報の更新
                UpdateVirutalCurrency(res);

                // インベントリ情報の更新
                UpdateUserInventory(res);
            }))
            .AsUnitObservable();
    }

    /// <summary>
    /// インベントリ情報によるユーザー仮想通貨情報の更新
    /// </summary>
    private static void UpdateVirutalCurrency(GetUserInventoryResult result)
    {
        var userVirtualCurrency = UserDataUtil.GetUserVirutalCurrency(result);
        ApplicationContext.userVirtualCurrency = userVirtualCurrency;
    }

    /// <summary>
    /// インベントリ情報の更新
    /// </summary>
    private static void UpdateUserInventory(GetUserInventoryResult result)
    {
        var userInventory = UserDataUtil.GetUserInventory(result);
        ApplicationContext.userInventory = userInventory;
    }
}
