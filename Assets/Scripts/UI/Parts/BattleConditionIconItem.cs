using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleConditionIconItem")]
public class BattleConditionIconItem : MonoBehaviour
{
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected Text _turnText;
    [SerializeField] protected Sprite _emptySprite;

    private long currentBattleConditionId;

    public void SetInfo(BattleConditionInfo battleCondition)
    {
        currentBattleConditionId = battleCondition.battleConditionId;
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.BattleCondition, battleCondition.battleConditionId)
            .Where(_ => currentBattleConditionId == battleCondition.battleConditionId)
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
