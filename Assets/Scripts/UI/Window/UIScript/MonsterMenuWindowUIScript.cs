using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterMenu")]
public class MonsterMenuWindowUIScript : WindowBase
{
    [SerializeField] protected Button _boxButton;
    [SerializeField] protected Button _formationButton;
    [SerializeField] protected Button _itemBoxButton;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _boxButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterBoxWindowFactory.Create(new MonsterBoxWindowRequest()
            {
                userMontserList = ApplicationContext.userData.userMonsterList,
            }))
            .Subscribe();

        _formationButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterPartyListWindowFactory.Create(new MonsterPartyListWindowRequest() { }))
            .Subscribe();

        _itemBoxButton.OnClickIntentAsObservable()
            .SelectMany(_ => ItemBoxWindowFactory.Create(new ItemBoxWindowRequest()))
            .Subscribe();
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
