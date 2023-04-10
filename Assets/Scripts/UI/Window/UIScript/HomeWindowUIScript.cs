using System;
using System.Collections.Generic;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using PM.Enum.Item;
using PM.Enum.Date;
using UnityEngine.U2D;

[ResourcePath("UI/Window/Window-Home")]
public class HomeWindowUIScript : WindowBase
{
    [SerializeField] protected Button _questButton;
    [SerializeField] protected Button _missionButton;
    [SerializeField] protected Button _presentButton;
    [SerializeField] protected RewardAdButton _rewardAdButton;
    [SerializeField] protected Transform _monsterAreaParentBase;
    [SerializeField] protected GameObject _missionIconBadge;
    [SerializeField] protected GameObject _presentIconBadge;
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected SpriteAtlas _monsterSpriteAtlas;

    private List<Transform> monsterAreaParentList = new List<Transform>();

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _questButton.OnClickIntentAsObservable()
            .SelectMany(_ => QuestCategoryWindowFactory.Create(new QuestCategoryWindowRequest()))
            .Subscribe();

        _missionButton.OnClickIntentAsObservable()
            .SelectMany(_ => MissionDialogFactory.Create(new MissionDialogRequest()))
            .Subscribe();

        _presentButton.OnClickIntentAsObservable()
            .SelectMany(_ => PresentDialogFactory.Create(new PresentDialogRequest()))
            .Subscribe();

        _rewardAdButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                var title = "確認";
                var content = $"広告を視聴することで限定アイテムを獲得します\n{GetRewardAdRemainNumText(_rewardAdButton.rewardAdId)}";
                return CommonDialogFactory.Create(new CommonDialogRequest() { commonDialogType = CommonDialogType.NoAndYes, title = title, content = content });
            })
            .Where(res => res.dialogResponseType == DialogResponseType.Yes)
            .SelectMany(_ => MobileAdsManager.Instance.ShowRewardObservable())
            .SelectMany(_ => ApiConnection.RewardAdGrantReward(_rewardAdButton.rewardAdId))
            .SelectMany(res =>
            {
                var itemList = MasterRecord.GetMasterOf<RewardAdMB>().Get(_rewardAdButton.rewardAdId).itemList.Select(i => (ItemMI)i).ToList();
                return RouletteDialogFactory.Create(new RouletteDialogRequest()
                {
                    itemList = itemList,
                    electedIndex = res.electedIndex,
                });
            })
            .Do(_ => RefreshRewardAdButton())
            .Subscribe();

        RefreshRewardAdButton();
        ActivateBadge();
        SetMonsterImage();

        // 実行中のバトルがあればそちらを再開
        var resumeQuestId = SaveDataUtil.Battle.GetResumeQuestId();
        var resumeUserMonsterPartyId = SaveDataUtil.Battle.GetResumeUserMonsterPartyId();
        var resumeUserBattleId = SaveDataUtil.Battle.GetResumeUserBattleId();
        var resumeBattleLogList = SaveDataUtil.Battle.GetResumeBattleLogList();
        if (resumeQuestId > 0 && resumeUserMonsterPartyId != string.Empty && resumeUserBattleId != string.Empty && resumeBattleLogList.Any())
        {
            CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.NoAndYes,
                title = "確認",
                content = "実行中のバトルがあります\n再開しますか？",
            })
                .SelectMany(res =>
                {
                    if (res.dialogResponseType == DialogResponseType.No)
                    {
                        SaveDataUtil.Battle.ClearAllResumeSaveData();
                        return ApiConnection.BattleInterruption(resumeUserBattleId).AsUnitObservable();
                    }
                    else
                    {
                        return BattleManager.Instance.ResumeBattleObservable(resumeQuestId, resumeUserMonsterPartyId, resumeUserBattleId, resumeBattleLogList);
                    }
                })
                .Do(_ => MainSceneManager.Instance.SetIsReadyToShowStackableDialog(true))
                .Subscribe();
        }
        else
        {
            MainSceneManager.Instance.SetIsReadyToShowStackableDialog(true);
        }

        SetMonster();
    }

    private void SetMonster()
    {
        var spriteList = new List<Sprite>();
        for (var i = 0; i < _monsterSpriteAtlas.spriteCount; i++)
        {
            var spriteName = $"boss_antiswarm_{i}";
            spriteList.Add(_monsterSpriteAtlas.GetSprite(spriteName));
        }

        var attackIndexList = new List<int>() { 80, 47, 39, 31, 23, 15, 7, 111, 104, 97, 90, 83, 76, 69, 62, 54, 46, 38, 30, 22, 14, 6, 110, 103, 96, 89, 82, 75, 68, 61, 53, 45, 37, 29, 21, 13, 5, 109, 102, 95, 88, 81, 74 };
        var idleIndex = 71;

        var frameSeconds = 0.1f;
        Observable.Interval(TimeSpan.FromSeconds(5.0f))
            .SelectMany(_ =>
            {
                return Observable.Interval(TimeSpan.FromSeconds(frameSeconds))
                    .Do(index =>
                    {
                        Debug.Log(index);
                        _monsterImage.sprite = spriteList[attackIndexList[(int)index]];
                    })
                    .Take(attackIndexList.Count)
                    .Buffer(attackIndexList.Count);
            })
            .Do(_ => _monsterImage.sprite = spriteList[idleIndex])
            .Subscribe();
    }

    private void SetMonsterImage()
    {
        var userMonsterParty = ApplicationContext.userData.userMonsterPartyList?.FirstOrDefault();
        if (userMonsterParty != null)
        {
            monsterAreaParentList.Clear();
            foreach (Transform child in _monsterAreaParentBase)
            {
                monsterAreaParentList.Add(child);
            }
            monsterAreaParentList = monsterAreaParentList.Shuffle().ToList();

            userMonsterParty.userMonsterIdList.ForEach((userMonsterId, index) =>
            {
                var parent = monsterAreaParentList[index];
                var userMonster = ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
                if (userMonster != null)
                {
                    var homeMonsterItem = UIManager.Instance.CreateContent<HomeMonsterItem>(parent);
                    var rectTransform = homeMonsterItem.GetComponent<RectTransform>();
                    homeMonsterItem.SetMonsterImage(userMonster.monsterId);
                }
            });
        }
    }

    private string GetRewardAdRemainNumText(long rewardAdId)
    {
        var rewardAd = MasterRecord.GetMasterOf<RewardAdMB>().Get(rewardAdId);
        var grantedNum = DateTimeUtil.GetTermValidUserRewardAdList(rewardAd.termType, ApplicationContext.userData.userRewardAdList).Count;
        var termText = "";
        switch (rewardAd.termType)
        {
            case TermType.Day:
                termText = "本日の残り回数";
                break;
        }

        return $"{termText}:{rewardAd.limitNum - grantedNum}/{rewardAd.limitNum}";
    }

    private void RefreshRewardAdButton()
    {
        var rewardAd = MasterRecord.GetMasterOf<RewardAdMB>().Get(_rewardAdButton.rewardAdId);
        var isShowRewardAdButton = DateTimeUtil.GetTermValidUserRewardAdList(rewardAd.termType, ApplicationContext.userData.userRewardAdList).Where(u => u.rewardAdId == rewardAd.id).Count() < rewardAd.limitNum;
        _rewardAdButton.gameObject.SetActive(isShowRewardAdButton);
    }

    private void ActivateBadge()
    {
        var isShowPresentIconBadge = ApplicationContext.userData.userPresentList.Any(u => u.IsValid());
        var isShowMissionIconBadge = MasterRecord.GetMasterOf<MissionMB>().GetAll()
            .Where(m =>
            {
                // 表示条件を満たしているミッションに絞る
                return ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList);
            })
            .Where(m =>
            {
                // クリア条件を満たしているか否か
                var canClear = ConditionUtil.IsValid(ApplicationContext.userData, m.canClearConditionList);
                // すでにクリアしているか否か
                var isCleared = ApplicationContext.userData.userMissionList
                    .Where(u => u.missionId == m.id)
                    .Where(u => u.completedDate > DateTimeUtil.Epoch)
                    .Where(u => (u.startExpirationDate <= DateTimeUtil.Epoch && u.endExpirationDate <= DateTimeUtil.Epoch) || (u.startExpirationDate > DateTimeUtil.Epoch && u.endExpirationDate > DateTimeUtil.Epoch && u.startExpirationDate <= DateTimeUtil.Now && DateTimeUtil.Now < u.endExpirationDate))
                    .Any();

                // クリア可能 && 未クリアならバッチを表示
                return canClear && !isCleared;
            })
            .Any();

        _presentIconBadge.SetActive(isShowPresentIconBadge);
        _missionIconBadge.SetActive(isShowMissionIconBadge);
    }

    public override void Open(WindowInfo info)
    {
        RefreshRewardAdButton();
        ActivateBadge();
        HeaderFooterManager.Instance.ActivateBadge();
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}