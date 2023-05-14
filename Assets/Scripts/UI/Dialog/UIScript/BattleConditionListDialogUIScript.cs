using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;

[ResourcePath("UI/Dialog/Dialog-BattleConditionList")]
public class BattleConditionListDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Text _monsterNameText;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected Text _text;

    private List<BattleConditionInfo> targetBattleConditionList;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var battleMonster = (BattleMonsterInfo)info.param["battleMonster"];
        targetBattleConditionList = battleMonster.battleConditionList;

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ =>
            {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        _text.text = $"" +
            $"最大HP: {battleMonster.maxHp}\n" +
            $"現在HP: {battleMonster.currentHp}\n" +
            $"現在攻撃力: {battleMonster.currentAttack()}\n" +
            $"現在防御力: {battleMonster.currentDefense()}\n" +
            $"現在スピード: {battleMonster.currentSpeed()}\n" +
            $"現在シールド: {battleMonster.shield()}";

        SetMonsterNameText(battleMonster);
        RefreshScroll();
    }

    private void SetMonsterNameText(BattleMonsterInfo battleMonster)
    {
        var possess = battleMonster.index.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        _monsterNameText.text = $"{possess}{monster.name}に発動中のステータス効果";
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        _infiniteScroll.Init(targetBattleConditionList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((targetBattleConditionList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<BattleConditionScrollItem>();
        var battleCondition = targetBattleConditionList[index];

        scrollItem.SetInfo(battleCondition);
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