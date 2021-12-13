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
            title = "オーブ追加",
            action = new Action(() =>
            {
                const long debugOrbBundleId = 9001001;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugOrbBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "オーブを追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
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
            title = "コイン追加",
            action = new Action(() =>
            {
                const long debugCoinBundleId = 9001002;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugCoinBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "コインを追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
                    .Do(res => HeaderFooterManager.Instance.UpdateVirutalCurrencyText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "コインの追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "プレイヤー経験値追加",
            action = new Action(() =>
            {
                const long debugPlayerExpBundleId = 9001003;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugPlayerExpBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "プレイヤー経験値を追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
                    .Do(res => HeaderFooterManager.Instance.UpdateVirutalCurrencyText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "プレイヤー経験値の追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "モンスター経験値追加",
            action = new Action(() =>
            {
                const long debugMonsterExpBundleId = 9001004;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugMonsterExpBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "モンスター経験値を追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
                    .Do(res => HeaderFooterManager.Instance.UpdateVirutalCurrencyText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "モンスター経験値の追加が完了しました",
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
