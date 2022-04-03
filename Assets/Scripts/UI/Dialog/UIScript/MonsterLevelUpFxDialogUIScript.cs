using UniRx;
using UnityEngine;
using GameBase;
using TMPro;

[ResourcePath("UI/Dialog/Dialog-MonsterLevelUpFx")]
public class MonsterLevelUpFxDialogUIScript : MonsterStrengthFxDialogBaseUIScript
{
    [SerializeField] TextMeshProUGUI _levelUpText;

    public override void Init(DialogInfo info)
    {
        base.Init(info);

        var beforeLevel = (int)info.param["beforeLevel"];
        var afterLevel = (int)info.param["afterLevel"];

        SetUI(beforeLevel, afterLevel);
        PlayAnimationObservable().Subscribe();
    }

    private void SetUI(int beforeLevel, int afterLevel)
    {
        _levelUpText.text = $"Lv.{beforeLevel} Å® <color=#00E3FF>{afterLevel}</color>";
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
