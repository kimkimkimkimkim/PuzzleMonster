using GameBase;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using UniRx;
using System;
using System.Linq;
using UnityEngine.U2D;
using UnityEditor.iOS.Xcode;
using System.Text.RegularExpressions;
using PM.Enum.Monster;

[ResourcePath("UI/Parts/Parts-IconItem")]
public class IconItem : MonoBehaviour
{
    [SerializeField] protected Image _backgroundImage;
    [SerializeField] protected Image _frameImage;
    [SerializeField] protected Image _iconImage;
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

    private ItemType itemType;
    private long itemId;
    private List<MonsterSpriteInfo> monsterSpriteInfoList;
    private IDisposable onClickButtonObservable;
    private IDisposable onLongClickButtonObservable;
    private IDisposable monsterAnimationObservable;

    public void ShowIcon(bool isShow)
    {
        _iconImage.gameObject.SetActive(isShow);
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
        SetShowItemDetailDialogAction(true);

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
                if (sprite != null) _iconImage.sprite = sprite;
            })
            .AsUnitObservable();
    }

    private IObservable<Unit> SetMonsterIconImageObservable(long monsterId)
    {
        SpriteAtlas spriteAtlas = null;
        TextAsset textAsset = null;
        return Observable.WhenAll(
            PMAddressableAssetUtil.GetMonsterIconSpriteAtlasObservable(monsterId).Do(sa => spriteAtlas = sa).AsUnitObservable(),
            PMAddressableAssetUtil.GetMonsterSpriteInfoObservable(monsterId).Do(ta => textAsset = ta).AsUnitObservable()
        )
            .Do(_ =>
            {
                if (spriteAtlas != null && textAsset != null)
                {
                    monsterSpriteInfoList = new List<MonsterSpriteInfo>();
                    var textList = textAsset.text.Split('\n').ToList();
                    var monsterStateNameList = Enum.GetNames(typeof(MonsterState)).Select(name => $"(?<name>{name})_(?<index>...).png").ToList();
                    var monsterStatePattern = string.Join("|", monsterStateNameList);

                    // スプライト情報からステイトや座標などを取得
                    textList.ForEach((text, index) =>
                    {
                        var match = Regex.Match(text, monsterStatePattern, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            var monsterState = GetMonsterState(match.Groups["name"].ToString());
                            var success = int.TryParse(match.Groups["index"].ToString().TrimStart('0'), out int stateIndex);
                            var posAndSizeStr = textList[index + 3];
                            var posAndSizeMatch = Regex.Match(posAndSizeStr, "{{(?<x>.+),(?<y>.+)},{(?<w>.+),(?<h>.+)}}");
                            if (posAndSizeMatch.Success)
                            {
                                var x = int.Parse(posAndSizeMatch.Groups["x"].ToString());
                                var y = int.Parse(posAndSizeMatch.Groups["y"].ToString());
                                var w = int.Parse(posAndSizeMatch.Groups["w"].ToString());
                                var h = int.Parse(posAndSizeMatch.Groups["h"].ToString());
                                var xIndex = x / (w + 1);
                                var yIndex = y / (h + 1);
                                var monsterSpriteInfo = new MonsterSpriteInfo()
                                {
                                    xIndex = xIndex,
                                    yIndex = yIndex,
                                    monsterState = monsterState,
                                    stateIndex = stateIndex,
                                };
                                monsterSpriteInfoList.Add(monsterSpriteInfo);
                            }
                        }
                    });

                    // マルチプルスプライトの分割順に並び替える
                    monsterSpriteInfoList = monsterSpriteInfoList.OrderBy(i => i.yIndex).ThenBy(i => i.xIndex).ToList();

                    // スプライトとスプライトのインデックスを割り当てる
                    monsterSpriteInfoList.ForEach((i, index) =>
                    {
                        i.spriteAtlasIndex = index;
                        i.sprite = spriteAtlas.GetSprite($"{monsterId}_{index}");
                    });
                }
            })
            .Do(_ => SetMonsterState(MonsterState.Idle));
    }

    public void SetMonsterState(MonsterState state)
    {
        if (monsterSpriteInfoList == null || !monsterSpriteInfoList.Any()) return;

        if (monsterAnimationObservable != null) monsterAnimationObservable.Dispose();
        _iconImage.sprite = monsterSpriteInfoList.First(i => i.monsterState == state).sprite;
    }

    public void SetMonsterStateAnimation(MonsterState state)
    {
        if (monsterSpriteInfoList == null || !monsterSpriteInfoList.Any()) return;

        if (monsterAnimationObservable != null) monsterAnimationObservable.Dispose();
        monsterAnimationObservable = PlayMonsterStateAnimationObservable(state, false)
            .RepeatSafe()
            .Subscribe()
            .AddTo(this);
    }

    public IObservable<bool> PlayMonsterStateAnimationObservable(MonsterState state, bool isDispose = true)
    {
        if (monsterSpriteInfoList == null || !monsterSpriteInfoList.Any()) return Observable.Return(false);

        if (isDispose && monsterAnimationObservable != null) monsterAnimationObservable.Dispose();
        return Observable.Create<bool>(observer =>
        {
            const float INTERVAL = 0.07f;
            var spriteList = monsterSpriteInfoList.Where(i => i.monsterState == state).OrderBy(i => i.stateIndex).ToList();
            monsterAnimationObservable = Observable.Interval(TimeSpan.FromSeconds(INTERVAL))
                .Do(index =>
                {
                    _iconImage.sprite = spriteList[(int)index].sprite;
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

    private MonsterState GetMonsterState(string monsterStateName)
    {
        foreach (MonsterState state in Enum.GetValues(typeof(MonsterState)))
        {
            if (Regex.IsMatch(state.ToString(), monsterStateName, RegexOptions.IgnoreCase)) return state;
        }
        return MonsterState.None;
    }

    private class MonsterSpriteInfo
    {
        public int xIndex { get; set; }
        public int yIndex { get; set; }
        public MonsterState monsterState { get; set; }
        public int stateIndex { get; set; }
        public int spriteAtlasIndex { get; set; }
        public Sprite sprite { get; set; }
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