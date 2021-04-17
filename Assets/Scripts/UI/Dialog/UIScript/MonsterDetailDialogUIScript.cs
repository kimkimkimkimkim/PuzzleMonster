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
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _hpText;
    [SerializeField] protected Text _attackText;
    [SerializeField] protected Text _healText;

    private MonsterMB monster;
    private UserMonsterInfo userMonster;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        userMonster = (UserMonsterInfo)info.param["userMonster"];

        monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

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

        _levelUpButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterLevelUpDialogFactory.Create(new MonsterLevelUpDialogRequest()))
            .Subscribe();

        RefreshUI().Subscribe();
    }

    private IObservable<Unit> RefreshUI()
    {
        _nameText.text = monster.name;
        _monsterGradeParts.SetGradeImage(userMonster.customData.grade);
        _levelText.text = GetLevelText(userMonster.customData.level);
        _hpText.text = userMonster.customData.hp.ToString();
        _attackText.text = userMonster.customData.attack.ToString();
        _healText.text = userMonster.customData.heal.ToString();

        return PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monster.id)
            .Do(sprite => _monsterImage.sprite = sprite)
            .AsUnitObservable();
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
