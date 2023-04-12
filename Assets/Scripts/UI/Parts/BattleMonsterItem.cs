using DG.Tweening;
using GameBase;
using PM.Enum.Battle;
using PM.Enum.Item;
using PM.Enum.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleMonsterItem")]
public class BattleMonsterItem : MonoBehaviour
{
    [SerializeField] protected RectTransform _rectTransform;
    [SerializeField] protected Button _button;
    [SerializeField] protected IconItem _monsterIconItem;
    [SerializeField] protected Image _attributeImage;
    [SerializeField] protected Image _graveImage;
    [SerializeField] protected Slider _hpSlider;
    [SerializeField] protected Slider _energySlider;
    [SerializeField] protected Slider _shieldSlider;
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _missText;
    [SerializeField] protected GameObject _monsterImageBase;
    [SerializeField] protected GameObject _shieldSliderBase;
    [SerializeField] protected Transform _effectBase;
    [SerializeField] protected CanvasGroup _battleConditionBaseCanvasGroup;
    [SerializeField] protected List<BattleConditionIconItem> _battleConditionIconItemList;

    private const int BATTLE_CONDITION_NUM = 4;
    private const float BATTLE_CONDITION_ANIMATION_TIME = 1.5f;
    private const float BATTLE_CONDITION_FADE_OUT_TIME = 0.3f;

    public RectTransform rectTransform
    { get { return _rectTransform; } }

    public GameObject monsterImageBase
    { get { return _monsterImageBase; } }

    public Slider hpSlider
    { get { return _hpSlider; } }

    public Slider energySlider
    { get { return _energySlider; } }

    public Slider shieldSlider
    { get { return _shieldSlider; } }

    public Text missText
    { get { return _missText; } }

    public Transform effectBase
    { get { return _effectBase; } }

    private IDisposable battleConditionAnimationObservable;
    private IDisposable onClickButtonObservable;

    public void Init(long monsterId, int level, bool isPlayer)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
        var status = MonsterUtil.GetMonsterStatus(monster, level);
        var rotationY = isPlayer ? 0.0f : 180.0f;

        _monsterIconItem.SetIcon(ItemType.Monster, monsterId);

        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterAttribute, (long)monster.attribute)
            .Do(sprite =>
            {
                if (sprite != null) _attributeImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);

        _monsterIconItem.transform.rotation = Quaternion.Euler(new Vector3(0.0f, rotationY, 0.0f));

        _levelText.text = level.ToString();

        _hpSlider.maxValue = status.hp;
        _hpSlider.value = status.hp;

        _energySlider.maxValue = ConstManager.Battle.MAX_ENERGY_VALUE;
        _energySlider.value = ConstManager.Battle.INITIAL_ENERGY_VALUE;
    }

    public void RefreshShieldSlider(List<BattleConditionInfo> battleConditionList)
    {
        var shieldValue = battleConditionList
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.battleConditionId).battleConditionType == BattleConditionType.Shield)
            .Sum(c => c.shieldValue);
        if (shieldValue <= 0)
        {
            _shieldSliderBase.SetActive(false);
        }
        else
        {
            _shieldSlider.maxValue = shieldValue;
            _shieldSlider.value = shieldValue;
            _shieldSliderBase.SetActive(true);
        }
    }

    public void RefreshBattleCondition(List<BattleConditionInfo> battleConditionList)
    {
        if (battleConditionAnimationObservable != null)
        {
            battleConditionAnimationObservable.Dispose();
            battleConditionAnimationObservable = null;
        }

        if (battleConditionList.Count <= BATTLE_CONDITION_NUM)
        {
            // 状態異常が4つ以下ならアニメーションなしで表示
            SetBattleConditionIcon(battleConditionList);
        }
        else
        {
            battleConditionAnimationObservable = Observable.Interval(TimeSpan.FromSeconds(BATTLE_CONDITION_ANIMATION_TIME))
                .SelectMany(count =>
                {
                    var startIndex = (int)(count * BATTLE_CONDITION_NUM) % (battleConditionList.Count + (BATTLE_CONDITION_NUM - (battleConditionList.Count % BATTLE_CONDITION_NUM)));
                    SetBattleConditionIcon(battleConditionList, startIndex);

                    var fadeOutSequence = DOTween.Sequence()
                        .SetDelay(BATTLE_CONDITION_ANIMATION_TIME - BATTLE_CONDITION_FADE_OUT_TIME - 0.01f)
                        .Append(_battleConditionBaseCanvasGroup.DOFade(0, BATTLE_CONDITION_FADE_OUT_TIME));
                    return fadeOutSequence.OnCompleteAsObservable().AsUnitObservable();
                })
                .Subscribe();
        }

        // シールド値の更新も行う
        RefreshShieldSlider(battleConditionList);
    }

    private void SetBattleConditionIcon(List<BattleConditionInfo> battleConditionList, int startIndex = 0)
    {
        var loopCount = battleConditionList.Count - startIndex >= BATTLE_CONDITION_NUM ? BATTLE_CONDITION_NUM : battleConditionList.Count - startIndex;

        _battleConditionIconItemList.ForEach(i => i.ResetInfo());
        _battleConditionBaseCanvasGroup.alpha = 1.0f;
        for (var i = 0; i < loopCount; i++)
        {
            var battleConditionIconItem = _battleConditionIconItemList[i];
            var battleCondition = battleConditionList[startIndex + i];
            battleConditionIconItem.SetInfo(battleCondition);
        }
    }

    public void ShowShieldSlider(bool isShow)
    {
        _shieldSliderBase.SetActive(isShow);
    }

    public void ShowMonsterImage(bool isShow)
    {
        _monsterIconItem.gameObject.SetActive(isShow);
    }

    public void ShowGraveImage(bool isShow)
    {
        _graveImage.gameObject.SetActive(isShow);
    }

    public void SetOnClickAction(Action action)
    {
        if (action == null) return;

        if (onClickButtonObservable != null)
        {
            onClickButtonObservable.Dispose();
            onClickButtonObservable = null;
        }

        onClickButtonObservable = _button.OnClickAsObservable()
            .Do(_ => action())
            .Subscribe();
    }

    private void OnDestroy()
    {
        if (battleConditionAnimationObservable != null)
        {
            battleConditionAnimationObservable.Dispose();
            battleConditionAnimationObservable = null;
        }
    }
}