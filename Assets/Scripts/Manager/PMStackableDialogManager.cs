
using GameBase;
using System.Linq;
using UniRx;

public class PMStackableDialogManager : SingletonMonoBehaviour<PMStackableDialogManager>
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
            if (userLoginBonus == null)
            {
                SaveDataUtil.StackableDialog.RemoveUserLoginBonusId(id);
                return;
            }

            // 今日のでなければスキップ
            var lastDate = userLoginBonus.loginDateList.MaxOrDefault();
            var isSameGameDate = DateTimeUtil.IsSameGameDate(lastDate, DateTimeUtil.Now);
            if (!isSameGameDate)
            {
                SaveDataUtil.StackableDialog.RemoveUserLoginBonusId(id);
                return;
            }

            // スタックに積む
            StackLoginBonusDialog(userLoginBonus);
        });
    }

    public void StackLoginBonusDialog(UserLoginBonusInfo userLoginBonus)
    {
        // まだスタックされていないときだけダイアログをスタックする
        if (LoginBonusDialogFactory.IsStacked(userLoginBonus.id)) return;

        LoginBonusDialogFactory.Push(new LoginBonusDialogRequest() { userLoginBonus = userLoginBonus })
            .Do(res => {
                // 再生が終わったらPrefsから対象ユーザーに関する表示済みのidを削除
                SaveDataUtil.StackableDialog.RemoveUserLoginBonusId(userLoginBonus.id);
            })
            .Subscribe(); 
    }
}