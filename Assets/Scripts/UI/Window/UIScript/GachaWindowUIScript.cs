using System;
using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Gacha;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Gacha")]
public class GachaWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected Toggle _isNotPlayGachaAnimationToggle;

    private bool isPlayGachaAnimation;
    private List<GachaBoxMB> gachaBoxList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        isPlayGachaAnimation = SaveDataUtil.Setting.GetIsPlayGachaAnimation();
        _isNotPlayGachaAnimationToggle.isOn = !isPlayGachaAnimation;
        _isNotPlayGachaAnimationToggle.OnValueChangedIntentAsObservable()
            .Do(isNotPlayGachaAnimation =>
            {
                SaveDataUtil.Setting.SetIsPlayGachaAnimation(!isNotPlayGachaAnimation);
                isPlayGachaAnimation = !isNotPlayGachaAnimation;
            })
            .Subscribe();

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        gachaBoxList = MasterRecord.GetMasterOf<GachaBoxMB>().GetAll()
            .Where(gachaBox => IsOpened(gachaBox))
            .OrderBy(m => m.sortOrder)
            .ToList();

        _infiniteScroll.Init(gachaBoxList.Count, OnUpdateItem);
    }

    private bool IsOpened(GachaBoxMB gachaBox)
    {
        switch (gachaBox.openType)
        {
            case GachaOpenType.Schedule:
                // 対象のスケジュールが期間内かつ対応するガチャボックス詳細の中に一つでも表示条件を満たしているものがあれば表示する
                var gachaSchedule = MasterRecord.GetMasterOf<GachaScheduleMB>().GetAll().FirstOrDefault(m => m.gachaBoxId == gachaBox.id);
                if (gachaSchedule != null && DateTimeUtil.GetDateFromMasterString(gachaSchedule.startDate) <= DateTimeUtil.Now && DateTimeUtil.Now < DateTimeUtil.GetDateFromMasterString(gachaSchedule.endDate))
                {
                    return gachaBox.gachaBoxDetailIdList
                        .Select(id => MasterRecord.GetMasterOf<GachaBoxDetailMB>().Get(id))
                        .Any(gachaBoxDetail => ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.displayConditionList));
                }
                else
                {
                    return false;
                }
            case GachaOpenType.Condition:
                // ガチャボックスに対応するガチャボックス詳細マスタの中に一つでも表示条件を満たしているものがあれば表示する
                return gachaBox.gachaBoxDetailIdList
                    .Select(id => MasterRecord.GetMasterOf<GachaBoxDetailMB>().Get(id))
                    .Any(gachaBoxDetail => ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.displayConditionList));

            default:
                return false;
        }
    }

    private bool IsOpened(GachaBoxDetailMB gachaBoxDetail)
    {
        return ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.displayConditionList);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((gachaBoxList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<GachaBoxScrollItem>();
        var gachaBox = gachaBoxList[index];
        var gachaBoxDetailList = gachaBox.gachaBoxDetailIdList
            .Select(id => MasterRecord.GetMasterOf<GachaBoxDetailMB>().Get(id))
            .Where(m => ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList))
            .ToList();

        scrollItem.SetGachaBannerImage(gachaBox.id);
        scrollItem.SetLimitText(gachaBox);
        scrollItem.SetOnClickEmissionRateButtonAction(new Action(() =>
        {
            GachaEmissionRateDialogFactory.Create(new GachaEmissionRateDialogRequest()
            {
                gachaBoxId = gachaBox.id,
            }).Subscribe();
        }));
        scrollItem.RefreshScroll(gachaBox.pickUpMonsterIdList);

        // ガチャ実行ボタンの生成
        foreach (Transform n in scrollItem.executeButtonBase.transform) GameObject.Destroy(n.gameObject);
        gachaBoxDetailList.ForEach(gachaBoxDetail =>
        {
            var gachaExecuteButton = UIManager.Instance.CreateContent<GachaExecuteButton>(scrollItem.executeButtonBase);
            var title = gachaBoxDetail.title;
            var canExecute = ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.canExecuteConditionList);
            var cost = gachaBoxDetail.requiredItem.num;
            var requiredItem = gachaBoxDetail.requiredItem;
            var possessedRequiredItemNum = ClientItemUtil.GetPossessedNum(requiredItem.itemType, requiredItem.itemId);
            var enoughRequiredItem = possessedRequiredItemNum >= cost;

            gachaExecuteButton.ShowGrayoutPanel(!canExecute || !enoughRequiredItem);
            gachaExecuteButton.SetText(title);
            gachaExecuteButton.SetCostText(cost.ToString());
            gachaExecuteButton.SetCostIcon(requiredItem);
            gachaExecuteButton.SetOnClickAction(() => OnClickGachaExecuteButtonAction(gachaBox, gachaBoxDetail));
        });
    }

    private void OnClickGachaExecuteButtonAction(GachaBoxMB gachaBox, GachaBoxDetailMB gachaBoxDetail)
    {
        const float FADE_ANIMATION_TIME = 0.3f;

        var name = ClientItemUtil.GetName(gachaBoxDetail.requiredItem);
        var cost = gachaBoxDetail.requiredItem.num;
        var num = gachaBoxDetail.gachaExecuteType.Num();

        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            commonDialogType = CommonDialogType.NoAndYes,
            title = "確認",
            content = $"{name}を{cost}個使用してガチャを{num}回まわしますか？",
        })
            .Where(res => res.dialogResponseType == DialogResponseType.Yes)
            .SelectMany(_ =>
            {
                if (IsOpened(gachaBox) && IsOpened(gachaBoxDetail))
                {
                    return Observable.Return(true);
                }
                else
                {
                    var title = "確認";
                    var content = "ガチャ開催条件を満たしていません";
                    return CommonDialogFactory.Create(new CommonDialogRequest() { commonDialogType = CommonDialogType.YesOnly, title = title, content = content })
                        .Do(res => RefreshScroll())
                        .Select(res => false);
                }
            })
            .Where(isContinued => isContinued)
            .SelectMany(_ => ApiConnection.ExecuteGacha(gachaBox.id, gachaBoxDetail.id))
            .SelectMany(res =>
            {
                if (isPlayGachaAnimation)
                {
                    return FadeManager.Instance.PlayFadeAnimationObservable(1.0f, FADE_ANIMATION_TIME)
                        .SelectMany(_ =>
                        {
                            var gachaAnimation = UIManager.Instance.CreateContent<GachaAnimation>(UIManager.Instance.gachaAnimationParent);
                            return gachaAnimation.PlayGachaAnimationObservable();
                        })
                        .Do(_ => RefreshScroll())
                        .Do(_ => FadeManager.Instance.PlayFadeAnimationObservable(0.0f, FADE_ANIMATION_TIME).Subscribe())
                        .Select(_ => res);
                }
                else
                {
                    RefreshScroll();
                    return Observable.Return(res);
                }
            })
            .SelectMany(res =>
            {
                return GachaResultWindowFactory.Create(new GachaResultWindowRequest() { itemList = res.rewardItemList });
            })
            .Subscribe();
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