using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using PM.Enum.Item;

[ResourcePath("UI/Dialog/Dialog-MonsterLevelUp")]
public class MonsterLevelUpDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _levelUpButton;
    [SerializeField] protected Text _hpValueText;
    [SerializeField] protected Text _attackValueText;
    [SerializeField] protected Text _healValueText;
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _possessionExpNumText;
    [SerializeField] protected Text _consumedExpNumText;
    [SerializeField] protected Slider _hpSliderBack;
    [SerializeField] protected Slider _hpSliderAfterValue;
    [SerializeField] protected Slider _hpSliderFront;
    [SerializeField] protected Slider _attackSliderBack;
    [SerializeField] protected Slider _attackSliderAfterValue;
    [SerializeField] protected Slider _attackSliderFront;
    [SerializeField] protected Slider _healSliderBack;
    [SerializeField] protected Slider _healSliderAfterValue;
    [SerializeField] protected Slider _healSliderFront;
    [SerializeField] protected Slider _levelSlider;
    [SerializeField] protected GameObject _levelUpButtonGrayoutPanel;

    private UserMonsterInfo userMonster;
    private MonsterMB monster;
    private int afterLevel;
    private int maxAfterLevel;
    private int minAfterLevel;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        userMonster = (UserMonsterInfo)info.param["userMonster"];

        monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        _levelUpButton.OnClickIntentAsObservable()
            .Where(_ => ApplicationContext.userInventory.userMonsterList.Any())
            .SelectMany(_ =>
            {
                var targetLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll().First(m => m.level == afterLevel);
                var consumedExp = Math.Max(targetLevelUpTable.totalRequiredExp - userMonster.customData.exp, 0);
                return ApiConnection.MonsterLevelUp(userMonster.id, consumedExp);
            })
            .SelectMany(res => CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.YesOnly,
                title = "強化結果",
                content = $"{res.level}レベルになりました",
            }))
            .Subscribe();

        _levelSlider.onValueChanged.AsObservable()
            .Do(value =>
            {
                afterLevel = (int)value;
                RefreshUI();
            })
            .Subscribe();

        //SetAfterLevel();
        SetSliderValue();
        RefreshUI();
    }

    /// <summary>
    /// 強化後レベル関係のデータを設定
    /// </summary>
    private void SetAfterLevel() {
        // 最小強化後レベルは現在レベル
        minAfterLevel = userMonster.customData.level;

        var monsterExp = ApplicationContext.userInventory.userPropertyList.GetOrDefault(PropertyType.MonsterExp);
        var monsterExpNum = monsterExp == null ? 0 : monsterExp.num;

        // 所持している経験値を全て使用した際のモンスター経験値量
        var maxExp = userMonster.customData.exp + monsterExpNum;

        // maxExpで到達可能なレベルのレベルアップテーブルマスタ
        var targetLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll()
            .LastOrDefault(m => m.totalRequiredExp <= maxExp);

        // 最大強化後レベルを設定
        maxAfterLevel = targetLevelUpTable == null ? minAfterLevel : targetLevelUpTable.level;
        if (maxAfterLevel < minAfterLevel) maxAfterLevel = minAfterLevel;

        // 最初の強化後レベルは最大強化後レベルに設定
        afterLevel = maxAfterLevel;
    }

    private void SetSliderValue() {
        // モンスター全体の最大値
        _hpSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _hpSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _hpSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _attackSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _attackSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _attackSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _healSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _healSliderAfterValue.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _healSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;

        // モンスター固有の最大値
        _hpSliderBack.value = monster.level100Hp;
        _attackSliderBack.value = monster.level100Attack;
        _healSliderBack.value = monster.level100Heal;

        // モンスターの現在値
        _hpSliderFront.value = userMonster.customData.hp;
        _attackSliderFront.value = userMonster.customData.attack;
        _healSliderFront.value = userMonster.customData.heal;

        // レベルスライダー
        _levelSlider.minValue = minAfterLevel;
        _levelSlider.maxValue = maxAfterLevel;
    }

    /// <summary>
    /// 強化後のレベルに合わせてUIを更新
    /// </summary>
    private void RefreshUI()
    {
        // ステータス
        var afterStatus = MonsterUtil.GetMonsterStatus(monster, afterLevel);
        _hpValueText.text = GetStatusValueText(afterStatus.hp, afterStatus.hp - userMonster.customData.hp);
        _attackValueText.text = GetStatusValueText(afterStatus.attack, afterStatus.attack - userMonster.customData.attack);
        _healValueText.text = GetStatusValueText(afterStatus.heal, afterStatus.heal - userMonster.customData.heal);

        // レベルテキスト
        _levelText.text = GetLevelText(userMonster.customData.level, afterLevel);

        // レベルスライダー
        _levelSlider.value = afterLevel;

        // 経験値
        var targetLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll().First(m => m.level == afterLevel);
        var consumedExp = Math.Max(targetLevelUpTable.totalRequiredExp - userMonster.customData.exp,0);
        var monsterExpNum = ApplicationContext.userInventory.userPropertyList.GetNum(PropertyType.MonsterExp);
        _possessionExpNumText.text = GetPossessionExpNumText(monsterExpNum, monsterExpNum - consumedExp);
        _consumedExpNumText.text = consumedExp.ToString();

        // グレーアウトパネル
        _levelUpButtonGrayoutPanel.SetActive(consumedExp <= 0);
    }

    /// <summary>
    /// ステータステキストで使用する文字列を取得
    /// </summary>
    private string GetStatusValueText(int afterValue,int increaseValue)
    {
        return $"{afterValue} <color=\"blue\">+{increaseValue}</color>";
    }

    /// <summary>
    /// レベルテキストで使用する文字列を取得
    /// </summary>
    /// <returns>The level text.</returns>
    private string GetLevelText(int currentValue,int afterValue)
    {
        return $"Lv.{currentValue}<color=\"blue\"> → {afterValue}/{ConstManager.Monster.MAX_LEVEL}</color>";
    }

    /// <summary>
    /// 所持経験値テキストで使用する文字列を取得
    /// </summary>
    /// <returns>The exp number text.</returns>
    private string GetPossessionExpNumText(int currentNum,int afterNum)
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
