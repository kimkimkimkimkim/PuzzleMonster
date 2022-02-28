using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;

[ResourcePath("UI/Dialog/Dialog-MonsterDetail")]
public class MonsterDetailDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _levelUpButton;
    [SerializeField] protected Text _nameText;
    [SerializeField] protected Text _normalSkillNameText;
    [SerializeField] protected Text _normalSkillDescriptionText;
    [SerializeField] protected Text _ultimateSkillNameText;
    [SerializeField] protected Text _ultimateSkillDescriptionText;
    [SerializeField] protected Text _passiveSkillNameText;
    [SerializeField] protected Text _passiveSkillDescriptionText;
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected Image _monsterAttributeImage;
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _hpText;
    [SerializeField] protected Text _attackText;
    [SerializeField] protected Text _defenseText;
    [SerializeField] protected Text _speedText;
    [SerializeField] protected Slider _hpSliderBack;
    [SerializeField] protected Slider _hpSliderFront;
    [SerializeField] protected Slider _attackSliderBack;
    [SerializeField] protected Slider _attackSliderFront;
    [SerializeField] protected Slider _defenseSliderBack;
    [SerializeField] protected Slider _defenseSliderFront;
    [SerializeField] protected Slider _speedSliderBack;
    [SerializeField] protected Slider _speedSliderFront;
    [SerializeField] protected GameObject _passiveSkillBase;

    private bool isNeedRefresh;
    private MonsterMB monster;
    private UserMonsterInfo userMonster;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action<bool>)info.param["onClickClose"];
        userMonster = (UserMonsterInfo)info.param["userMonster"];

        monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

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

        _levelUpButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterLevelUpDialogFactory.Create(new MonsterLevelUpDialogRequest()
            {
                userMonster = userMonster,
            }))
            .Where(res => res.isNeedRefresh)
            .SelectMany(_ =>
            {
                isNeedRefresh = true;
                userMonster = ApplicationContext.userInventory.userMonsterList.First(u => u.id == userMonster.id);
                return RefreshUIObservable();
            })
            .Subscribe();

        SetSliderMaxValue();
        RefreshUIObservable().Subscribe();
    }

    private void SetSliderMaxValue()
    {
        _hpSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _hpSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _attackSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _attackSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _defenseSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _defenseSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _speedSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _speedSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
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

        // ゲージ
        _hpSliderBack.value = monster.level100Hp;
        _hpSliderFront.value = status.hp;
        _attackSliderBack.value = monster.level100Attack;
        _attackSliderFront.value = status.attack;
        _defenseSliderBack.value = monster.level100Defense;
        _defenseSliderFront.value = status.defense;
        _speedSliderBack.value = monster.level100Speed;
        _speedSliderFront.value = status.speed;

        // スキル
        SetSkillText();

        // モンスター画像、属性画像の設定
        return Observable.WhenAll(
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monster.id).Do(sprite => _monsterImage.sprite = sprite),
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterAttribute, (int)monster.attribute).Do(sprite => _monsterAttributeImage.sprite = sprite)
        ).AsUnitObservable();
    }

    private void SetSkillText()
    {
        var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.normalSkillId);
        var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.ultimateSkillId);
        var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);

        _normalSkillNameText.text = normalSkill.name;
        _normalSkillDescriptionText.text = normalSkill.description;
        _ultimateSkillNameText.text = ultimateSkill.name;
        _ultimateSkillDescriptionText.text = ultimateSkill.description;
        _passiveSkillBase.SetActive(passiveSkill != null);
        if (passiveSkill != null)
        {
            _passiveSkillNameText.text = passiveSkill.name;
            _passiveSkillDescriptionText.text = passiveSkill.description;
        }
    }

    /// <summary>
    /// レベルテキストで指定する文字列を取得する
    /// </summary>
    private string GetLevelText(int level) {
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
