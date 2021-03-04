using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-DropItem")]
public class DropItem : MonoBehaviour
{
    [SerializeField] protected List<Sprite> _dropSpriteList;
    [SerializeField] protected Image _dropImage;
    [SerializeField] protected Image _grayOutImage;
    [SerializeField] protected GameObject _grayOutPanel;
    [SerializeField] protected CanvasGroup _canvasGroup;

    private DropType type;
    private DropIndex index;

    public void SetInfo(DropIndex index)
    {
        var disturbDropRatio = 35.0f;
        var random = UnityEngine.Random.Range(0.0f, 100.0f);
        var type = random < disturbDropRatio ? DropType.Disturb : DropType.Normal;

        var spriteIndex = (int)type;
        _dropImage.sprite = _dropSpriteList[spriteIndex];
        _grayOutImage.sprite = _dropSpriteList[spriteIndex];

        this.type = type;
        this.index = index;
    }

    public DropIndex GetIndex()
    {
        return index;
    }

    public DropType GetDropType()
    {
        return type;
    }

    public void RefreshIndex(DropIndex index)
    {
        this.index = index;
    }

    public void ShowGrayOutPanel(bool isShow)
    {
        _grayOutPanel.SetActive(isShow);
    }

    // 自身からターゲットとなるドロップの選択が可能かどうかを返す
    public bool CanSelect(DropItem drop)
    {
        // お邪魔ドロップなら選択不可
        if (drop.GetDropType() == DropType.Disturb) return false;

        var neighboringIndexList = GameUtil.GetNeighboringIndexList(this.index);
        return neighboringIndexList.Any(index => index == drop.GetIndex());
    }

    public IObservable<Unit> PlayDeleteAnimationObservable()
    {
        var time = 0.5f;
        var endAlpha = 0.1f;

        return _canvasGroup.DOFade(endAlpha, time).OnCompleteAsObservable().Do(_ => Destroy(gameObject)).AsUnitObservable();
    }

    public CanvasGroup GetCanvasGroup()
    {
        return _canvasGroup;
    }
}

public struct DropIndex
{
    public int column; // 左からのIndex
    public int row; // 下からのIndex

    public DropIndex(int column,int row)
    {
        this.column = column;
        this.row = row;
    }

    public static bool operator ==(DropIndex index1, DropIndex index2)
    {
        return index1.column == index2.column && index1.row == index2.row;
    }

    public static bool operator !=(DropIndex index1, DropIndex index2)
    {
        return !(index1 == index2);
    }
}
