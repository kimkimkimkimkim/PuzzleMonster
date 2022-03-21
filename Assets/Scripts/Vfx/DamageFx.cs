using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageFx : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _normalDamageText;
    [SerializeField] protected TextMeshProUGUI _advantageDamageText;
    [SerializeField] protected TextMeshProUGUI _criticalDamageText;
    [SerializeField] protected TextMeshProUGUI _blockDamageText;
    [SerializeField] protected TextMeshProUGUI _healText;
    [SerializeField] protected CanvasGroup _canvasGroup;

    public CanvasGroup canvasGroup { get { return _canvasGroup; } }

    private void ShowAllText(bool isShow)
    {
        _normalDamageText.gameObject.SetActive(isShow);
        _advantageDamageText.gameObject.SetActive(isShow);
        _criticalDamageText.gameObject.SetActive(isShow);
        _blockDamageText.gameObject.SetActive(isShow);
        _healText.gameObject.SetActive(isShow);
    }

    public void SetText(BeDoneBattleMonsterData beDoneBattleMonsterData)
    {
        ShowAllText(false);

        if( beDoneBattleMonsterData.hpChanges > 0)
        {
            // 回復
            _healText.text = $"+{beDoneBattleMonsterData.hpChanges}";
            _healText.gameObject.SetActive(true);
        }
        else
        {
            if (beDoneBattleMonsterData.isCritical)
            {
                _criticalDamageText.text = beDoneBattleMonsterData.hpChanges.ToString();
                _criticalDamageText.gameObject.SetActive(true);
            }
            else if (beDoneBattleMonsterData.isBlocked)
            {
                _blockDamageText.text = beDoneBattleMonsterData.hpChanges.ToString();
                _blockDamageText.gameObject.SetActive(true);
            }
            // TODO: 有利属性のパターン
            /*
            else if (beDoneBattleMonsterData.isAdvantageAttribute)
            {
                _advantageDamageText.text = beDoneBattleMonsterData.hpChanges.ToString();
                _advantageDamageText.gameObject.SetActive(true);
            }
            `*/
            else 
            {
                _normalDamageText.text = beDoneBattleMonsterData.hpChanges.ToString();
                _normalDamageText.gameObject.SetActive(true);
            }
        }
    }
}
