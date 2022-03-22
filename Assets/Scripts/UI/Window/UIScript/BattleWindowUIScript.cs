using DG.Tweening;
using GameBase;
using PM.Enum.Battle;
using PM.Enum.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected TextMeshProUGUI _turnText;
    [SerializeField] protected TextMeshProUGUI _waveText;
    [SerializeField] protected TextMeshProUGUI _skillNameText;
    [SerializeField] protected TextMeshProUGUI _actionDescriptionTitleText;
    [SerializeField] protected TextMeshProUGUI _actionDescriptionContentText;
    [SerializeField] protected TextMeshProUGUI _skillCutInSkillNameText;
    [SerializeField] protected TextMeshProUGUI _skillCutInSkillNameTitleText;
    [SerializeField] protected List<BattleMonsterBase> _playerMonsterBaseList;
    [SerializeField] protected List<BattleMonsterBase> _enemyMonsterBaseList;
    [SerializeField] protected Image _fxParentImage;
    [SerializeField] protected Image _skillCutInMonsterBaseImage;
    [SerializeField] protected Image _skillCutInMonsterIconImage;
    [SerializeField] protected Transform _fxParent;
    [SerializeField] protected Transform _battleMonsterInfoItemBase;
    [SerializeField] protected GameObject _skillNameBase;
    [SerializeField] protected GameObject _actionDescriptionBase;
    [SerializeField] protected GameObject _playerWholeSkillEffectBase;
    [SerializeField] protected GameObject _enemyWholeSkillEffectBase;
    [SerializeField] protected GameObject _skillCutInSkillNameBase;
    [SerializeField] protected GameObject _skillCutInBase;

    [SerializeField] protected Button _pauseButton;

    private UserMonsterPartyInfo userMonsterParty;
    private QuestMB quest;
    private IDisposable skillNameObservable;
    private IDisposable actionDescriptionObservable;

    public void Init(string userMonsterPartyId, long questId)
    {
        userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        _pauseButton.OnClickIntentAsObservable()
            .Do(_ => TimeManager.Instance.Pause())
            .SelectMany(_ => BattlePauseDialogFactory.Create(new BattlePauseDialogRequest()))
            .Do(_ => TimeManager.Instance.SpeedBy1())
            .Subscribe();

        SetPlayerMonsterImage();
        SetTurnText(1);
        SetWaveText(1, quest.questWaveIdList.Count);
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
            var skillFxObservable = Observable.Timer(TimeSpan.FromSeconds(0.5f)).SelectMany(_ => VisualFxManager.Instance.PlaySkillFxObservable(skillEffect, targetMonsterBase.battleMonsterItem.effectBase, _enemyWholeSkillEffectBase, _fxParentImage));

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
            var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            if (userMonster != null) {
                var parent = _playerMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);

                parent.SetBattleMonsterItem(item);
                item.Init(userMonster.monsterId, userMonster.customData.level);
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
        var questWaveId = quest.questWaveIdList[waveIndex];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);

        questWave.questMonsterIdList.ForEach((questMonsterId, index) =>
        {
            var questMonster = MasterRecord.GetMasterOf<QuestMonsterMB>().GetAll().FirstOrDefault(m => m.id == questMonsterId);
            if(questMonster != null)
            {
                var parent = _enemyMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);

                parent.SetBattleMonsterItem(item);
                item.Init(questMonster.monsterId, questMonster.level);
            }
        });
    }

    private void SetBattleMonsterInfoItem(UserMonsterPartyInfo userMonsterParty)
    {
        userMonsterParty.userMonsterIdList.ForEach(userMonsterId =>
        {
            var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            var battleMonsterInfoItem = UIManager.Instance.CreateContent<BattleMonsterInfoItem>(_battleMonsterInfoItemBase);
            battleMonsterInfoItem.Set(userMonster);
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
                _actionDescriptionContentText.text = $"{skillNameAndDescription.skillName}が発動！\n<color=\"yellow\">{skillNameAndDescription.skillDescription}";
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
        const float SKILL_NAME_TEXT_ANGLE = -12.23f;
        const float SKILL_NAME_OFFSET = 2000.0f;
        const float MOVE_ANIMATION_TIME = 0.2f;
        const float STOP_TIME = 1.5f;
        const float MONSTER_ICON_OFFSET_X = 50.0f;

        return PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .SelectMany(sprite =>
            {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.ultimateSkillId);
                var monsterIconDefaultPosition = _skillCutInMonsterIconImage.transform.localPosition;
                var skillNameBaseDefaultPosition = _skillCutInSkillNameBase.transform.localPosition;
                var skillNameBaseFromPosition = new Vector3(skillNameBaseDefaultPosition.x + SKILL_NAME_OFFSET, skillNameBaseDefaultPosition.y + (float)Math.Sin(Math.PI * SKILL_NAME_TEXT_ANGLE / 180.0), 0.0f);
                var skillNameBaseToPosition = new Vector3(skillNameBaseDefaultPosition.x - SKILL_NAME_OFFSET, skillNameBaseDefaultPosition.y - (float)Math.Sin(Math.PI * SKILL_NAME_TEXT_ANGLE / 180.0), 0.0f);
                var skillCutInBaseRotation = isPlayer ? Vector3.zero : Vector3.up * 180.0f;
                var skillNameTextRotation = isPlayer ? new Vector3(0.0f, 0.0f, SKILL_NAME_TEXT_ANGLE) : new Vector3(0.0f, 180.0f, -SKILL_NAME_TEXT_ANGLE);
                var skillNameTitleTextAlignment = isPlayer ? TextAlignmentOptions.BottomLeft : TextAlignmentOptions.BottomRight;

                // モンスターアイコンの設定
                _skillCutInMonsterIconImage.sprite = sprite;
                _skillCutInMonsterIconImage.transform.localPosition = new Vector3(monsterIconDefaultPosition.x - MONSTER_ICON_OFFSET_X / 2, monsterIconDefaultPosition.y, monsterIconDefaultPosition.z);
                
                // スキルタイトルテキストの設定
                _skillCutInSkillNameTitleText.transform.localRotation = Quaternion.Euler(skillNameTextRotation);
                _skillCutInSkillNameTitleText.alignment = skillNameTitleTextAlignment;

                // スキル名テキストの設定
                _skillCutInSkillNameText.transform.localRotation = Quaternion.Euler(skillNameTextRotation);
                _skillCutInSkillNameText.text = ultimateSkill.name;

                // スキルベースの設定
                _skillCutInSkillNameBase.transform.localPosition = skillNameBaseFromPosition;

                // モンスターベースの設定
                _skillCutInMonsterBaseImage.fillOrigin = 0;
                _skillCutInMonsterBaseImage.fillAmount = 0.0f;

                // カットインベースの設定
                _skillCutInBase.transform.localRotation = Quaternion.Euler(skillCutInBaseRotation);
                _skillCutInBase.SetActive(true);

                var sequence = DOTween.Sequence()
                    .Append(DOVirtual.Float(0.0f, 1.0f, MOVE_ANIMATION_TIME, value => _skillCutInMonsterBaseImage.fillAmount = value))
                    .Join(_skillCutInSkillNameBase.transform.DOLocalMove(skillNameBaseDefaultPosition, MOVE_ANIMATION_TIME))
                    .AppendInterval(STOP_TIME)
                    .AppendCallback(() => _skillCutInMonsterBaseImage.fillOrigin = 1)
                    .Append(DOVirtual.Float(1.0f, 0.0f, MOVE_ANIMATION_TIME, value => _skillCutInMonsterBaseImage.fillAmount = value))
                    .Join(_skillCutInSkillNameBase.transform.DOLocalMove(skillNameBaseToPosition, MOVE_ANIMATION_TIME))
                    .AppendCallback(() =>
                    {
                        _skillCutInBase.SetActive(false);
                        _skillCutInSkillNameBase.transform.localPosition = skillNameBaseDefaultPosition;
                        _skillCutInMonsterIconImage.transform.localPosition = monsterIconDefaultPosition;
                    });
                var monsterIconSequence = DOTween.Sequence()
                    .Append(_skillCutInMonsterIconImage.transform.DOLocalMoveX(MONSTER_ICON_OFFSET_X / 2, STOP_TIME + MOVE_ANIMATION_TIME * 2));
                return Observable.WhenAll(
                    sequence.OnCompleteAsObservable().AsUnitObservable(),
                    monsterIconSequence.OnCompleteAsObservable().AsUnitObservable()
                );
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
        return VisualFxManager.Instance.PlayTakeDamageFxObservable(targetBeDoneBattleMonsterData, monsterBase.battleMonsterItem,skillFxId, Math.Max(0, currentHp), currentEnergy, currentShield, wholeSkillEffectBase, _fxParentImage);
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
        var monster = monsterBase.battleMonsterItem.gameObject;
        monster.SetActive(false);
        return Observable.ReturnUnit();
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
        return VisualFxManager.Instance.PlayLoseBattleFxObservable(_fxParent).Delay(TimeSpan.FromSeconds(1)); ;
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
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        switch (actionType) {
            case BattleActionType.PassiveSkill:
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                return (passiveSkill.name, passiveSkill.description);
            case BattleActionType.NormalSkill:
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.normalSkillId);
                return (normalSkill.name, normalSkill.description);
            case BattleActionType.UltimateSkill:
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.ultimateSkillId);
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
