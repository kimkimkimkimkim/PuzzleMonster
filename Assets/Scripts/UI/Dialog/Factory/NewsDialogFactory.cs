using UniRx;
using GameBase;
using System;
using PM.Enum.UI;
using PM.Enum.News;

public class NewsDialogFactory : IStackableDialogFactory {

    private static StackableDialogPriority priority = StackableDialogPriority.News;
    private static string type(string userNewsId) => $"NewsDialog_{userNewsId}";

    public static IObservable<NewsDialogResponse> Push(UserNewsInfo userNews) {
        var news = MasterRecord.GetMasterOf<NewsMB>().Get(userNews.newsId);
        switch (news.type) {
            case NewsType.Message: {
                    // メッセージタイプの場合はコモンダイアログを表示する
                    var create = CommonDialogFactory.Create(new CommonDialogRequest() {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = news.title,
                        content = news.message,
                    });
                    return StackableDialogManager.Instance.Push(create, (int)priority, type(userNews.id)).Select(_ => new NewsDialogResponse());
                }
            case NewsType.Present: {
                    // プレゼントタイプの場合はコモンレシーブダイアログを表示する
                    var create = CommonReceiveDialogFactory.Create(new CommonReceiveDialogRequest() {
                        itemList = news.itemList,
                        title = news.title,
                        content = news.message,
                    });
                    return StackableDialogManager.Instance.Push(create, (int)priority, type(userNews.id)).Select(_ => new NewsDialogResponse());
                }
            default:
                return Observable.Return(new NewsDialogResponse());
        }
    }

    public static bool IsStacked(string userNewsId) {
        return StackableDialogManager.Instance.IsStacked(type(userNewsId));
    }
}
