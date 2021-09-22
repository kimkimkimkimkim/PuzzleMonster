using GameBase;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;

[ResourcePath("UI/Window/Window-QuestSelectParty")]
public class QuestSelectPartyWindowUIScript : WindowBase
{
    [SerializeField] protected TextMeshProUGUI _titleText;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected TabAnimationController _tabAnimationController;
    [SerializeField] protected List<ToggleWithValue> _tabList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        var questId = (long)info.param["questId"];
        var quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        _titleText.text = quest.name;

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                // TODO : partyId‚ÌŽw’è
                return BattleManager.Instance.BattleStartObservable(questId, 1);
            })
            .Subscribe();

        SetTabChangeAction();
        _tabAnimationController.SetTabChangeAnimation();
    }

    private void SetTabChangeAction()
    {

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
