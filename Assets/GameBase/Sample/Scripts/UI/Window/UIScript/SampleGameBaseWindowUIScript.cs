using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-SampleGameBase")]
public class SampleGameBaseWindowUIScript : WindowBase
{
    [SerializeField] protected Button _dialogOpenButton;

    public override void Init(WindowInfo info)
    {
        _dialogOpenButton.OnClickIntentAsObservable()
            .SelectMany(_ => SampleGameBaseDialogFactory.Create(new SampleGameBaseDialogRequest()))
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
    }
}