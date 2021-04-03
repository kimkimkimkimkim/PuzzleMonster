using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterMenu")]
public class MonsterMenuWindowUIScript : WindowBase
{
    [SerializeField] protected Button _boxButton;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _boxButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterBoxWindowFactory.Create(new MonsterBoxWindowRequest()
            {
                userMontserList = ApplicationContext.userData.userMonsterList,
            }))
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
