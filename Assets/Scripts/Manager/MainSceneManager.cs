using GameBase;
using UniRx;

public class MainSceneManager : SingletonMonoBehaviour<MainSceneManager>
{
    private void Start()
    {
        TitleWindowFactory.Create(new TitleWindowRequest())
            .SelectMany(_ => HeaderFooterWindowFactory.Create(new HeaderFooterWindowRequest()))
            .Subscribe();
    }
}
