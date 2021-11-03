using System;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class UserTestAction : ITestAction
{
    public List<TestActionData> GetTestActionDataList()
    {
        var testActionDataList = new List<TestActionData>();

        testActionDataList.Add(new TestActionData()
        {
            title = "セーブデータ削除",
            action = new Action(() =>
            {
                SaveDataUtil.Clear();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "オーブ追加(1000個)",
            action = new Action(() =>
            {
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "オーブを追加します(1000個)"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.AddUserVirtualCurrency(VirtualCurrencyType.OB,1000))
                    .Do(res => HeaderFooterManager.Instance.UpdateVirutalCurrencyText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "オーブの追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "モンスター経験値付与(100exp)",
            action = new Action(() =>
            {
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "モンスター経験値を付与します(100exp)"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => {
                        // 付与するアイテムの作成
                        var propertyId = (long)PropertyType.MonsterExp;
                        var itemId = ItemUtil.GetItemId(ItemType.Property,propertyId);
                        var itemIdList = Enumerable.Repeat(itemId, 100).ToList();
                        return ApiConnection.GrantItemsToUser(itemIdList);
                    })
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "モンスター経験値の付与が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "開発用:インベントリカスタムデータ更新",
            action = new Action(() =>
            {
                var itemInstanceId = "24C48BD78B361A0C";
                var data = new Dictionary<string, string>()
                {
                    {"sampleDataKey","sampleDataValue" },
                    {"level","1" },
                    {"grade","3" },
                };

                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "インベントリカスタムデータを更新します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => {
                        return ApiConnection.DevelopUpdateUserInventoryCustomData(itemInstanceId,data);
                    })
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "データの更新が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "スタミナ消費(5)",
            action = new Action(() =>
            {
                var consumeStamina = 5;

                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = $"スタミナを{consumeStamina}消費します",
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.DevelopConsumeStamina(consumeStamina))
                    .Do(_ => HeaderFooterManager.Instance.SetStaminaText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "スタミナを消費しました",
                    }))
                    .Subscribe();
            })
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "ObservableText",
            action = new Action(() =>
            {
                var observableList = new List<string>() { "a", "b", "c", "d", "e" }.Select(s =>
                {
                    return Observable.Timer(TimeSpan.FromSeconds(1)).Do(_ => Debug.Log(s)).AsUnitObservable();
                });

                Observable.WhenAll(Observable.ReturnUnit().Concat(observableList.ToArray()))
                    .Do(_ => Debug.Log("=================== end =================="))
                    .Subscribe();
            })
        });

        return testActionDataList;

    }

}
