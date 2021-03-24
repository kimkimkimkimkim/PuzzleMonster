using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterBox")]
public class MonsterBoxWindowUIScript : WindowBase
{
    [SerializeField] protected Button _backButton;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _backButton.OnClickIntentAsObservable()
            .Do(_ => UIManager.Instance.CloseWindow())
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
