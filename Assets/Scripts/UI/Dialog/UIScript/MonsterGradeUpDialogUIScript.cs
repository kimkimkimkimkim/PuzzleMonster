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
                title = "����",
                content = $"�O���[�h{userMonster.customData.grade}�ɂȂ�܂���",
            }))
            .Subscribe();

        RefreshScroll();
        RefreshUI();
    }

    /// <summary>
    /// RefreshScroll�̌�Ɏ��s����K�v����
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
            // �K�v�f�ނ�����Ă��Ȃ����̂�������Ίo���\
            var possessedNum = ClientItemUtil.GetPossessedNum(i.itemType, i.itemId);
            return possessedNum < i.num;
        });

        // ������x���e�L�X�g
        _beforeMaxLevelText.text = $"������x��: {beforeMaxLevel}";
        _afterMaxLevelText.text = $"������x��: {afterMaxLevel}";

        // �O���[�h�A�C�R��
        _beforeMonsterGradeParts.SetGradeImage(beforeGrade);
        _afterMonsterGradeParts.SetGradeImage(afterGrade);

        // ���ʃe�L�X�g
        _canGradeUpTextBase.SetActive(canGradeUp);
        _canNotGradeUpTextBase.SetActive(!canGradeUp);

        // OK�{�^���O���[�A�E�g
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

        // �o���̕K�v�f�ނ�Property�Ɍ���
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
