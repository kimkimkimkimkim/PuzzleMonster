using GameBase;
using PM.Enum.Item;
using PM.Enum.Monster;
using PM.Enum.UI;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleMonsterInfoItem")]
public class BattleMonsterInfoItem : MonoBehaviour
{
    [SerializeField] private GameObject _baseObject;
    [SerializeField] private IconItem _iconItem;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private MonsterGradeParts _monsterGrade;
    [SerializeField] private Button _button;

    private IDisposable onClickButtonObservable;

    public void Set(UserMonsterInfo userMonster)
    {
        if (userMonster == null)
        {
            _baseObject.SetActive(false);
            return;
        }

        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

        _backgroundImage.color = monster.attribute.Color();
        _monsterGrade.SetGradeImage(userMonster.customData.grade);
        _iconItem.SetIcon(ItemType.Monster, userMonster.monsterId);
    }

    public void SetOnClickAction(Action action)
    {
        if (action == null) return;

        if (onClickButtonObservable != null)
        {
            onClickButtonObservable.Dispose();
            onClickButtonObservable = null;
        }

        onClickButtonObservable = _button.OnClickAsObservable()
            .Do(_ => action())
            .Subscribe();
    }
}