using GameBase;
using PM.Enum.Monster;
using PM.Enum.UI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleMonsterItem")]
public class BattleMonsterItem : MonoBehaviour
{
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected Image _attributeImage;
    [SerializeField] protected Slider _hpSlider;
    [SerializeField] protected Slider _ctSlider;
    [SerializeField] protected TextMeshProUGUI _levelText;

    public void Init(long monsterId, int level)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
        var status = MonsterUtil.GetMonsterStatus(monster, level);

        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .Do(sprite =>
            {
                if (sprite != null) _monsterImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);

        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterAttribute, (long)monster.attribute)
            .Do(sprite =>
            {
                if (sprite != null) _attributeImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);

        _levelText.text = level.ToString();

        _hpSlider.maxValue = status.hp;
        _hpSlider.value = status.hp;

        _ctSlider.maxValue = ConstManager.Battle.MAX_CT_VALUE;
        _ctSlider.value = 0.0f;
    }
}