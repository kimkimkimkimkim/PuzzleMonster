using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.Monster;
using PM.Enum.SortOrder;

[ResourcePath("UI/Dialog/Dialog-SortOrderMonster")]
public class SortOrderMonsterDialogUIScript : DialogBase
{
    [SerializeField] protected List<Toggle> _filterAttributeToggleList;
    [SerializeField] protected List<Toggle> _sortOrderTypeToggleList;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected Button _closeButton;

    public override void Init(DialogInfo info)
    {
        var initialFilterAttribute = (List<MonsterAttribute>)info.param["initialFilterAttribute"];
        var initialSortOrderType = (SortOrderTypeMonster)info.param["initialSortOrderType"];
        var onClickClose = (Action)info.param["onClickClose"];
        var onClickOk = (Action<List<MonsterAttribute>, SortOrderTypeMonster>)info.param["onClickOk"];

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                var filterAttribute = GetSelectedFilterAttribute();
                var sortOrderType = GetSelectedSortOrderType();
                if (onClickOk != null) {
                    onClickOk(filterAttribute, sortOrderType);
                    onClickOk = null;
                }
            })
            .Subscribe();

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

        SetFilterAttriuteToggle(initialFilterAttribute);
        SetSortOrderToggle(initialSortOrderType);
    }

    private void SetFilterAttriuteToggle(List<MonsterAttribute> filterAttribute) 
    {
        _filterAttributeToggleList.ForEach((toggle, index) => {
            var monsterAttribute = (MonsterAttribute)(index + 1);
            var isSelected = filterAttribute.Contains(monsterAttribute);
            toggle.isOn = isSelected;
        });
    }

    private void SetSortOrderToggle(SortOrderTypeMonster sortOrderType) 
    {
        var index = (int)sortOrderType - 1;
        _sortOrderTypeToggleList[index].isOn = true;
    }

    private List<MonsterAttribute> GetSelectedFilterAttribute() 
    {
        // もし全属性を選択していたら空のリストを返す
        if (_filterAttributeToggleList.All(toggle => toggle.isOn)) return new List<MonsterAttribute>();

        return _filterAttributeToggleList
            .Select((toggle, index) => (toggle, index))
            .Where(taple => taple.toggle.isOn)
            .Select(taple => (MonsterAttribute)(taple.index + 1))
            .ToList();
    }

    private SortOrderTypeMonster GetSelectedSortOrderType() 
    {
        return (SortOrderTypeMonster)(_sortOrderTypeToggleList.FindIndex(t => t.isOn) + 1);
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
