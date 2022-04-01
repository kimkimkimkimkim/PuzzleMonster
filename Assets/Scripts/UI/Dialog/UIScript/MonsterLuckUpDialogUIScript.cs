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
using TMPro;

[ResourcePath("UI/Dialog/Dialog-MonsterLuckUp")]
public class MonsterLuckUpDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _luckUpButton;
    [SerializeField] protected Button _minusButton;
    [SerializeField] protected Button _plusButton;
    [SerializeField] protected IconItem _iconItem;
    [SerializeField] protected TextMeshProUGUI _confirmText;
    [SerializeField] protected TextMeshProUGUI _beforeLuckText;
    [SerializeField] protected TextMeshProUGUI _afterLuckText;
    [SerializeField] protected TextMeshProUGUI _beforeStackText;
    [SerializeField] protected TextMeshProUGUI _afterStackText;
    [SerializeField] protected TextMeshProUGUI _consumeStackNumText;
    [SerializeField] protected Slider _consumeStackSlider;
    [SerializeField] protected GameObject _luckUpButtonGrayoutPanel;

    private bool isNeedRefresh;
    private UserMonsterInfo userMonster;
    private MonsterMB monster;
    private int consumeStackNum;
    private int maxConsumeStackNum;
    private int minConsumeStackNum = 0;

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

        _luckUpButton.OnClickIntentAsObservable()
            .SelectMany(_ => ApiConnection.MonsterLuckUp(userMonster.id, consumeStackNum))
            .Do(res =>
            {
                isNeedRefresh = true;
                userMonster = res.userMonster;
                SetMaxConsumeStackNum();
                SetSliderValue();
                RefreshUI();
            })
            .SelectMany(res => CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.YesOnly,
                title = "限界突破結果",
                content = $"ラック{userMonster.customData.luck}になりました",
            }))
            .Subscribe();

        _minusButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                consumeStackNum = Math.Max(consumeStackNum - 1, minConsumeStackNum);
                _consumeStackSlider.value = consumeStackNum;
                RefreshUI();
            })
            .Subscribe();

        _plusButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                consumeStackNum = Math.Min(consumeStackNum + 1, maxConsumeStackNum);
                _consumeStackSlider.value = consumeStackNum;
                RefreshUI();
            })
            .Subscribe();

        SetMaxConsumeStackNum();
        SetSliderValue();
        RefreshUI();

        // スライダーの宣言は強化後レベルの計算が終わってから行う
        _consumeStackSlider.OnValueChangedIntentAsObservable()
           .Do(value =>
           {
               consumeStackNum = (int)Math.Round(value, MidpointRounding.AwayFromZero);

               // 最小or最大ならスライダーの値を変更
               if (consumeStackNum == minConsumeStackNum || consumeStackNum == maxConsumeStackNum)
               {
                   // floatの比較は誤差があるので以下のように比較
                   if (!Mathf.Approximately(value, consumeStackNum)) _consumeStackSlider.value = consumeStackNum;
               }

               RefreshUI();
           })
           .Subscribe();
    }

    /// <summary>
    /// 消費可能な最大スタック数を設定
    /// </summary>
    private void SetMaxConsumeStackNum()
    {
        // ラックは自身のレベルより大きくはできない
        var maxLuck = Math.Min(userMonster.customData.level, ConstManager.Monster.MAX_LUCK);
        var needMaxLuckConsumeStack = (maxLuck - userMonster.customData.luck) / ConstManager.Monster.LUCK_UP_NUM(monster.isGachaMonster);
        maxConsumeStackNum = Math.Min(needMaxLuckConsumeStack, userMonster.GetStack());

        // 最初の強化後レベルは最大強化後レベルに設定
        consumeStackNum = maxConsumeStackNum;
    }

    private void SetSliderValue()
    {
        _consumeStackSlider.minValue = minConsumeStackNum;
        _consumeStackSlider.maxValue = maxConsumeStackNum;
        _consumeStackSlider.value = consumeStackNum;
    }

    /// <summary>
    /// 強化後のレベルに合わせてUIを更新
    /// </summary>
    private void RefreshUI()
    {
        // アイコン
        _iconItem.SetIcon(ItemType.Monster, monster.id);

        // 本文
        _confirmText.text = $"スタックを <color=#D14B39><size=46>{consumeStackNum}</size></color> 消費します\nよろしいですか？";

        // ステータステキスト
        var luckUpNum = ConstManager.Monster.LUCK_UP_NUM(monster.isGachaMonster) * consumeStackNum;
        var afterLuck = userMonster.customData.luck + luckUpNum;
        _beforeLuckText.text = userMonster.customData.luck.ToString();
        _afterLuckText.text = $"(+{luckUpNum}) {afterLuck}";
        _beforeStackText.text = userMonster.GetStack().ToString();
        _afterStackText.text = $"(-{consumeStackNum}) {Math.Max(userMonster.GetStack() - consumeStackNum, 0)}";

        // 消費量テキスト
        _consumeStackNumText.text = $"スタック消費量: {consumeStackNum}";

        // グレーアウトパネル
        _luckUpButtonGrayoutPanel.SetActive(consumeStackNum <= 0);
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
