using DG.Tweening;
using GameBase;
using PM.Enum.Battle;
using PM.Enum.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected Text _turnText;
    [SerializeField] protected Text _waveText;
    [SerializeField] protected Text _skillNameText;
    [SerializeField] protected Text _actionDescriptionTitleText;
    [SerializeField] protected Text _actionDescriptionContentText;
    [SerializeField] protected List<BattleMonsterBase> _playerMonsterBaseList;
    [SerializeField] protected List<BattleMonsterBase> _enemyMonsterBaseList;
    [SerializeField] protected Image _fxParentImage;
    [SerializeField] protected Image _skillCutInMonsterIconImage;
    [SerializeField] protected Transform _fxParent;
    [SerializeField] protected Transform _battleMonsterInfoItemBase;
    [SerializeField] protected GameObject _skillNameBase;
    [SerializeField] protected GameObject _actionDescriptionBase;
    [SerializeField] protected GameObject _playerWholeSkillEffectBase;
    [SerializeField] protected GameObject _enemyWholeSkillEffectBase;
    [SerializeField] protected GameObject _skillCutInBase;
    [SerializeField] protected GameObject _headerBase;
    [SerializeField] protected GameObject _bossWaveEffectBase;
    [SerializeField] protected Transform _topWaveBase;
    [SerializeField] protected Transform _bottomWaveBase;
    [SerializeField] protected CanvasGroup _bossWaveBaseCanvasGroup;
    [SerializeField] protected CanvasGroup _bossWaveTitleCanvasGroup;
    [SerializeField] protected SkillFxItem _skillFxItem;
    [SerializeField] protected Sprite _playerSkillCutInSprite;
    [SerializeField] protected Sprite _enemySkillCutInSprite;
    [SerializeField] protected Button _pauseButton;

    private List<BattleMonsterInfo> playerBattleMonsterList;
    private List<BattleMonsterInfo> enemyBattleMonsterList;
    private UserMonsterPartyInfo userMonsterParty;
    private QuestMB quest;
    private IDisposable skillNameObservable;
    private IDisposable actionDescriptionObservable;

    public void Init(string userMonsterPartyId, long questId, string userBattleId)
    {
        userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        _pauseButton.OnClickIntentAsObservable()
            .Do(_ => TimeManager.Instance.Pause())
            .SelectMany(_ => BattlePauseDialogFactory.Create(new BattlePauseDialogRequest()))
            .SelectMany(res =>
            {
                switch (res.responseType)
                {
                    case BattlePauseDialogResponseType.Close:
                        return Observable.ReturnUnit();
                    case BattlePauseDialogResponseType.Interruption:
                        return ApiConnection.BattleInterruption(userBattleId)
                            .Do(_ => SaveDataUtil.Battle.ClearAllResumeSaveData())
                            .SelectMany(_ => BattleManager.Instance.FadeOutObservable());
                    case BattlePauseDialogResponseType.None:
                    default:
                        return Observable.ReturnUnit();
                }
            })
            .Do(_ => TimeManager.Instance.SpeedBy1())
            .Subscribe();

        SetPlayerMonsterImage();
        SetTurnText(1);
        SetWaveText(1, quest.questMonsterListByWave.Count);
        SetBattleMonsterInfoItem(userMonsterParty);
    }

    public void StartSkillTest()
    {
        quest = MasterRecord.GetMasterOf<QuestMB>().GetAll().First();
        SetEnemyMonsterImage(1);

        var targetMonsterBase = _enemyMonsterBaseList[1];
        var skillFxList = Enumerable.Range(1, 234).Select(id => new SkillFxMB()
        {
            id = id,
        }).ToList();
        skillFxList = MasterRecord.GetMasterOf<SkillFxMB>().GetAll().Where(m => m.id >= 1 && m.id <= 10).ToList();
        var animationObservableList = skillFxList.Shuffle().Select(skillEffect =>
        {
            const float SHOW_TIME = 2.0f;
            
            var skillNameObservable = Observable.ReturnUnit()
                .Do(_ =>
                {
                    _skillNameText.text = $"スキル演出ID{skillEffect.id}の演出";
                    _skillNameBase.SetActive(true);
                })
                .Delay(TimeSpan.FromSeconds(SHOW_TIME / 1.5f))
                .DoOnCompleted(() => _skillNameBase.SetActive(false));
            var actionDescriptionObservable = Observable.ReturnUnit()
                .Do(_ =>
                {
                    _actionDescriptionTitleText.text = $"スキル演出ID{skillEffect.id}";
                    _actionDescriptionContentText.text = skillEffect.description;
                    _actionDescriptionBase.SetActive(true);
                })
                .Delay(TimeSpan.FromSeconds(SHOW_TIME))
                .DoOnCompleted(() => _actionDescriptionBase.SetActive(false));
            var skillFxObservable = Observable.Timer(TimeSpan.FromSeconds(0.5f)).SelectMany(_ => VisualFxManager.Instance.PlaySkillFxObservable(skillEffect, targetMonsterBase.battleMonsterItem.effectBase,  _fxParentImage));

            return Observable.WhenAll(
                skillNameObservable,
                actionDescriptionObservable,
                skillFxObservable
            ).Delay(TimeSpan.FromSeconds(1.0f));
        }).ToList();

        Observable.ReturnUnit().Connect(animationObservableList).Subscribe();
    }

    private void SetPlayerMonsterImage()
    {
        userMonsterParty.userMonsterIdList.ForEach((userMonsterId, index) =>
        {
            var userMonster = ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            if (userMonster != null) {
                var parent = _playerMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);
                var battleMonsterIndex = new BattleMonsterIndex() { index = index, isPlayer = true };

                parent.SetBattleMonsterItem(item);
                item.Init(userMonster.monsterId, userMonster.customData.level);
                item.SetOnClickAction(() =>
                {
                    var battleMonster = GetBattleMonster(battleMonsterIndex);
                    if (battleMonster == null) return;

                    Observable.ReturnUnit()
                        .Do(_ => TimeManager.Instance.Pause())
                        .SelectMany(_ => BattleConditionListDialogFactory.Create(new BattleConditionListDialogRequest() { battleMonster = battleMonster }))
                        .Do(_ => TimeManager.Instance.SpeedBy1())
                        .Subscribe();
                });
            }
        });
    }

    public void SetEnemyMonsterImage(int waveCount)
    {
        _enemyMonsterBaseList.ForEach(monsterBase =>
        {
            foreach (Transform child in monsterBase.transform)
            {
                Destroy(child.gameObject);
            }
        });

        var waveIndex = waveCount - 1;
        var questMonsterList = quest.questMonsterListByWave[waveIndex];

        questMonsterList.ForEach((questMonster, index) =>
        {
            if (questMonster.monsterId > 0)
            {
                var parent = _enemyMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);
                var battleMonsterIndex = new BattleMonsterIndex() { index = index, isPlayer = false };

                parent.SetBattleMonsterItem(item);
                item.Init(questMonster.monsterId, questMonster.level);
                item.SetOnClickAction(() =>
                {
                    var battleMonster = GetBattleMonster(battleMonsterIndex);
                    if (battleMonster == null) return;

                    Observable.ReturnUnit()
                        .Do(_ => TimeManager.Instance.Pause())
                        .SelectMany(_ => BattleConditionListDialogFactory.Create(new BattleConditionListDialogRequest() { battleMonster = battleMonster }))
                        .Do(_ => TimeManager.Instance.SpeedBy1())
                        .Subscribe();
                });
            }
        });
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex battleMonsterIndex)
    {
        var targetBattleMonsterList = battleMonsterIndex.isPlayer ? playerBattleMonsterList : enemyBattleMonsterList;
        return targetBattleMonsterList.FirstOrDefault(b => b.index.IsSame(battleMonsterIndex));
    }

    public void UpdateBattleMonster(BattleMonsterInfo battleMonster)
    {
        var targetBattleMonsterList = battleMonster.index.isPlayer ? playerBattleMonsterList : enemyBattleMonsterList;
        var index = targetBattleMonsterList.FindIndex(b => b.index.IsSame(battleMonster.index));
        targetBattleMonsterList[index] = battleMonster;
    }

    public void UpdateBattleMonster(List<BattleMonsterInfo> battleMonsterList)
    {
        battleMonsterList.ForEach(battleMonster => UpdateBattleMonster(battleMonster));
    }

    public void SetBattleMonsterList(List<BattleMonsterInfo> battleMonsterList)
    {
        var isPlayer = battleMonsterList.First().index.isPlayer;
        if (isPlayer)
        {
            playerBattleMonsterList = battleMonsterList;
        }
        else
        {
            enemyBattleMonsterList = battleMonsterList;
        }
    }

    private void SetBattleMonsterInfoItem(UserMonsterPartyInfo userMonsterParty)
    {
        userMonsterParty.userMonsterIdList.ForEach(userMonsterId =>
        {
            var userMonster = ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            var battleMonsterInfoItem = UIManager.Instance.CreateContent<BattleMonsterInfoItem>(_battleMonsterInfoItemBase);
            battleMonsterInfoItem.Set(userMonster);
            battleMonsterInfoItem.SetOnClickAction(() =>
            {
                Observable.ReturnUnit()
                    .Do(_ => TimeManager.Instance.Pause())
                    .SelectMany(_ => MonsterDetailDialogFactory.Create(new MonsterDetailDialogRequest()
                    {
                        userMonster = userMonster,
                        canStrength = false,
                    }))
                    .Do(_ => TimeManager.Instance.SpeedBy1())
                    .Subscribe();
            });
        });
    }

    public void SetTurnText(int turnCount)
    {
        _turnText.text = $"Turn {turnCount}";
    }

    public void SetWaveText(int waveCount, int maxWaveCount)
    {
        _waveText.text = $"Wave {waveCount}/{maxWaveCount}";
    }

    public IObservable<Unit> ShowSkillInfoObservable(BattleMonsterInfo doBattleMonster, BattleActionType actionType)
    {
        const float SHOW_TIME = 2.0f;

        var skillNameAndDescription = GetSkillNameAndDescription(doBattleMonster, actionType);

        if (skillNameObservable != null)
        {
            skillNameObservable.Dispose();
            skillNameObservable = null;
        }
        if (actionDescriptionObservable != null)
        {
            actionDescriptionObservable.Dispose();
            actionDescriptionObservable = null;
        }

        skillNameObservable = Observable.ReturnUnit()
            .Do(_ =>
            {
                _skillNameText.text = skillNameAndDescription.skillName;
                _skillNameBase.SetActive(true);
            })
            .Delay(TimeSpan.FromSeconds(SHOW_TIME/1.5f))
            .DoOnCompleted(() => _skillNameBase.SetActive(false))
            .Subscribe();

        actionDescriptionObservable = Observable.ReturnUnit()
            .Do(_ =>
            {
                var actionName = GetActionName(actionType);
                _actionDescriptionTitleText.text = $"{actionName}の効果";
                _actionDescriptionContentText.text = $"{skillNameAndDescription.skillName}が発動！\n<color=\"yellow\">{skillNameAndDescription.skillDescription}</color>";
                _actionDescriptionBase.SetActive(true);
            })
            .Delay(TimeSpan.FromSeconds(SHOW_TIME))
            .DoOnCompleted(() => _actionDescriptionBase.SetActive(false))
            .Subscribe();

        // スキル情報表示演出は終了を待たない
        return Observable.ReturnUnit();
    }

    public IObservable<Unit> PlaySkillCutInAnimationObservable(long monsterId, bool isPlayer)
    {
        const float CUT_IN_ANIMATION_TIME = 2.0f;
        const float MONSTER_ICON_OFFSET_X = 50.0f;

        return PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .SelectMany(sprite =>
            {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.level1UltimateSkillId);
                var monsterIconDefaultPosition = _skillCutInMonsterIconImage.transform.localPosition;
                var skillFxItemRotation = isPlayer ? Vector3.zero : Vector3.up * 180.0f;

                // モンスターアイコンの設定
                _skillCutInMonsterIconImage.sprite = sprite;
                _skillCutInMonsterIconImage.transform.localPosition = new Vector3(monsterIconDefaultPosition.x - MONSTER_ICON_OFFSET_X / 2, monsterIconDefaultPosition.y, monsterIconDefaultPosition.z);

                // SkillFxItem
                _skillFxItem.renderer.material.mainTexture = isPlayer ? _playerSkillCutInSprite.texture : _enemySkillCutInSprite.texture;

                // カットインベースの設定
                _skillCutInBase.transform.localRotation = Quaternion.Euler(skillFxItemRotation);
                _skillCutInBase.SetActive(true);

                var monsterIconSequence = DOTween.Sequence()
                    .Append(_skillCutInMonsterIconImage.transform.DOLocalMoveX(monsterIconDefaultPosition.x + MONSTER_ICON_OFFSET_X / 2, CUT_IN_ANIMATION_TIME));
                return monsterIconSequence.OnCompleteAsObservable()
                    .AsUnitObservable()
                    .Do(_ =>
                    {
                        _skillCutInBase.SetActive(false);
                        _skillCutInMonsterIconImage.transform.localPosition = monsterIconDefaultPosition;
                    });
            });
    }

    public IObservable<Unit> PlayAttackAnimationObservable(BattleMonsterInfo doBattleMonster, BattleActionType actionType)
    {
        var isPlayer = doBattleMonster.index.isPlayer;
        var doMonsterBase = GetBattleMonsterBase(doBattleMonster.index);
        var doMonsterRT = doMonsterBase.battleMonsterItem.GetComponent<RectTransform>();
        return Observable.ReturnUnit()
            .SelectMany(_ =>
            {
                if (actionType == BattleActionType.UltimateSkill)
                {
                    // スキルカットイン
                    return PlaySkillCutInAnimationObservable(doBattleMonster.monsterId, doBattleMonster.index.isPlayer);
                }
                else
                {
                    return Observable.ReturnUnit();
                }
            })
            .SelectMany(_ => VisualFxManager.Instance.PlayStartAttackFxObservable(doMonsterRT, isPlayer));
    }

    public IObservable<Unit> PlayActionFailedAnimationObservable(BattleMonsterIndex doBattleMonsterIndex)
    {
        var monsterBase = GetBattleMonsterBase(doBattleMonsterIndex);
        return VisualFxManager.Instance.PlayActionFailedAnimationObservable(monsterBase.battleMonsterItem);
    }

    public IObservable<Unit> PlayTakeDamageAnimationObservable(BeDoneBattleMonsterData targetBeDoneBattleMonsterData,long skillFxId, int currentHp, int currentEnergy, int currentShield)
    {
        var monsterBase = GetBattleMonsterBase(targetBeDoneBattleMonsterData.battleMonsterIndex);
        var wholeSkillEffectBase = targetBeDoneBattleMonsterData.battleMonsterIndex.isPlayer ? _playerWholeSkillEffectBase : _enemyWholeSkillEffectBase;
        return VisualFxManager.Instance.PlayTakeDamageFxObservable(targetBeDoneBattleMonsterData, monsterBase.battleMonsterItem,skillFxId, Math.Max(0, currentHp), currentEnergy, currentShield, monsterBase.battleMonsterItem.effectBase, _fxParentImage);
    }

    public IObservable<Unit> PlayEnergySliderAnimationObservable(BattleMonsterIndex monsterIndex, int currentEnergy)
    {
        var monsterBase = GetBattleMonsterBase(monsterIndex);
        var energySlider = monsterBase.battleMonsterItem.energySlider;
        return VisualFxManager.Instance.PlayEnergySliderAnimationObservable(energySlider, currentEnergy);
    }

    public IObservable<Unit> PlayDieAnimationObservable(BattleMonsterIndex battleMonsterIndex)
    {
        var monsterBase = GetBattleMonsterBase(battleMonsterIndex);
        return VisualFxManager.Instance.PlayDieAnimationObservable(monsterBase.battleMonsterItem);
    }
    
    public IObservable<Unit> PlayWaveTitleFxObservable(int currentWaveCount, int maxWaveCount){
        SetEnemyMonsterImage(currentWaveCount);
        SetWaveText(currentWaveCount, maxWaveCount);
        return VisualFxManager.Instance.PlayWaveTitleFxObservable(_fxParent, currentWaveCount, maxWaveCount);
    }

    public IObservable<Unit> PlayTurnFxObservable(int currentTurn)
    {
        SetTurnText(currentTurn);
        return Observable.ReturnUnit();
    }

    public IObservable<Unit> PlayWinAnimationObservable()
    {
        return VisualFxManager.Instance.PlayWinBattleFxObservable(_fxParent).Delay(TimeSpan.FromSeconds(1));
    }

    public IObservable<Unit> PlayLoseAnimationObservable()
    {
        return VisualFxManager.Instance.PlayLoseBattleFxObservable(_fxParent).Delay(TimeSpan.FromSeconds(1));
    }

    public IObservable<Unit> PlayBossWaveAnimationObservable()
    {
        const float BOSS_WAVE_BASE_ANIMATION_TIME = 4.0f;
        const float BOSS_WAVE_BASE_FADE_ANIMATION_TIME = 0.15f;
        const float BOSS_WAVE_BASE_FADE_OUT_DELAY_TIME = BOSS_WAVE_BASE_ANIMATION_TIME - (BOSS_WAVE_BASE_FADE_ANIMATION_TIME * 2);
        const float WARNING_TEXT_ANIMATION_DISTANCE = 500.0f;
        const float WARNING_TEXT_ANIMATION_TIME = BOSS_WAVE_BASE_ANIMATION_TIME;
        const float TITLE_TEXT_ANIMATION_DELAY_TIME = 0.25f;
        const float TITLE_TEXT_FADE_ANIMATION_TOTAL_TIME = BOSS_WAVE_BASE_ANIMATION_TIME - TITLE_TEXT_ANIMATION_DELAY_TIME;
        const int TITLE_TEXT_FADE_COUNT = 3;
        const float TITLE_TEXT_FADE_ANIMATION_TIME = (TITLE_TEXT_FADE_ANIMATION_TOTAL_TIME / TITLE_TEXT_FADE_COUNT) / 2;

        Debug.Log($"delay:{TITLE_TEXT_ANIMATION_DELAY_TIME}, fade:{TITLE_TEXT_FADE_ANIMATION_TIME}");

        return Observable.ReturnUnit()
            .Do(_ =>
            {
                // 準備
                _bossWaveEffectBase.SetActive(true);
                _headerBase.SetActive(false);
                _topWaveBase.DOLocalMoveX(0.0f, 0.0f);
                _bottomWaveBase.DOLocalMoveX(0.0f, 0.0f);
                _bossWaveTitleCanvasGroup.DOFade(0.0f, 0.0f);
                _bossWaveBaseCanvasGroup.DOFade(0.0f, 0.0f);
            })
            .SelectMany(_ =>
            {
                var bossWaveBaseAnimationSequence = DOTween.Sequence()
                    .Append(_bossWaveBaseCanvasGroup.DOFade(1.0f, BOSS_WAVE_BASE_FADE_ANIMATION_TIME))
                    .AppendInterval(BOSS_WAVE_BASE_FADE_OUT_DELAY_TIME)
                    .Append(_bossWaveBaseCanvasGroup.DOFade(0.0f, BOSS_WAVE_BASE_FADE_ANIMATION_TIME));

                var topWarningTextAnimationSequence = DOTween.Sequence()
                    .Append(_topWaveBase.DOLocalMoveX(-WARNING_TEXT_ANIMATION_DISTANCE, WARNING_TEXT_ANIMATION_TIME).SetEase(Ease.Linear));

                var bottomWarningTextAnimationSequence = DOTween.Sequence()
                    .Append(_bottomWaveBase.DOLocalMoveX(WARNING_TEXT_ANIMATION_DISTANCE, WARNING_TEXT_ANIMATION_TIME).SetEase(Ease.Linear));

                var titleTextAnimationSequence = DOTween.Sequence()
                    .AppendInterval(TITLE_TEXT_ANIMATION_DELAY_TIME)
                    .Append(_bossWaveTitleCanvasGroup.DOFade(1.0f, TITLE_TEXT_FADE_ANIMATION_TIME))
                    .Append(_bossWaveTitleCanvasGroup.DOFade(0.0f, TITLE_TEXT_FADE_ANIMATION_TIME))
                    .Append(_bossWaveTitleCanvasGroup.DOFade(1.0f, TITLE_TEXT_FADE_ANIMATION_TIME))
                    .Append(_bossWaveTitleCanvasGroup.DOFade(0.0f, TITLE_TEXT_FADE_ANIMATION_TIME))
                    .Append(_bossWaveTitleCanvasGroup.DOFade(1.0f, TITLE_TEXT_FADE_ANIMATION_TIME))
                    .Append(_bossWaveTitleCanvasGroup.DOFade(0.0f, TITLE_TEXT_FADE_ANIMATION_TIME));

                return Observable.WhenAll(
                    bossWaveBaseAnimationSequence.PlayAsObservable().AsUnitObservable(),
                    topWarningTextAnimationSequence.PlayAsObservable().AsUnitObservable(),
                    bottomWarningTextAnimationSequence.PlayAsObservable().AsUnitObservable(),
                    titleTextAnimationSequence.PlayAsObservable().AsUnitObservable()
                );
            })
            .Do(_ =>
            {
                // 片付け
                _bossWaveEffectBase.SetActive(false);
                _headerBase.SetActive(true);
            });
    }

    public void RefreshBattleCondition(BattleMonsterInfo battleMonster)
    {
        var monsterBase = GetBattleMonsterBase(battleMonster.index);
        monsterBase.battleMonsterItem.RefreshBattleCondition(battleMonster.battleConditionList);
    }

    private string GetActionName(BattleActionType actionType)
    {
        switch (actionType) {
            case BattleActionType.PassiveSkill:
                return "パッシブスキル";
            case BattleActionType.NormalSkill:
                return "通常スキル";
            case BattleActionType.UltimateSkill:
                return "アルティメットスキル";
            case BattleActionType.None:
            default:
                return "";
        }
    }
    private (string skillName, string skillDescription) GetSkillNameAndDescription(BattleMonsterInfo battleMonster, BattleActionType actionType)
    {
        switch (actionType) {
            case BattleActionType.PassiveSkill:
                var passiveSkillId = ClientMonsterUtil.GetPassiveSkillId(battleMonster.monsterId, battleMonster.level);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(passiveSkillId);
                return (passiveSkill?.name ?? "-", passiveSkill?.description ?? "-");
            case BattleActionType.NormalSkill:
                var normalSkillId = ClientMonsterUtil.GetNormalSkillId(battleMonster.monsterId, battleMonster.level);
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(normalSkillId);
                return (normalSkill.name, normalSkill.description);
            case BattleActionType.UltimateSkill:
                var ultimateSkillId = ClientMonsterUtil.GetUltimateSkillId(battleMonster.monsterId, battleMonster.level);
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(ultimateSkillId);
                return (ultimateSkill.name, ultimateSkill.description);
            default:
                return ("", "");
        }
    }

    private BattleMonsterBase GetBattleMonsterBase(BattleMonsterIndex battleMonsterIndex)
    {
        var isPlayer = battleMonsterIndex.isPlayer;
        var monsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var monsterBase = monsterBaseList[battleMonsterIndex.index];
        return monsterBase;
    }
}
