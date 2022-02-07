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
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected Image _monsterAttributeImage;
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _hpText;
    [SerializeField] protected Text _attackText;
    [SerializeField] protected Text _healText;
    [SerializeField] protected Slider _hpSliderBack;
    [SerializeField] protected Slider _hpSliderFront;
    [SerializeField] protected Slider _attackSliderBack;
    [SerializeField] protected Slider _attackSliderFront;
    [SerializeField] protected Slider _healSliderBack;
    [SerializeField] protected Slider _healSliderFront;

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
        _healSliderBack.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
        _healSliderFront.maxValue = ConstManager.Monster.MAX_STATUS_VALUE;
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
        _healText.text = status.heal.ToString();

        // ゲージ
        _hpSliderBack.value = monster.level100Hp;
        _hpSliderFront.value = status.hp;
        _attackSliderBack.value = monster.level100Attack;
        _attackSliderFront.value = status.attack;
        _healSliderBack.value = monster.level100Heal;
        _healSliderFront.value = status.heal;

        // モンスター画像、属性画像の設定
        return Observable.WhenAll(
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monster.id).Do(sprite => _monsterImage.sprite = sprite),
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterAttribute, (int)monster.attribute).Do(sprite => _monsterAttributeImage.sprite = sprite)
        ).AsUnitObservable();
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
