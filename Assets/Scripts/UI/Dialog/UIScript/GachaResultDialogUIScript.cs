using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;

[ResourcePath("UI/Dialog/Dialog-GachaResult")]
public class GachaResultDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Image _gachaItemImage;
    [SerializeField] protected Text _contentText;
    [SerializeField] protected GameObject _iconItemBase;
    [SerializeField] protected List<IconItem> _iconItemList;

    private List<ItemMI> itemList;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        itemList = (List<ItemMI>)info.param["itemList"];

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ =>
            {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        if (itemList.Count == 1)
        {
            SetOneUI();
        }
        else
        {
            SetTenUI();
        }
    }

    /// <summary>
    /// 単発ガチャ用のUIをセット
    /// </summary>
    private void SetOneUI()
    {
        _gachaItemImage.gameObject.SetActive(true);
        _contentText.gameObject.SetActive(true);
        _iconItemBase.gameObject.SetActive(false);

        SetGachaItemImage();
        SetText();
    }

    /// <summary>
    /// 10連ガチャ用のUIをセット
    /// </summary>
    private void SetTenUI()
    {
        _gachaItemImage.gameObject.SetActive(false);
        _contentText.gameObject.SetActive(false);
        _iconItemBase.gameObject.SetActive(true);

        _iconItemList.ForEach((iconItem, index) =>
        {
            var item = itemList[index];
            iconItem.SetIcon(item);
            iconItem.ShowRarityImage(true);
        });
    }

    private void SetGachaItemImage()
    {
        if (itemList == null || !itemList.Any()) return;

        // アイコン画像の設定
        // TODO : リスト内全部のアイテムを表示するように
        var item = itemList[0];
        PMAddressableAssetUtil.GetIconImageSpriteObservable(ClientItemUtil.GetIconImageType(item.itemType), item.itemId)
            .Do(sprite =>
            {
                _gachaItemImage.sprite = sprite;
            })
            .Subscribe();
    }

    private void SetText()
    {
        if (itemList == null || !itemList.Any()) return;

        // テキストの設定
        var item = itemList[0];
        // TODO : 現状はモンスターだけ
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(item.itemId);
        var contentText = $"{monster.name}をゲットした";
        _contentText.text = contentText;
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