using GameBase;
using UniRx;

public class MainSceneManager : SingletonMonoBehaviour<MainSceneManager>
{
    private void Start()
    {
        TitleWindowFactory.Create(new TitleWindowRequest())
            .SelectMany(_ => HeaderFooterWindowFactory.Create(new HeaderFooterWindowRequest()))
            .Subscribe();

        // デバッグボタンの作成
        var debugItem = UIManager.Instance.CreateContent<DebugItem>(UIManager.Instance.debugParent);
        debugItem.SetOnClickAction(() =>
        {
            TestActionDialogFactory.Create(new TestActionDialogRequest()).Subscribe();
        });
    }
}
