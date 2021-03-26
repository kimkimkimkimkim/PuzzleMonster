using System;
using System.Collections.Generic;
using Enum.UI;
using GameBase;
using UniRx;

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
                    .Subscribe();
            }),
        });

        return testActionDataList;

    }
}
