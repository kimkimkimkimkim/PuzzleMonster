using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using PM.Enum.Battle;

[ResourcePath("UI/Dialog/Dialog-MonsterDetail")]
public class MonsterDetailDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _levelUpButton;
    [SerializeField] protected Button _gradeUpButton;
    [SerializeField] protected Button _luckUpButton;
    [SerializeField] protected Text _nameText;
    [SerializeField] protected Text _normalSkillNameText;
    [SerializeField] protected Text _normalSkillDescriptionText;
    [SerializeField] protected Text _ultimateSkillNameText;
    [SerializeField] protected Text _ultimateSkillDescriptionText;
    [SerializeField] protected Text _passiveSkillNameText;
    [SerializeField] protected Text _passiveSkillDescriptionText;
    [SerializeField] protected Button _normalSkillDetailButton;
    [SerializeField] protected Button _ultimateSkillDetailButton;
    [SerializeField] protected Button _passiveSkillDetailButton;
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected Image _monsterAttributeImage;
    [SerializeField] protected Image _monsterRarityImage;
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _hpText;
    [SerializeField] protected Text _attackText;
    [SerializeField] protected Text _defenseText;
    [SerializeField] protected Text _luckText;
    [SerializeField] protected Text _speedText;
    [SerializeField] protected Slider _hpSliderBack;
    [SerializeField] protected Slider _hpSliderFront;
    [SerializeField] protected Slider _attackSliderBack;
    [SerializeField] protected Slider _attackSliderFront;
    [SerializeField] protected Slider _defenseSliderBack;
    [SerializeField] protected Slider _defenseSliderFront;
    [SerializeField] protected Slider _luckSliderBack;
    [SerializeField] protected Slider _luckSliderFront;
    [SerializeField] protected Slider _speedSliderBack;
    [SerializeField] protected Slider _speedSliderFront;
    [SerializeField] protected GameObject _passiveSkillBase;
    [SerializeField] protected GameObject _strengthButtonBase;

    private bool isNeedRefresh;
    private MonsterMB monster;
    private UserMonsterInfo userMonster;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action<bool>)info.param["onClickClose"];
        userMonster = (UserMonsterInfo)info.param["userMonster"];
        var canStrength = (bool)info.param["canStrength"];

        monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ =>
            {
                if (onClickClose != null)
                {
                    onClickClose(isNeedRefresh);
                    onClickClose = null;
                }
            })
            .Subscribe();

        _levelUpButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterLevelUpDialogFactory.Create(new MonsterLevelUpDialogRequest()
            {
                userMonster = userMonster,
            }))
            .Where(res => res.isNeedRefresh)
            .SelectMany(_ =>
            {
                isNeedRefresh = true;
                userMonster = ApplicationContext.userData.userMonsterList.First(u => u.id == userMonster.id);
                return RefreshUIObservable();
            })
            .Subscribe();

        _gradeUpButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterGradeUpDialogFactory.Create(new MonsterGradeUpDialogRequest()
            {
                userMonster = userMonster,
            }))
            .Where(res => res.isNeedRefresh)
            .SelectMany(_ =>
            {
                isNeedRefresh = true;
                userMonster = ApplicationContext.userData.userMonsterList.First(u => u.id == userMonster.id);
                return RefreshUIObservable();
            })
            .Subscribe();

        _luckUpButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterLuckUpDialogFactory.Create(new MonsterLuckUpDialogRequest()
            {
                userMonster = userMonster,
            }))
            .Where(res => res.isNeedRefresh)
            .SelectMany(_ =>
            {
                isNeedRefresh = true;
                userMonster = ApplicationContext.userData.userMonsterList.First(u => u.id == userMonster.id);
                return RefreshUIObservable();
            })
            .Subscribe();

        _normalSkillDetailButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterSkillDetailDialogFactory.Create(new MonsterSkillDetailDialogRequest()
            {
                userMonster = userMonster,
                battleActionType = BattleActionType.NormalSkill,
            }))
            .Subscribe();

        _ultimateSkillDetailButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterSkillDetailDialogFactory.Create(new MonsterSkillDetailDialogRequest()
            {
                userMonster = userMonster,
                battleActionType = BattleActionType.UltimateSkill,
            }))
            .Subscribe();

        _passiveSkillDetailButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterSkillDetailDialogFactory.Create(new MonsterSkillDetailDialogRequest()
            {
                userMonster = userMonster,
                battleActionType = BattleActionType.PassiveSkill,
            }))
            .Subscribe();

        _strengthButtonBase.SetActive(canStrength);
        SetSliderMaxValue();
        RefreshUIObservable().Subscribe();
    }

    private void SetSliderMaxValue()
    {
        _hpSliderBack.maxValue = ConstManager.Monster.MAX_HP_VALUE;
        _hpSliderFront.maxValue = ConstManager.Monster.MAX_HP_VALUE;
        _attackSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _attackSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _defenseSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _defenseSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _speedSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _speedSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_WITHOUT_HP_VALUE;
        _luckSliderBack.maxValue = 0; // TODO
        _luckSliderFront.maxValue = ClientMonsterUtil.GetMaxMonsterLevel(ConstManager.Monster.MAX_RARITY, ConstManager.Monster.MAX_GRADE);
    }

    private IObservable<Unit> RefreshUIObservable()
    {
        // 名前
        _nameText.text = monster.name;

        // グレード
        _monsterGradeParts.SetGradeImage(userMonster.customData.grade);

        // ステータス
        var status = MonsterUtil.GetMonsterStatus(monster, userMonster.customData.level);
        _levelText.text = GetLevelText(userMonster.customData.level);
        _hpText.text = status.hp.ToString();
        _attackText.text = status.attack.ToString();
        _defenseText.text = status.defense.ToString();
        _speedText.text = status.speed.ToString();
        _luckText.text = userMonster.customData.luck.ToString();

        // ゲージ
        var maxGrade = MasterRecord.GetMasterOf<GradeUpTableMB>().GetAll().Max(m => m.targetGrade);
        var maxLevel = MasterRecord.GetMasterOf<MaxMonsterLevelMB>().GetAll().First(m => m.monsterRarity == monster.rarity && m.monsterGrade == maxGrade).maxMonsterLevel;
        var maxLevelStatus = MonsterUtil.GetMonsterStatus(monster, maxLevel);
        _hpSliderBack.value = maxLevelStatus.hp;
        _hpSliderFront.value = status.hp;
        _attackSliderBack.value = maxLevelStatus.attack;
        _attackSliderFront.value = status.attack;
        _defenseSliderBack.value = maxLevelStatus.defense;
        _defenseSliderFront.value = status.defense;
        _speedSliderBack.value = maxLevelStatus.speed;
        _speedSliderFront.value = status.speed;
        _luckSliderBack.value = ClientMonsterUtil.GetMaxMonsterLevel(monster.rarity, ConstManager.Monster.MAX_GRADE);
        _luckSliderFront.value = userMonster.customData.luck;

        // スキル
        SetSkillText();

        // モンスター画像、属性画像の設定
        return Observable.WhenAll(
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monster.id).Do(sprite => _monsterImage.sprite = sprite),
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterAttribute, (int)monster.attribute).Do(sprite => _monsterAttributeImage.sprite = sprite),
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterRarity, (int)monster.rarity).Do(sprite => _monsterRarityImage.sprite = sprite)
        ).AsUnitObservable();
    }

    private void SetSkillText()
    {
        var normalSkillId = ClientMonsterUtil.GetNormalSkillId(monster.id, userMonster.customData.level);
        var ultimateSkillId = ClientMonsterUtil.GetUltimateSkillId(monster.id, userMonster.customData.level);
        var passiveSkillId = ClientMonsterUtil.GetPassiveSkillId(monster.id, userMonster.customData.level);
        var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(normalSkillId);
        var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(ultimateSkillId);
        var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(passiveSkillId);

        _normalSkillNameText.text = normalSkill.name;
        _normalSkillDescriptionText.SetOmmitedText(normalSkill.description);
        _ultimateSkillNameText.text = ultimateSkill.name;
        _ultimateSkillDescriptionText.SetOmmitedText(ultimateSkill.description);
        _passiveSkillNameText.text = passiveSkill?.name ?? "-";
        _passiveSkillDescriptionText.SetOmmitedText(passiveSkill?.description ?? "-");
    }

    /// <summary>
    /// レベルテキストで指定する文字列を取得する
    /// </summary>
    private string GetLevelText(int level)
    {
        return $"Lv <size=64>{level.ToString()}</size>";
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