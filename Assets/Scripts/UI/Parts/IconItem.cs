using GameBase;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using UniRx;
using System;
using System.Linq;
using PM.Enum.Monster;

[ResourcePath("UI/Parts/Parts-IconItem")]
public class IconItem : MonoBehaviour
{
    [SerializeField] protected Image _backgroundImage;
    [SerializeField] protected Image _frameImage;
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected Image _checkImage;
    [SerializeField] protected Image _rarityImage;
    [SerializeField] protected GameObject _notifyPanel;
    [SerializeField] protected GameObject _focusPanel;
    [SerializeField] protected GameObject _grayoutPanel;
    [SerializeField] protected GameObject _labelPanel;
    [SerializeField] protected GameObject _stackIconPanel;
    [SerializeField] protected Text _numText;
    [SerializeField] protected Text _grayoutText;
    [SerializeField] protected Text _labelText;
    [SerializeField] protected Text _text;
    [SerializeField] protected Text _stackNumText;
    [SerializeField] protected Text _levelText;
    [SerializeField] protected Text _underText;
    [SerializeField] protected List<Sprite> _frameSpriteList;
    [SerializeField] protected List<Color> _backgroundColorList;
    [SerializeField] protected Toggle _toggle;
    [SerializeField] protected Button _button;
    [SerializeField] protected CanvasGroup _canvasGroup;
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;

    public Toggle toggle
    { get { return _toggle; } private set { _toggle = value; } }

    private ItemType itemType;
    private long itemId;
    private IDisposable onClickButtonObservable;
    private IDisposable onLongClickButtonObservable;

    public void ShowIcon(bool isShow)
    {
        _iconImage.gameObject.SetActive(isShow);
    }

    public void SetIcon(ItemMI item, bool showNumTextAtOne = false, bool isMaxStatus = false)
    {
        itemType = item.itemType;
        itemId = item.itemId;

        var iconColorType = ClientItemUtil.GetIconColorType(item);
        var iconImageType = ClientItemUtil.GetIconImageType(item.itemType);
        var numText = item.num <= 1 && !showNumTextAtOne ? "" : item.num.ToString();

        SetFrameImage(iconColorType);
        SetIconImage(iconImageType, item.itemId);
        SetNumText(numText);
        SetShowItemDetailDialogAction(true);

        if (itemType == ItemType.Monster)
        {
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(itemId);
            var maxGrade = MasterRecord.GetMasterOf<GradeUpTableMB>().GetAll().Max(m => m.targetGrade);
            var maxLevel = MasterRecord.GetMasterOf<MaxMonsterLevelMB>().GetAll().First(m => m.monsterRarity == monster.rarity && m.monsterGrade == maxGrade).maxMonsterLevel;
            var userMonsterCustomData = new UserMonsterCustomData() { level = maxLevel, grade = maxGrade };
            var userMonster = ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.monsterId == itemId);
            if (isMaxStatus || userMonster == null) userMonster = new UserMonsterInfo() { monsterId = itemId, customData = userMonsterCustomData };
            SetMonsterIconInfo(userMonster);
            SetMonsterRarityImage(monster.rarity);
        }
    }

    public void SetIcon(ItemType itemType, long itemId)
    {
        var item = new ItemMI() { itemType = itemType, itemId = itemId };
        SetIcon(item);
    }

    public void SetMonsterIconInfo(UserMonsterInfo userMonster)
    {
        var action = new Action(() => { MonsterDetailDialogFactory.Create(new MonsterDetailDialogRequest() { userMonster = userMonster, canStrength = false }).Subscribe(); });

        SetLevelText(userMonster.customData.level);
        SetGrade(userMonster.customData.grade);
        SetLongClickAction(true, action);
    }

    private void SetFrameImage(IconColorType iconColor)
    {
        var index = (int)iconColor;
        _frameImage.sprite = _frameSpriteList[index];
        _backgroundImage.color = _backgroundColorList[index];
    }

    private void SetIconImage(IconImageType iconImageType, long itemId)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(iconImageType, itemId)
            .Do(sprite =>
            {
                if (sprite != null) _iconImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);
    }

    private void SetMonsterRarityImage(MonsterRarity monsterRarity)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterRarity, (int)monsterRarity)
            .Do(sprite =>
            {
                if (sprite != null) _rarityImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);
    }

    public void SetNumText(string text)
    {
        _numText.text = text;
    }

    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        _toggle.group = toggleGroup;
    }

    public void SetOnClickAction(Action action)
    {
        if (action == null) return;

        if (onClickButtonObservable != null)
        {
            onClickButtonObservable.Dispose();
            onClickButtonObservable = null;
        }

        onClickButtonObservable = _button.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }

    public void SetRaycastTarget(bool isOn)
    {
        _canvasGroup.blocksRaycasts = isOn;
    }

    public void ShowGrayoutPanel(bool isShow, string text = "")
    {
        _grayoutPanel.SetActive(isShow);
        _grayoutText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        _grayoutText.text = text;
    }

    public void ShowLabel(bool isShow, string text = "")
    {
        _labelPanel.SetActive(isShow);
        _labelText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        _labelText.text = text;
    }

    public void ShowText(bool isShow, string text = "")
    {
        _text.gameObject.SetActive(isShow);
        _text.text = text;
    }

    public void ShowCheckImage(bool isShow)
    {
        _checkImage.gameObject.SetActive(isShow);
    }

    public void SetStack(int stackNum)
    {
        _stackNumText.text = stackNum.ToString();
    }

    public void ShowStack(bool isShow)
    {
        _stackIconPanel.SetActive(isShow);
    }

    public void SetLevelText(int level)
    {
        _levelText.text = $"Lv.{level}";
    }

    public void ShowLevelText(bool isShow)
    {
        _levelText.gameObject.SetActive(isShow);
    }

    public void SetGrade(int grade)
    {
        _monsterGradeParts.SetGradeImage(grade);
    }

    public void ShowGrade(bool isShow)
    {
        _monsterGradeParts.gameObject.SetActive(isShow);
    }

    public void SetUnderText(string text)
    {
        _underText.text = text;
    }

    public void ShowUnderText(bool isShow)
    {
        _underText.gameObject.SetActive(isShow);
    }

    public void ShowRarityImage(bool isShow)
    {
        _rarityImage.gameObject.SetActive(isShow);
    }

    /// <summary>
    /// 長押し時にアイテム詳細ダイアログを表示するようにする
    /// </summary>
    public void SetShowItemDetailDialogAction(bool isSet)
    {
        var action = new Action(() =>
        {
            var itemDescription = MasterRecord.GetMasterOf<ItemDescriptionMB>().GetAll().FirstOrDefault(m => m.itemType == itemType && m.itemId == itemId);
            if (itemDescription != null) ItemDetailDialogFactory.Create(new ItemDetailDialogRequest() { itemDescription = itemDescription }).Subscribe();
        });
        SetLongClickAction(isSet, action);
    }

    public void SetLongClickAction(bool isSet, Action action = null)
    {
        if (onLongClickButtonObservable != null)
        {
            onLongClickButtonObservable.Dispose();
            onLongClickButtonObservable = null;
        }

        if (isSet)
        {
            onLongClickButtonObservable = _button.OnLongClickIntentAsObservable()
                .Do(_ => action?.Invoke())
                .Subscribe();
        }
    }
}