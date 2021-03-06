using System.Collections.Generic;
using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterFormation")]
public class MonsterFormationWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected RectTransform _initialMonsterAreaPanel;

    private int partyId;
    private List<UserMonsterInfo> initialUserMonsterList = new List<UserMonsterInfo>();
    private List<UserMonsterInfo> userMonsterList;
    private List<string> selectedUserMonsterIdList = Enumerable.Repeat<string>("", ConstManager.Battle.MAX_PARTY_MEMBER_NUM).ToList();
    private int currentNumber = 1;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        partyId = (int)info.param["partyId"];
        initialUserMonsterList = (List<UserMonsterInfo>)info.param["initialUserMonsterList"];
        userMonsterList = (List<UserMonsterInfo>)info.param["userMonsterList"];

        // すでに編成済みのモンスターを排除
        userMonsterList = userMonsterList.Where(u => !initialUserMonsterList.Any(initialUserMonster => initialUserMonster.id == u.id)).ToList();

        SetInitialMonster();
        RefreshScroll();
    }

    private void SetInitialMonster()
    {
        for(var i = 0; i < ConstManager.Battle.MAX_PARTY_MEMBER_NUM; i++)
        {
            var item = UIManager.Instance.CreateContent<MonsterFormationInitialScrollItem>(_initialMonsterAreaPanel);
            if(i < initialUserMonsterList.Count && initialUserMonsterList[i] != null)
            {
                var userMonster = initialUserMonsterList[i];
                item.SetMonsterImage(userMonster.monsterId);
                item.SetOnClickAction(() => OnClickItemAction(item, userMonster.id));
            }
        }
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        if (userMonsterList.Any()) _infiniteScroll.Init(userMonsterList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<MonsterFormationScrollItem>();
        var userMonster = userMonsterList[index];

        scrollItem.SetGradeImage(userMonster.customData.grade);
        scrollItem.SetMonsterImage(userMonster.monsterId);
        scrollItem.SetOnClickAction(() => OnClickItemAction(scrollItem, userMonster.id));
    }

    private void OnClickItemAction(MonsterFormationScrollItem scrollItem,string userMonsterId)
    {
        var i = selectedUserMonsterIdList.FindIndex(id => id == userMonsterId);
        if (i >= 0)
        {
            // 選択済みなら未選択状態に
            scrollItem.SetSelectionState(0);
            selectedUserMonsterIdList[i] = "";
            currentNumber = selectedUserMonsterIdList.FindIndex(id => id == "") + 1;
        }
        else
        {
            // 未選択なら選択状態に
            scrollItem.SetSelectionState(currentNumber);
            selectedUserMonsterIdList[currentNumber - 1] = userMonsterId;
            currentNumber = selectedUserMonsterIdList.FindIndex(id => id == "") + 1;

            // パーティーメンバー分選択したら確認ダイアログを表示
            if (selectedUserMonsterIdList.All(id => id != ""))
            {
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = PM.Enum.UI.CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "こちらのパーティでよろしいですか？"
                })
                    .Where(res => res.dialogResponseType == PM.Enum.UI.DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.UpdateUserMosnterFormation(partyId, selectedUserMonsterIdList))
                    .Do(_ => UIManager.Instance.CloseWindow())
                    .Do(_ => {
                        if (onClose != null)
                        {
                            onClose();
                            onClose = null;
                        }
                    })
                    .Subscribe();
            }
        }
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
