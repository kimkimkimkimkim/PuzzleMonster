using PM.Enum.UI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BattleConditionIconItem : MonoBehaviour
{
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected TextMeshProUGUI _turnText;
    [SerializeField] protected Sprite _emptySprite;

    private long currentBattleConditionId;

    public void SetInfo(BattleConditionInfo battleCondition)
    {
        currentBattleConditionId = battleCondition.battleCondition.id;
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.BattleCondition, battleCondition.battleCondition.id)
            .Where(_ => currentBattleConditionId == battleCondition.battleCondition.id)
            .Do(sprite => _iconImage.sprite = sprite)
            .Subscribe();

        if(battleCondition.remainingTurnNum == 0)
        {
            _turnText.gameObject.SetActive(false);
        }
        else if(battleCondition.remainingTurnNum == -1)
        {
            _turnText.gameObject.SetActive(true);
            _turnText.text = "∞";
        }
        else
        {
            _turnText.gameObject.SetActive(true);
            _turnText.text = battleCondition.remainingTurnNum.ToString();
        }
    }

    public void ResetInfo()
    {
        currentBattleConditionId = 0;
        _iconImage.sprite = _emptySprite;
        _turnText.gameObject.SetActive(false);
    }
}
