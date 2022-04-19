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
            title = "モンスター付与",
            action = new Action(() =>
            {
                var bundleIdList = new List<long>() { 9001005, 9001006, 9001007, 9001008 };
                
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "モンスターを付与します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => {
                        var observableList = bundleIdList.Select(bundleId => {
                            var itemId = ItemUtil.GetItemId(ItemType.Bundle, bundleId);
                            return ApiConnection.GrantItemsToUser(itemId).AsUnitObservable();
                        });
                        return Observable.ReturnUnit().Connect(observableList);
                    })
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "モンスターの付与が完了しました",
                    }))
                    .Subscribe();
            })
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "資産付与",
            action = new Action(() =>
            {
                var itemIdList = MasterRecord.GetMasterOf<PropertyMB>().GetAll().Select(m => ItemUtil.GetItemId(ItemType.Property, m.id));

                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "資産を付与します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => {
                        var observableList = itemIdList.Select(itemId => {
                            var grantItemNum = 20;
                            var grantItemIdList = Enumerable.Repeat(itemId, grantItemNum).ToList();
                            return ApiConnection.GrantItemsToUser(grantItemIdList).AsUnitObservable();
                        });
                        return Observable.ReturnUnit().Connect(observableList);
                    })
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "資産の付与が完了しました",
                    }))
                    .Subscribe();
            })
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
            title = "作業用",
            action = new Action(() =>
            {
                var p = new ProbabilityItemMI()
                {
                    itemType = ItemType.Bundle,
                    itemId = 1,
                    num = 2,
                    percent = 1,
                    weight = 1,
                };
                var i = new ItemMI()
                {
                    itemType = p.itemType,
                    itemId = p.itemId,
                    num = p.num,
                };
                var g = (GachaRewardItemMI)i;
            }),
        });

        return testActionDataList;

    }

}
