using System;
using System.Collections.Generic;
using Enum.Item;
using Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;

public class UserTestAction : ITestAction
{
    public List<TestActionData> GetTestActionDataList()
    {
        var testActionDataList = new List<TestActionData>();

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
                    .Do(res =>
                    {
                        Debug.Log($"仮想通貨の追加に成功");

                        //仮想通貨の情報をログで表示
                        Debug.Log($"変更した仮想通貨のコード : {res.VirtualCurrency}");
                        Debug.Log($"変更後の残高 : {res.Balance}");
                        Debug.Log($"加算額 : {res.BalanceChange}");
                    })
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "オーブの追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        return testActionDataList;

    }
}
