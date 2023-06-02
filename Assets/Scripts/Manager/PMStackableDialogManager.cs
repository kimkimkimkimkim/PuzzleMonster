
using GameBase;
using PM.Enum.Condition;
using PM.Enum.Notification;
using System;
using System.Linq;
using UniRx;

public class PMStackableDialogManager : SingletonMonoBehaviour<PMStackableDialogManager> {

    /// <summary>
    /// 未処理の通知ダイアログをスタック
    /// </summary>
    public void Restack(bool isLoginTiming = false) {
        // ログインボーナス
        var userLoginBonusIdList = SaveDataUtil.StackableDialog.GetUserLoginBonusIdList();
        userLoginBonusIdList.ForEach(id => {
            var userLoginBonus = ApplicationContext.userData.userLoginBonusList.FirstOrDefault(u => u.id == id);
            if (userLoginBonus == null) {
                SaveDataUtil.StackableDialog.RemoveUserLoginBonusId(id);
                return;
            }

            // 今日のでなければスキップ
            var lastDate = userLoginBonus.loginDateList.MaxOrDefault();
            var isSameGameDate = DateTimeUtil.IsSameGameDate(lastDate, DateTimeUtil.Now);
            if (!isSameGameDate) {
                SaveDataUtil.StackableDialog.RemoveUserLoginBonusId(id);
                return;
            }

            // スタックに積む
            StackLoginBonusDialog(userLoginBonus);
        });

        // お知らせ
        var userNewsIdList = SaveDataUtil.StackableDialog.GetUserNewsIdList();
        userNewsIdList.ForEach(id => {
            var userNews = ApplicationContext.userData.userNewsList.FirstOrDefault(u => u.id == id);
            if (userNews == null) {
                SaveDataUtil.StackableDialog.RemoveUserNewsId(id);
                return;
            }

            // 表示条件を満たしていなければ削除
            var news = MasterRecord.GetMasterOf<NewsMB>().Get(userNews.newsId);
            if (!ConditionUtil.IsValid(ApplicationContext.userData, news.conditionList) || !IsShowDialog(userNews, isLoginTiming)) {
                SaveDataUtil.StackableDialog.RemoveUserNewsId(id);
                return;
            }

            // スタックに積む
            StackNewsDialog(userNews);
        });
    }

    public void StackLoginBonusDialog(UserLoginBonusInfo userLoginBonus) {
        // まだスタックされていないときだけダイアログをスタックする
        if (LoginBonusDialogFactory.IsStacked(userLoginBonus.id)) return;

        LoginBonusDialogFactory.Push(new LoginBonusDialogRequest() { userLoginBonus = userLoginBonus })
            .Do(_ => {
                // 再生が終わったらPrefsから対象ユーザーに関する表示済みのidを削除
                SaveDataUtil.StackableDialog.RemoveUserLoginBonusId(userLoginBonus.id);
            })
            .Subscribe();
    }

    public void StackNewsDialog(UserNewsInfo userNews) {
        // まだスタックされていないときだけダイアログをスタックする
        if (NewsDialogFactory.IsStacked(userNews.id)) return;

        NewsDialogFactory.Push(userNews)
            .Do(_ => {
                // 再生が終わったら表示日時を追加
                SaveDataUtil.StackableDialog.AddNewsOpenDate(userNews.id, DateTimeUtil.Now);

                // 削除する必要があればPrefsを削除
                if (IsDeletePrefsIfNeeded(userNews)) SaveDataUtil.StackableDialog.RemoveUserNewsId(userNews.id);
            })
            .Subscribe();
    }

    /// <summary>
    /// 表示履歴からこのダイアログを表示するか否かを返す
    /// </summary>
    private bool IsShowDialog(UserNewsInfo userNews, bool isLoginTiming) {
        var news = MasterRecord.GetMasterOf<NewsMB>().Get(userNews.newsId);
        var newsOpenDateList = SaveDataUtil.StackableDialog.GetNewsOpenDateList(userNews.id);
        switch (news.repeatType) {
            case RepeatType.EveryLogIn:
                // これがログイン時のものであれば表示
                return isLoginTiming;
            case RepeatType.OnceADay:
                // 今日が初めてなら表示
                return !newsOpenDateList.Any(d => DateTimeUtil.IsSameGameDate(d, DateTimeUtil.Now));
            case RepeatType.None:
                // すでに表示していたら表示しない
                return !newsOpenDateList.Any();
            default:
                return true;
        }
    }

    /// <summary>
    /// 今回の表示が最後になるならPrefsを削除する
    /// </summary>
    private bool IsDeletePrefsIfNeeded(UserNewsInfo userNews) {
        var news = MasterRecord.GetMasterOf<NewsMB>().Get(userNews.newsId);
        var newsOpenDateList = SaveDataUtil.StackableDialog.GetNewsOpenDateList(userNews.id);
        switch (news.repeatType) {
            case RepeatType.EveryLogIn:
                // ログインの度表示するのでここでは削除しない
                return false;
            case RepeatType.OnceADay:
                // 今日表示済みか否か
                var isShowToday = newsOpenDateList.Any(d => DateTimeUtil.IsSameGameDate(d, DateTimeUtil.Now));
                if (!isShowToday) return false;

                // 今日が表示の最終日か否か
                var lastDateCondition = news.conditionList.FirstOrDefault(c => c.type == ConditionType.LowerDate);
                var lastDateString = lastDateCondition?.valueString ?? "";
                var lastDate = DateTimeUtil.GetDateFromMasterString(lastDateString);
                var isLastDate = DateTimeUtil.IsSameGameDate(lastDate, DateTimeUtil.Now);
                if (!isLastDate) return false;

                // 今日表示済みかつ今日が表示の最終日になるのであれば削除する
                return true;
            case RepeatType.None:
                // 繰り返しなしなので常に削除する
                return true;
            default:
                return true;
        }
    }
}
