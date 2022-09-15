using GameBase;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleConditionScrollItem")]
public class BattleConditionScrollItem : MonoBehaviour
{
    [SerializeField] protected BattleConditionIconItem _battleConditionIconItem;
    [SerializeField] protected Text _battleConditionDescriptionText;

    public void SetInfo(BattleConditionInfo battleCondition)
    {
        _battleConditionIconItem.SetInfo(battleCondition);
        _battleConditionDescriptionText.text = battleCondition.battleCondition.description;
    }
}