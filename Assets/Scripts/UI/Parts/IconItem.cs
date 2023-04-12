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
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _monsterIconImage;
    [SerializeField] protected Image _checkImage;
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

    private Image iconImage
    {
        get
        {
            if (itemType != ItemType.Monster)
            {
                if (_monsterIconImage != null) _monsterIconImage.gameObject.SetActive(false);
                return _iconImage;
            }
            else
            {
                if (_iconImage != null) _iconImage.gameObject.SetActive(false);
                return _monsterIconImage;
            }
        }

        set
        {
            if (itemType != ItemType.Monster)
            {
                if (_monsterIconImage != null) _monsterIconImage.gameObject.SetActive(false);
                _iconImage = value;
            }
            else
            {
                if (_iconImage != null) _iconImage.gameObject.SetActive(false);
                _monsterIconImage = value;
            }
        }
    }

    private ItemType itemType;
    private long itemId;
    private List<MonsterSpriteDataMI> monsterSpriteDataList;
    private IDisposable onClickButtonObservable;
    private IDisposable onLongClickButtonObservable;
    private IDisposable monsterAnimationObservable;

    public void ShowIcon(bool isShow)
    {
        iconImage.gameObject.SetActive(isShow);
    }

    public void SetIcon(ItemMI item, bool showNumTextAtOne = false)
    {
        SetIconObservable(item, showNumTextAtOne).Subscribe().AddTo(this);
    }

    public IObservable<Unit> SetIconObservable(ItemMI item, bool showNumTextAtOne = false)
    {
        itemType = item.itemType;
        itemId = item.itemId;

        var iconColorType = ClientItemUtil.GetIconColorType(item);
        var iconImageType = ClientItemUtil.GetIconImageType(item.itemType);
        var numText = item.num <= 1 && !showNumTextAtOne ? "" : item.num.ToString();

        SetFrameImage(iconColorType);
        SetNumText(numText);

        if (itemType == ItemType.Monster)
        {
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(itemId);
            var maxGrade = MasterRecord.GetMasterOf<GradeUpTableMB>().GetAll().Max(m => m.targetGrade);
            var maxLevel = MasterRecord.GetMasterOf<MaxMonsterLevelMB>().GetAll().First(m => m.monsterRarity == monster.rarity && m.monsterGrade == maxGrade).maxMonsterLevel;
            var userMonsterCustomData = new UserMonsterCustomData() { level = maxLevel, grade = maxGrade };
            var userMonster = new UserMonsterInfo() { monsterId = itemId, customData = userMonsterCustomData };
            SetMonsterIconInfo(userMonster);
            return SetMonsterIconImageObservable(itemId);
        }
        else
        {
            return SetIconImageObservable(iconImageType, item.itemId);
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

    private IObservable<Unit> SetIconImageObservable(IconImageType iconImageType, long itemId)
    {
        return PMAddressableAssetUtil.GetIconImageSpriteObservable(iconImageType, itemId)
            .Do(sprite =>
            {
                if (sprite != null) iconImage.sprite = sprite;
            })
            .AsUnitObservable();
    }

    private IObservable<Unit> SetMonsterIconImageObservable(long monsterId)
    {
        return PMAddressableAssetUtil.GetMonsterIconSpriteAtlasObservable(monsterId)
            .Do(spriteAtlas =>
            {
                // スプライトの割り当て
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
                monsterSpriteDataList = monster.monsterSpriteDataList;
                monsterSpriteDataList.ForEach(m =>
                {
                    var sprite = spriteAtlas.GetSprite($"{monsterId}_{m.spriteAtlasIndex}");
                    m.sprite = sprite;
                });

                // 画像サイズの変更
                var width = monster.spriteWidth;
                var height = monster.spriteHeight;
                var xScale = width / 100.0f;
                var yScale = height / 100.0f;
                iconImage.transform.localScale = new Vector3(xScale, yScale, 1.0f);
            })
            .Do(_ => SetMonsterStateAnimation(MonsterState.Breathing))
            .AsUnitObservable();
    }

    public void SetMonsterState(MonsterState state)
    {
        if (monsterSpriteDataList == null || !monsterSpriteDataList.Any()) return;

        if (monsterAnimationObservable != null) monsterAnimationObservable.Dispose();
        iconImage.sprite = monsterSpriteDataList.First(i => i.monsterState == state).sprite;
    }

    public void SetMonsterStateAnimation(MonsterState state)
    {
        if (monsterSpriteDataList == null || !monsterSpriteDataList.Any()) return;

        if (monsterAnimationObservable != null) monsterAnimationObservable.Dispose();
        monsterAnimationObservable = PlayMonsterStateAnimationObservable(state, false)
            .RepeatSafe()
            .Subscribe()
            .AddTo(this);
    }

    public IObservable<bool> PlayMonsterStateAnimationObservable(MonsterState state, bool isDispose = true)
    {
        if (monsterSpriteDataList == null || !monsterSpriteDataList.Any()) return Observable.Return(false);

        if (isDispose && monsterAnimationObservable != null) monsterAnimationObservable.Dispose();
        return Observable.Create<bool>(observer =>
        {
            const float INTERVAL = 0.12f;
            var spriteList = monsterSpriteDataList.Where(i => i.monsterState == state).OrderBy(i => i.stateIndex).ToList();
            monsterAnimationObservable = Observable.Interval(TimeSpan.FromSeconds(INTERVAL))
                .Do(index =>
                {
                    if (iconImage != null) iconImage.sprite = spriteList[(int)index].sprite;
                })
                .Take(spriteList.Count)
                .Buffer(spriteList.Count)
                .DoOnCompleted(() =>
                {
                    observer.OnNext(true);
                    observer.OnCompleted();
                })
                .DoOnCancel(() =>
                {
                    observer.OnNext(false);
                })
                .Subscribe();
            return Disposable.Empty;
        });
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