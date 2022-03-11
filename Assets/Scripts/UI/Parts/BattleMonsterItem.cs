using GameBase;
using PM.Enum.Monster;
using PM.Enum.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleMonsterItem")]
public class BattleMonsterItem : MonoBehaviour
{
    [SerializeField] protected RectTransform _rectTransform;
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected Image _attributeImage;
    [SerializeField] protected Slider _hpSlider;
    [SerializeField] protected Slider _energySlider;
    [SerializeField] protected TextMeshProUGUI _levelText;
    [SerializeField] protected TextMeshProUGUI _missText;
    [SerializeField] protected List<BattleConditionIconItem> _battleConditionIconItemList;

    public RectTransform rectTransform { get { return _rectTransform; } }
    public Image monsterImage { get { return _monsterImage; } }
    public Slider hpSlider { get { return _hpSlider; } }
    public Slider energySlider { get { return _energySlider; } }
    public TextMeshProUGUI missText { get { return _missText; } }

    private IDisposable battleConditionAnimationObservable;

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

        _energySlider.maxValue = ConstManager.Battle.MAX_ENERGY_VALUE;
        _energySlider.value = ConstManager.Battle.INITIAL_ENERGY_VALUE;
    }

    public void RefreshBattleCondition(List<BattleConditionInfo> battleConditionList)
    {
        const int BATTLE_CONDITION_NUM = 4;
        const float BATTLE_CONDITION_ANIMATION_TIME = 1.5f;

        if(battleConditionAnimationObservable != null)
        {
            battleConditionAnimationObservable.Dispose();
            battleConditionAnimationObservable = null;
        }

        if(battleConditionList.Count <= BATTLE_CONDITION_NUM)
        {
            // 状態異常が4つ以下ならアニメーションなしで表示
            _battleConditionIconItemList.ForEach(i => i.ResetInfo());
            battleConditionList.ForEach((battleCondition, index) =>
            {
                var battleConditionIconItem = _battleConditionIconItemList[index];
                battleConditionIconItem.SetInfo(battleCondition);
            });
        }
        else
        {
            battleConditionAnimationObservable = Observable.Interval(TimeSpan.FromSeconds(BATTLE_CONDITION_ANIMATION_TIME))
                .Do(count =>
                {

                })
                .Subscribe();
        }
    }
}