﻿using System.Collections.Generic;
using PM.Enum.UI;
using GameBase;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using PlayFab.Json;
using UniRx;
using UnityEngine;

public class MainSceneManager : SingletonMonoBehaviour<MainSceneManager>
{
    private TitleWindowUIScript titleUIScript;

    private void Start()
    {
        // タイトル画面の表示
        titleUIScript = UIManager.Instance.CreateContent<TitleWindowUIScript>(UIManager.Instance.windowParent);
        titleUIScript.ShowTapToStartButton(false);
        titleUIScript.SetOnClickAction(() => StartGame());

        // アプリケーション起動時の処理
        ApplicationContext.EstablishSession()
            .SelectMany(_ => ApiConnection.GetTitleData().Do(res => MasterRecord.SetCacheMasterDict(res.Data))) // マスタの取得と保存
            .SelectMany(_ =>
            {
                if (string.IsNullOrWhiteSpace(ApplicationContext.userData.playerProfile.DisplayName))
                {
                    // 名前が未設定なので名前登録ダイアログを開く
                    return UserNameRegistrationDialogFactory.Create(new UserNameRegistrationDialogRequest())
                        .SelectMany(resp =>
                        {
                            switch (resp.dialogResponseType)
                            {
                                case DialogResponseType.Yes:
                                    return ApiConnection.UpdateUserTitleDisplayName(resp.userName).Select(res => true);
                                case DialogResponseType.No:
                                default:
                                    return Observable.Return<bool>(false);
                            }
                        });
                }
                else
                {
                    // 通常ログイン
                    return Observable.Return<bool>(true);
                }
            })
            .Where(isOk => isOk)
            .Do(_ =>
            {
                // CloudScriptの実行
                CallCSharpExecuteFunction();

                // スタートボタンを表示
                titleUIScript.ShowTapToStartButton(true);

                // デバッグボタンの作成
                var debugItem = UIManager.Instance.CreateContent<DebugItem>(UIManager.Instance.debugParent);
                debugItem.SetOnClickAction(() =>
                {
                    TestActionDialogFactory.Create(new TestActionDialogRequest()).Subscribe();
                });
            })
            .Subscribe();
    }

    private void StartGame()
    {
        // タイトル画面を消してホーム画面へ遷移
        if (titleUIScript != null) Destroy(titleUIScript.gameObject);
        HeaderFooterWindowFactory.Create(new HeaderFooterWindowRequest()).Subscribe();
    }

    private void CallCSharpExecuteFunction()
    {
        PlayFabCloudScriptAPI.ExecuteFunction(new ExecuteFunctionRequest()
        {
            Entity = new PlayFab.CloudScriptModels.EntityKey()
            {
                Id = PlayFabSettings.staticPlayer.EntityId, //Get this from when you logged in,
                Type = PlayFabSettings.staticPlayer.EntityType //Get this from when you logged in
            },
            FunctionName = "DropItem", //This should be the name of your Azure Function that you created.
            FunctionParameter = new Dictionary<string, object>() { { "DropTableName", "NormalGachaSingle" } },
            GeneratePlayStreamEvent = true
        }, (ExecuteFunctionResult result) =>
        {
            var grantedItems = PlayFabSimpleJson.DeserializeObject<List<ItemInstance>>(result.FunctionResult.ToString());
            Debug.Log($"Get Item");
            grantedItems.ForEach(item =>
            {
                Debug.Log($"itemId : {item.ItemId} ,itemClass : {item.ItemClass}");
            });


        }, (PlayFabError error) =>
        {
            Debug.Log($"Opps Something went wrong: {error.GenerateErrorReport()}");
        });

    }
}
