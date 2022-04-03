using UniRx;
using UnityEngine;
using GameBase;
using TMPro;

[ResourcePath("UI/Dialog/Dialog-MonsterGradeUpFx")]
public class MonsterGradeUpFxDialogUIScript : MonsterStrengthFxDialogBaseUIScript
{
    [SerializeField] protected TextMeshProUGUI _beforeGradeText;
    [SerializeField] protected TextMeshProUGUI _afterGradeText;
    [SerializeField] protected MonsterGradeParts _beforeMonsterGradeParts;
    [SerializeField] protected MonsterGradeParts _afterMonsterGradeParts;

    public override void Init(DialogInfo info)
    {
        base.Init(info);

        var beforeGrade = (int)info.param["beforeGrade"];
        var afterGrade = (int)info.param["afterGrade"];

        SetUI(beforeGrade, afterGrade);
        PlayAnimationObservable().Subscribe();
    }

    private void SetUI(int beforeGrade, int afterGrade)
    {
        _beforeGradeText.text = $"グレード: {beforeGrade}";
        _afterGradeText.text = $"グレード: {afterGrade}";
        _beforeMonsterGradeParts.SetGradeImage(beforeGrade);
        _afterMonsterGradeParts.SetGradeImage(afterGrade);
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
