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
    [SerializeField] protected Button _minusButton;
    [SerializeField] protected Button _plusButton;
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

    private bool isNeedRefresh;
    private UserMonsterInfo userMonster;
    private MonsterMB monster;
    private int afterLevel;
    private int maxAfterLevel;
    private int minAfterLevel;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action<bool>)info.param["onClickClose"];
        userMonster = (UserMonsterInfo)info.param["userMonster"];

        monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickClose != null)
                {
                    onClickClose(isNeedRefresh);
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
            .Do(_ =>
            {
                isNeedRefresh = true;
                userMonster = ApplicationContext.userInventory.userMonsterList.First(u => u.id == userMonster.id);
                SetAfterLevel();
                SetSliderValue();
                RefreshUI();
            })
            .SelectMany(res => CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.YesOnly,
                title = "強化結果",
                content = $"{res.level}レベルになりました",
            }))
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

               // 最小or最大ならスライダーの値を変更
               if (afterLevel == minAfterLevel || afterLevel == maxAfterLevel)
               {
                   // floatの比較は誤差があるので以下のように比較
                   if (!Mathf.Approximately(value, afterLevel)) _levelSlider.value = afterLevel;
               }

               RefreshUI();
           })
           .Subscribe();
    }

    /// <summary>
    /// 強化後レベル関係のデータを設定
    /// </summary>
    private void SetAfterLevel() {
        // 最小強化後レベルは現在レベル
        minAfterLevel = userMonster.customData.level;

        var monsterExp = ApplicationContext.userVirtualCurrency.monsterExp;

        // 所持している経験値を全て使用した際のモンスター総経験値量
        var maxExp = userMonster.customData.exp + monsterExp;

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
        var status = MonsterUtil.GetMonsterStatus(monster, userMonster.customData.level);
        _hpSliderFront.value = status.hp;
        _attackSliderFront.value = status.attack;
        _healSliderFront.value = status.heal;

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
        _healValueText.text = GetStatusValueText(afterStatus.heal, afterStatus.heal - currentStatus.heal);

        // ステータススライダー
        _hpSliderAfterValue.value = afterStatus.hp;
        _attackSliderAfterValue.value = afterStatus.attack;
        _healSliderAfterValue.value = afterStatus.heal;

        // レベルテキスト
        _levelText.text = GetLevelText(userMonster.customData.level, afterLevel);

        // 経験値
        var targetLevelUpTable = MasterRecord.GetMasterOf<MonsterLevelUpTableMB>().GetAll().FirstOrDefault(m => m.level == afterLevel);
        var consumedExp = targetLevelUpTable == null ? 0 : Math.Max(targetLevelUpTable.totalRequiredExp - userMonster.customData.exp,0);
        var monsterExp = ApplicationContext.userVirtualCurrency.monsterExp;
        _possessionExpNumText.text = GetPossessionExpNumText(monsterExp, monsterExp - consumedExp);
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
