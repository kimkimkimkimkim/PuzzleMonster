using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.Item;
using UnityEngine.EventSystems;
using UniRx.Triggers;

[ResourcePath("UI/Dialog/Dialog-MonsterLevelUp")]
public class MonsterLevelUpDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _levelUpButton;
    [SerializeField] protected Button _minusButton;
    [SerializeField] protected Button _plusButton;
    [SerializeField] protected Text _hpValueText;
    [SerializeField] protected Text _attackValueText;
    [SerializeField] protected Text _defenseValueText;
    [SerializeField] protected Text _healValueText;
    [SerializeField] protected Text _speedValueText;
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _expNumNeededToOneLevelUpText;
    [SerializeField] protected Text _possessionExpNumText;
    [SerializeField] protected Text _consumedExpNumText;
    [SerializeField] protected Slider _hpSliderBack;
    [SerializeField] protected Slider _hpSliderAfterValue;
    [SerializeField] protected Slider _hpSliderFront;
    [SerializeField] protected Slider _attackSliderBack;
    [SerializeField] protected Slider _attackSliderAfterValue;
    [SerializeField] protected Slider _attackSliderFront;
    [SerializeField] protected Slider _defenseSliderBack;
    [SerializeField] protected Slider _defenseSliderAfterValue;
    [SerializeField] protected Slider _defenseSliderFront;
    [SerializeField] protected Slider _healSliderBack;
    [SerializeField] protected Slider _healSliderAfterValue;
    [SerializeField] protected Slider _healSliderFront;
    [SerializeField] protected Slider _speedSliderBack;
    [SerializeField] protected Slider _speedSliderAfterValue;
    [SerializeField] protected Slider _speedSliderFront;
    [SerializeField] protected Slider _levelSlider;
    [SerializeField] protected GameObject _levelUpButtonGrayoutPanel;
    [SerializeField] protected ObservableEventTrigger _levelSliderEventTrigger;

    private bool isNeedRefresh;
    private UserMonsterInfo userMonster;
    private MonsterMB monster;
    private int afterLevel;
    private int maxAfterLevel;
    private int minAfterLevel;
    private int beforeLevelForFx;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action<bool>)info.param["onClickClose"];
        userMonster = (UserMonsterInfo)info.param["userMonster"];

        monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
        beforeLevelForFx = userMonster.customData.level;

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ =>
            {
                if (onClickClose != null)
                {
                    onClickClose(isNeedRefresh);
                    onClickClose = null;
                }
            })
            .Subscribe();

        _levelUpButton.OnClickIntentAsObservable()
            .Where(_ => ApplicationContext.userData.userMonsterList.Any())
            .SelectMany(_ =>
            {
                var targetLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll().First(m => m.level == afterLevel);
                var consumedExp = Math.Max(targetLevelUpTable.totalRequiredExp - userMonster.customData.exp, 0);
                return ApiConnection.MonsterLevelUp(userMonster.id, consumedExp);
            })
            .Do(_ =>
            {
                isNeedRefresh = true;
                userMonster = ApplicationContext.userData.userMonsterList.First(u => u.id == userMonster.id);
                SetAfterLevel();
                SetSliderValue();
                RefreshUI();
            })
            .SelectMany(res =>
            {
                return MonsterLevelUpFxDialogFactory.Create(new MonsterLevelUpFxDialogRequest()
                {
                    beforeLevel = beforeLevelForFx,
                    afterLevel = res.level,
                }).Do(_ => beforeLevelForFx = res.level);
            })
            .Subscribe();

        _minusButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                afterLevel = Math.Max(afterLevel - 1, minAfterLevel);
                _levelSlider.value = afterLevel;
                RefreshUI();
            })
            .Subscribe();

        _plusButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                afterLevel = Math.Min(afterLevel + 1, maxAfterLevel);
                _levelSlider.value = afterLevel;
                RefreshUI();
            })
            .Subscribe();

        SetAfterLevel();
        SetSliderValue();
        RefreshUI();

        // レベルスライダーの宣言は強化後レベルの計算が終わってから行う
        _levelSlider.OnValueChangedIntentAsObservable()
           .Do(value =>
           {
               // 強化後のレベルはスライダーの値を四捨五入した値
               afterLevel = (int)Math.Round(value, MidpointRounding.AwayFromZero);
               RefreshUI();
           })
           .Subscribe();

        _levelSliderEventTrigger.OnPointerUpAsObservable()
            .Do(_ =>
            {
                // スライダーから手を離したら値を固定
                _levelSlider.value = afterLevel;
            })
            .Subscribe();
    }

    /// <summary>
    /// 強化後レベル関係のデータを設定
    /// </summary>
    private void SetAfterLevel()
    {
        // 最小強化後レベルは現在レベル
        minAfterLevel = userMonster.customData.level;

        var monsterExp = ApplicationContext.userData.userPropertyList.GetNum(PropertyType.MonsterExp);

        // 所持している経験値を全て使用した際のモンスター総経験値量
        var maxExp = userMonster.customData.exp + monsterExp;

        // maxExpで到達可能なレベルのレベルアップテーブルマスタ
        var targetLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll()
            .OrderBy(m => m.totalRequiredExp)
            .LastOrDefault(m => m.totalRequiredExp <= maxExp);
        var maxAfterLevelByExp = targetLevelUpTable == null ? minAfterLevel : targetLevelUpTable.level;

        // モンスターのレアリティ、グレードによる最大レベルを取得
        var maxAfterLevelByInfo = ClientMonsterUtil.GetMaxMonsterLevel(monster.rarity, userMonster.customData.grade) > 0 ?
            ClientMonsterUtil.GetMaxMonsterLevel(monster.rarity, userMonster.customData.grade) :
            minAfterLevel;

        // 最大強化後レベルを設定
        maxAfterLevel = Math.Min(maxAfterLevelByExp, maxAfterLevelByInfo);
        if (maxAfterLevel < minAfterLevel) maxAfterLevel = minAfterLevel;

        // 最初の強化後レベルは最大強化後レベルに設定
        afterLevel = maxAfterLevel;
    }

    private void SetSliderValue()
    {
        // モンスター全体の最大値
        _hpSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _hpSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _hpSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _attackSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _attackSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _attackSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _defenseSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _defenseSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _defenseSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _healSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _healSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _healSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _speedSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _speedSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _speedSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;

        // モンスター固有の最大値
        var level100MonsterStatus = MonsterUtil.GetMonsterStatus(monster, 100);
        _hpSliderBack.value = level100MonsterStatus.hp;
        _attackSliderBack.value = level100MonsterStatus.attack;
        _defenseSliderBack.value = level100MonsterStatus.defense;
        _healSliderBack.value = level100MonsterStatus.heal;
        _speedSliderBack.value = level100MonsterStatus.speed;

        // モンスターの現在値
        var status = MonsterUtil.GetMonsterStatus(monster, userMonster.customData.level);
        _hpSliderFront.value = status.hp;
        _attackSliderFront.value = status.attack;
        _defenseSliderFront.value = status.defense;
        _healSliderFront.value = status.heal;
        _speedSliderFront.value = status.speed;

        // レベルスライダー
        _levelSlider.minValue = minAfterLevel;
        _levelSlider.maxValue = maxAfterLevel;
        _levelSlider.value = afterLevel;
    }

    /// <summary>
    /// 強化後のレベルに合わせてUIを更新
    /// </summary>
    private void RefreshUI()
    {
        // ステータステキスト
        var currentStatus = MonsterUtil.GetMonsterStatus(monster, userMonster.customData.level);
        var afterStatus = MonsterUtil.GetMonsterStatus(monster, afterLevel);
        _hpValueText.text = GetStatusValueText(afterStatus.hp, afterStatus.hp - currentStatus.hp);
        _attackValueText.text = GetStatusValueText(afterStatus.attack, afterStatus.attack - currentStatus.attack);
        _defenseValueText.text = GetStatusValueText(afterStatus.defense, afterStatus.defense - currentStatus.defense);
        _healValueText.text = GetStatusValueText(afterStatus.heal, afterStatus.heal - currentStatus.heal);
        _speedValueText.text = GetStatusValueText(afterStatus.speed, afterStatus.speed - currentStatus.speed);

        // ステータススライダー
        _hpSliderAfterValue.value = afterStatus.hp;
        _attackSliderAfterValue.value = afterStatus.attack;
        _defenseSliderAfterValue.value = afterStatus.defense;
        _healSliderAfterValue.value = afterStatus.heal;
        _speedSliderAfterValue.value = afterStatus.speed;

        // レベルテキスト
        _levelText.text = GetLevelText(userMonster.customData.level, afterLevel);

        // 経験値
        var targetLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll().FirstOrDefault(m => m.level == afterLevel);
        var consumedExp = targetLevelUpTable == null ? 0 : Math.Max(targetLevelUpTable.totalRequiredExp - userMonster.customData.exp, 0);
        var monsterExp = ApplicationContext.userData.userPropertyList.GetNum(PropertyType.MonsterExp);
        _possessionExpNumText.text = GetPossessionExpNumText(monsterExp, monsterExp - consumedExp);
        _consumedExpNumText.text = consumedExp.ToString();

        // 1レベル上げるのに必要な経験値量
        var maxMonsterLevel = ClientMonsterUtil.GetMaxMonsterLevel(monster.rarity, userMonster.customData.grade);
        var nextMonsterLevel = Math.Min(afterLevel + 1, maxMonsterLevel);
        var nextLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll().FirstOrDefault(m => m.level == nextMonsterLevel);
        _expNumNeededToOneLevelUpText.text = afterLevel + 1 <= maxMonsterLevel && nextLevelUpTable != null ? $"{nextMonsterLevel}まであと{nextLevelUpTable.requiredExp}" : "MAX";

        // グレーアウトパネル
        _levelUpButtonGrayoutPanel.SetActive(consumedExp <= 0);
    }

    /// <summary>
    /// ステータステキストで使用する文字列を取得
    /// </summary>
    private string GetStatusValueText(int afterValue, int increaseValue)
    {
        return $"{afterValue} <color=\"blue\">+{increaseValue}</color>";
    }

    /// <summary>
    /// レベルテキストで使用する文字列を取得
    /// </summary>
    /// <returns>The level text.</returns>
    private string GetLevelText(int currentValue, int afterValue)
    {
        var maxMonsterLevel = ClientMonsterUtil.GetMaxMonsterLevel(monster.rarity, userMonster.customData.grade);
        return $"Lv.{currentValue}<color=\"blue\"> → {afterValue}/{maxMonsterLevel}</color>";
    }

    /// <summary>
    /// 所持経験値テキストで使用する文字列を取得
    /// </summary>
    /// <returns>The exp number text.</returns>
    private string GetPossessionExpNumText(long currentNum, long afterNum)
    {
        return $"{currentNum} <color=\"#EC2E41\">→ {afterNum}</color>";
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