using UniRx;
using UnityEngine;
using GameBase;
using TMPro;

[ResourcePath("UI/Dialog/Dialog-MonsterLuckUpFx")]
public class MonsterLuckUpFxDialogUIScript : MonsterStrengthFxDialogBaseUIScript
{
    [SerializeField] protected TextMeshProUGUI _luckUpText;

    public override void Init(DialogInfo info)
    {
        base.Init(info);

        var beforeLuck = (int)info.param["beforeLuck"];
        var afterLuck = (int)info.param["afterLuck"];

        SetUI(beforeLuck, afterLuck);
        PlayAnimationObservable().Subscribe();
    }

    private void SetUI(int beforeLuck, int afterLuck)
    {
        _luckUpText.text = $"ÉâÉbÉN: {beforeLuck} Å® <color=#00E3FF>{afterLuck}</color>";
    }

    public override void Back(DialogInfo info)
    {
    }
    public override void Close(DialogInfo info)
    {
    }
    public override void Open(DialogInfo info)
    {
    }
}
