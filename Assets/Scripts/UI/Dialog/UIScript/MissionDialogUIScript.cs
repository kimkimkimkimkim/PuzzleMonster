using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using PM.Enum.Mission;

[ResourcePath("UI/Dialog/Dialog-Mission")]
public class MissionDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected List<ToggleWithValue> _tabList;
    [SerializeField] protected GameObject _dailyMissionBadge;
    [SerializeField] protected GameObject _mainMissionBadge;
    [SerializeField] protected GameObject _eventMissionBadge;

    private const int DAILY_MISSION_TAB_VALUE = 0;
    private const int MAIN_MISSION_TAB_VALUE = 1;
    private const int EVENT_MISSION_TAB_VALUE = 2;

    private int currentTabValue = DAILY_MISSION_TAB_VALUE;
    private List<MissionMB> targetMissionList;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];

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

        ActivateBadge();
        SetTabChangeAction();
    }

    private void SetTabChangeAction()
    {
        _tabList.ForEach(tab =>
        {
            tab.OnValueChangedIntentAsObservable()
                .Where(isOn => isOn)
                .Do(_ =>
                {
                    currentTabValue = tab.value;
                    RefreshScroll();
                })
                .Subscribe();
        });
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        // カテゴリに含まれるクエストのうち一つでも表示条件を満たしているものがあれば表示する
        targetMissionList = MasterRecord.GetMasterOf<MissionMB>().GetAll()
            .Where(m => m.missionType == GetMissionTypeFromTabValue(currentTabValue))
            .Where(m => ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList))
            .OrderBy(m => m.id)
            .ToList();

        _infiniteScroll.Init(targetMissionList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((targetMissionList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<MissionScrollItem>();
        var mission = targetMissionList[index];
        var firstRewardItem =　mission.rewardItemList.First();
        var canClear = ConditionUtil.IsValid(ApplicationContext.userData, mission.canClearConditionList);
        var isCleared = ApplicationContext.userData.userMissionList
            .Where(u => u.missionId == mission.id)
            .Where(u => u.completedDate > DateTimeUtil.Epoch)
            .Where(u => (u.startExpirationDate <= DateTimeUtil.Epoch && u.endExpirationDate <= DateTimeUtil.Epoch) || (u.startExpirationDate > DateTimeUtil.Epoch && u.endExpirationDate > DateTimeUtil.Epoch && u.startExpirationDate <= DateTimeUtil.Now && DateTimeUtil.Now < u.endExpirationDate))
            .Any();

        scrollItem.SetNameText(mission.name);
        scrollItem.ShowGrayoutPanel(!canClear);
        scrollItem.ShowClearedPanel(isCleared);
        scrollItem.SetIcon(firstRewardItem);
        scrollItem.SetOnClickAction(() =>
        {
            ApiConnection.ClearMission(mission.id)
                .SelectMany(res => CommonReceiveDialogFactory.Create(new CommonReceiveDialogRequest() { itemList = res.rewardItemList }))
                .Do(_ =>
                {
                    ActivateBadge();
                    RefreshScroll();
                })
                .Subscribe();
        });
    }

    private MissionType GetMissionTypeFromTabValue(int tabValue)
    {
        switch (tabValue)
        {
            case DAILY_MISSION_TAB_VALUE:
                return MissionType.Daily;
            case MAIN_MISSION_TAB_VALUE:
                return MissionType.Main;
            case EVENT_MISSION_TAB_VALUE:
                return MissionType.Event;
            default:
                return MissionType.Main;
        }
    }

    private void ActivateBadge()
    {
        // ミッションタイプに関わらずクリア可能かつ未クリアのミッションリストを取得
        var canClearMissionList = MasterRecord.GetMasterOf<MissionMB>().GetAll()
            .Where(m =>
            {
                // 表示可能なものに絞る
                return ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList);
            })
            .Where(m =>
            {
                // クリア可能なものに絞る
                return ConditionUtil.IsValid(ApplicationContext.userData, m.canClearConditionList);
            })
            .Where(m =>
            {
                // 未クリアのものに絞る
                return !ApplicationContext.userData.userMissionList
                    .Where(u => u.missionId == m.id)
                    .Where(u => u.completedDate > DateTimeUtil.Epoch)
                    .Where(u => (u.startExpirationDate <= DateTimeUtil.Epoch && u.endExpirationDate <= DateTimeUtil.Epoch) || (u.startExpirationDate > DateTimeUtil.Epoch && u.endExpirationDate > DateTimeUtil.Epoch && u.startExpirationDate <= DateTimeUtil.Now && DateTimeUtil.Now < u.endExpirationDate))
                    .Any();
            })
            .ToList();
        var isShowDailyMissionBadge = canClearMissionList.Any(m => m.missionType == GetMissionTypeFromTabValue(DAILY_MISSION_TAB_VALUE));
        var isShowMainMissionBadge = canClearMissionList.Any(m => m.missionType == GetMissionTypeFromTabValue(MAIN_MISSION_TAB_VALUE));
        var isShowEventMissionBadge = canClearMissionList.Any(m => m.missionType == GetMissionTypeFromTabValue(EVENT_MISSION_TAB_VALUE));

        _dailyMissionBadge.SetActive(isShowDailyMissionBadge);
        _mainMissionBadge.SetActive(isShowMainMissionBadge);
        _eventMissionBadge.SetActive(isShowEventMissionBadge);
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
