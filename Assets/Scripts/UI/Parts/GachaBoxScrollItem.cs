using GameBase;
using PM.Enum.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaBoxScrollItem")]
public class GachaBoxScrollItem : MonoBehaviour
{
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Button _emissionRateButton;
    [SerializeField] protected Transform _executeButtonBase;
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    public Transform executeButtonBase { get { return _executeButtonBase; } }

    private List<ItemMI> itemList;
    private IDisposable onClickEmissionRateButtonObservable;

    public void SetText(string text)
    {
        _titleText.text = text;
    }

    public void SetOnClickEmissionRateButtonAction(Action action)
    {
        if (onClickEmissionRateButtonObservable != null)
        {
            onClickEmissionRateButtonObservable.Dispose();
            onClickEmissionRateButtonObservable = null;
        }

        onClickEmissionRateButtonObservable = _emissionRateButton.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }

    public void RefreshScroll(List<long> pickUpMonsterIdList)
    {
        _infiniteScroll.Clear();

        itemList = pickUpMonsterIdList
            .Select(id => MasterRecord.GetMasterOf<MonsterMB>().Get(id))
            .Select(m => new ItemMI()
            {
                itemType = ItemType.Monster,
                itemId = m.id,
            })
            .ToList();

        _infiniteScroll.Init(itemList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((itemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var itemMI = itemList[index];

        scrollItem.SetIcon(itemMI);
    }
}