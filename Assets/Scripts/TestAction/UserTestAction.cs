using System;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                    .Do(res => HeaderManager.Instance.UpdateVirutalCurrencyText())
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
