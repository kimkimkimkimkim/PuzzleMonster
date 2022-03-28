using GameBase;
using TMPro;
using UnityEngine;

[ResourcePath("UI/Parts/Parts-BattleConditionScrollItem")]
public class BattleConditionScrollItem : MonoBehaviour
{
    [SerializeField] protected BattleConditionIconItem _battleConditionIconItem;
    [SerializeField] protected TextMeshProUGUI _battleConditionDescriptionText;

    public void SetInfo(BattleConditionInfo battleCondition)
    {
        _battleConditionIconItem.SetInfo(battleCondition);
        _battleConditionDescriptionText.text = battleCondition.battleCondition.description;
    }
}