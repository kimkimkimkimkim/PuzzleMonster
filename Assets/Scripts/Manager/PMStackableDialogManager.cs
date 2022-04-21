
using GameBase;
using System.Linq;

public class PMStackableDialogManager : StackableDialogManager
{

    /// <summary>
    /// 未処理の通知ダイアログをスタック
    /// </summary>
    public void Restack()
    {
        // ログインボーナス
        var userLoginBonusIdList = SaveDataUtil.StackableDialog.GetUserLoginBonusIdList();
        userLoginBonusIdList.ForEach(id =>
        {
            var userLoginBonus = ApplicationContext.userData.userLoginBonusList.FirstOrDefault(u => u.id == id);
            if (userLoginBonus == null) return;

            // 今日のでなければスキップ
            var lastDate = userLoginBonus.loginDateList.MaxOrDefault();
            var isSameGameDate = DateTimeUtil.IsSameGameDate(lastDate, DateTimeUtil.Now);
            if (!isSameGameDate) return;

            // スタックに積む
            StackLoginBonusDialog(userLoginBonus.loginBonusId, userLoginBonus.loginDateList.Count);
        });
    }

    public void StackLoginBonusDialog(long loginBonusId, int loginDateNum)
    {
        /*
         // まだスタックされていないときだけダイアログをスタックする
        if (TaskClearDialogFactory.IsStacked(shareUserId)) return;
        TaskClearDialogFactory.Push(new TaskClearDialogRequest() { shareUserId = shareUserId })
            .Do(res => {
                // 再生が終わったらPrefsから対象ユーザーに関する表示済みのidを削除
                PlayerPrefsUtil.StackableDialog.RemoveClearedTaskId(shareUserId, res.taskIdList);
            })
            .Do(res => {
                if (!UIManager.Instance.isSkipTutorial) TutorialManager.Instance.taskTriggered.PlayIfNeededByShareUserClearedTask(res.taskIdList);
            })
            .Subscribe(); 
        */
    }
}