using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using DG.Tweening;

[ResourcePath("UI/Dialog/Dialog-Roulette")]
public class RouletteDialogUIScript : DialogBase
{
    [SerializeField] protected List<RouletteItem> _rouletteItemList;
    [SerializeField] protected Button _spinButton;
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected RectTransform _arrowRT;
    [SerializeField] protected GameObject _rouletteBase;
    [SerializeField] protected GameObject _closeButtonBase;

    private float radius;

    public override void Init(DialogInfo info)
    {
        var itemList = (List<ItemMI>)info.param["itemList"];
        var electedIndex = (int)info.param["electedIndex"];
        var onClickClose = (Action)info.param["onClickClose"];

        _spinButton.OnClickIntentAsObservable(ButtonClickIntent.OnlyOneTap)
            .SelectMany(_ => SpinObservable(electedIndex))
            .SelectMany(_ => Observable.Timer(TimeSpan.FromSeconds(0.5f)))
            .SelectMany(_ => CommonReceiveDialogFactory.Create(new CommonReceiveDialogRequest() { itemList = new List<ItemMI>() { itemList[electedIndex]} }))
            .Do(_ => _closeButtonBase.SetActive(true))
            .Subscribe();

        _closeButton.OnClickIntentAsObservable(ButtonClickIntent.OnlyOneTap)
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        radius = _arrowRT.localPosition.y;
        _rouletteBase.SetActive(false);
        _closeButtonBase.SetActive(false);
        Focus(0);
        SetItemUI(itemList);
    }

    private void SetItemUI(List<ItemMI> itemList)
    {
        var observableList = _rouletteItemList.Select((rouletteItem, index) =>
        {
            var item = itemList[index];
            return rouletteItem.SetUIObservable(item);
        }).ToList();
        Observable.WhenAll(observableList).Do(_ => _rouletteBase.SetActive(true)).Subscribe();
    }

    private void Focus(int selectedIndex)
    {
        _rouletteItemList.ForEach((rouletteItem, index) =>
        {
            rouletteItem.Focus(index == selectedIndex);
        });
    }

    private IObservable<Unit> SpinObservable(int selectedIndex)
    {
        return Observable.ReturnUnit()
            .SelectMany(_ =>
            {
                var rotateNum = 25;
                var anglePerItem = 360.0f / _rouletteItemList.Count;
                var endAngle = anglePerItem * selectedIndex + ((rotateNum - 1) * 360.0f);
                var animation = DOTween.Sequence()
                    .Append(DOVirtual.Float(0, endAngle, 5, (a) => SetArrowPosition(a)).SetEase(Ease.InOutExpo));
                return animation.PlayAsObservable().AsUnitObservable();
            });
    }

    private void SetArrowPosition(float angle)
    {
        var radian = angle * (float)Math.PI / 180.0f;
        var x = radius * (float)Math.Sin(radian);
        var y = radius * (float)Math.Cos(radian);
        _arrowRT.anchoredPosition = new Vector3(x, y);
        _arrowRT.rotation = Quaternion.Euler(0.0f, 0.0f, -angle);

        var baseAngle = GetBaseAngle(angle);
        var anglePerItem = 360.0f / _rouletteItemList.Count;
        var areaIndex = (int)Math.Floor(baseAngle / (anglePerItem / 2));
        switch (areaIndex)
        {
            case 0:
            case 15:
                Focus(0);
                break;
            case 1:
            case 2:
                Focus(1);
                break;
            case 3:
            case 4:
                Focus(2);
                break;
            case 5:
            case 6:
                Focus(3);
                break;
            case 7:
            case 8:
                Focus(4);
                break;
            case 9:
            case 10:
                Focus(5);
                break;
            case 11:
            case 12:
                Focus(6);
                break;
            case 13:
            case 14:
                Focus(7);
                break;
            default:
                break;
        }
    }

    private float GetBaseAngle(float angle)
    {
        if (angle >= 0)
        {
            if (angle < 360.0f)
            {
                return angle;
            }
            else
            {
                return GetBaseAngle(angle - 360.0f);
            }
        }
        else
        {
            return GetBaseAngle(angle + 360.0f);
        }
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
