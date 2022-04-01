using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using TMPro;
using PM.Enum.Item;

[ResourcePath("UI/Dialog/Dialog-MonsterGradeUp")]
public class MonsterGradeUpDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected MonsterGradeParts _beforeMonsterGradeParts;
    [SerializeField] protected MonsterGradeParts _afterMonsterGradeParts;
    [SerializeField] protected TextMeshProUGUI _beforeMaxLevelText;
    [SerializeField] protected TextMeshProUGUI _afterMaxLevelText;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected GameObject _canGradeUpTextBase;
    [SerializeField] protected GameObject _canNotGradeUpTextBase;
    [SerializeField] protected GameObject _okButtonGrayoutPanel;

    private UserMonsterInfo userMonster;
    private List<ItemMI> requiredItemList;
    private bool isNeedRefresh;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action<bool>)info.param["onClickClose"];
        userMonster = (UserMonsterInfo)info.param["userMonster"];

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

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ =>ApiConnection.MonsterGradeUp(userMonster.id))
            .Do(res =>
            {
                isNeedRefresh = true;
                userMonster = res.userMonster;
                RefreshScroll();
                RefreshUI();
            })
            .SelectMany(res => CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.YesOnly,
                title = "結果",
                content = $"グレード{userMonster.customData.grade}になりました",
            }))
            .Subscribe();

        RefreshScroll();
        RefreshUI();
    }

    /// <summary>
    /// RefreshScrollの後に実行する必要あり
    /// </summary>
    private void RefreshUI()
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
        var beforeGrade = userMonster.customData.grade;
        var afterGrade = beforeGrade + 1;
        var beforeMaxLevel = ClientMonsterUtil.GetMaxMonsterLevel(monster.rarity, beforeGrade);
        var afterMaxLevel = ClientMonsterUtil.GetMaxMonsterLevel(monster.rarity, afterGrade);
        var canGradeUp = !requiredItemList.Any(i =>
        {
            // 必要素材が足りていないものが無ければ覚醒可能
            var possessedNum = ClientItemUtil.GetPossessedNum(i.itemType, i.itemId);
            return possessedNum < i.num;
        });

        // 上限レベルテキスト
        _beforeMaxLevelText.text = $"上限レベル: {beforeMaxLevel}";
        _afterMaxLevelText.text = $"上限レベル: {afterMaxLevel}";

        // グレードアイコン
        _beforeMonsterGradeParts.SetGradeImage(beforeGrade);
        _afterMonsterGradeParts.SetGradeImage(afterGrade);

        // 結果テキスト
        _canGradeUpTextBase.SetActive(canGradeUp);
        _canNotGradeUpTextBase.SetActive(!canGradeUp);

        // OKボタングレーアウト
        _okButtonGrayoutPanel.SetActive(!canGradeUp);
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
        var gradeUpTable = MasterRecord.GetMasterOf<GradeUpTableMB>().GetAll().First(m => m.monsterAttribute == monster.attribute && m.targetGrade == userMonster.customData.grade + 1);
        requiredItemList = gradeUpTable != null ? gradeUpTable.requiredItemList : new List<ItemMI>();

        if (requiredItemList.Any()) _infiniteScroll.Init(requiredItemList.Count, OnUpdate);
    }

    private void OnUpdate(int index, GameObject item)
    {
        if ((requiredItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var requiredItem = requiredItemList[index];

        // 覚醒の必要素材はPropertyに限る
        var possessedNum = ClientItemUtil.GetPossessedNum(requiredItem.itemType, requiredItem.itemId);
        var color = possessedNum - requiredItem.num > 0 ? "#FFFFFF" : "#D14B39";
        var numText = $"{possessedNum} / <color={color}>{requiredItem.num}</color>";

        scrollItem.SetIcon(requiredItem);
        scrollItem.SetNumText(numText);
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
