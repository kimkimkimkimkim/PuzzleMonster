using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using TMPro;

[ResourcePath("UI/Dialog/Dialog-PlayerRankUp")]
public class PlayerRankUpDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected TextMeshProUGUI _mainRankText;
    [SerializeField] protected TextMeshProUGUI _beforeRankText;
    [SerializeField] protected TextMeshProUGUI _afterRankText;
    [SerializeField] protected TextMeshProUGUI _beforeMaxStaminaText;
    [SerializeField] protected TextMeshProUGUI _afterMaxStaminaText;
    [SerializeField] protected TextMeshProUGUI _riseStaminaText;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var beforeRank = (int)info.param["beforeRank"];
        var afterRank = (int)info.param["afterRank"];
        var beforeMaxStamina = (int)info.param["beforeMaxStamina"];
        var afterMaxStamina = (int)info.param["afterMaxStamina"];

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        _mainRankText.text = afterRank.ToString();
        _beforeRankText.text = beforeRank.ToString();
        _afterRankText.text = afterRank.ToString();
        _beforeMaxStaminaText.text = beforeMaxStamina.ToString();
        _afterMaxStaminaText.text = afterMaxStamina.ToString();
        SetRiseStaminaText(beforeRank, afterRank);
    }

    private void SetRiseStaminaText(int beforeRank, int afterRank)
    {
        var staminaList = MasterRecord.GetMasterOf<StaminaMB>().GetAll().ToList();
        var riseStamina = 0;
        for (var rank = afterRank; rank > beforeRank; rank--)
        {
            var stamina = staminaList.First(m => m.rank == rank);
            riseStamina += stamina.stamina;
        }
        _riseStaminaText.text = $"プレイヤーランクが上がり、スタミナが<color=#F2548D>{riseStamina}</color>しました";
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
