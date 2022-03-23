using GameBase;
using PM.Enum.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleScoreScrollItem")]
public class BattleScoreScrollItem : MonoBehaviour
{
    [SerializeField] IconItem _iconItem;
    [SerializeField] Slider _giveDamageSlider;
    [SerializeField] Slider _healingSlider;
    [SerializeField] Slider _takeDamageSlider;
    [SerializeField] TextMeshProUGUI _giveDamageValueText;
    [SerializeField] TextMeshProUGUI _healingValueText;
    [SerializeField] TextMeshProUGUI _takeDamageValueText;

    public void SetInfo(BattleMonsterInfo battleMonster, int maxValue)
    {
        _iconItem.SetIcon(ItemType.Monster, battleMonster.monsterId);
        _giveDamageSlider.maxValue = maxValue;
        _healingSlider.maxValue = maxValue;
        _takeDamageSlider.maxValue = maxValue;
        _giveDamageSlider.value = battleMonster.totalGiveDamage;
        _healingSlider.value = battleMonster.totalHealing;
        _takeDamageSlider.value = battleMonster.totalTakeDamage;
        _giveDamageValueText.text = battleMonster.totalGiveDamage.ToString();
        _healingValueText.text = battleMonster.totalHealing.ToString();
        _takeDamageValueText.text = battleMonster.totalTakeDamage.ToString();
    }
}