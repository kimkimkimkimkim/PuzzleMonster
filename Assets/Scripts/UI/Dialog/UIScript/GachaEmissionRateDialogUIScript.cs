using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.Monster;
using PM.Enum.Gacha;

[ResourcePath("UI/Dialog/Dialog-GachaEmissionRate")]
public class GachaEmissionRateDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Transform _tabItemBase;
    [SerializeField] protected ToggleGroup _toggleGroup;
    [SerializeField] protected Text _contentText;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var gachaBoxId = (long)info.param["gachaBoxId"];

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

        SetTabItem(gachaBoxId);
    }

    private void SetTabItem(long gachaBoxId)
    {
        foreach(GameObject obj in _tabItemBase)
        {
            Destroy(obj);
        }

        var gachaBox = MasterRecord.GetMasterOf<GachaBoxMB>().Get(gachaBoxId);
        var gachaBoxDetailList = MasterRecord.GetMasterOf<GachaBoxDetailMB>().GetAll()
            .Where(m => m.gachaBoxId == gachaBox.id)
            .Where(m => ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList))
            .ToList();
        var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().ToList();
        var pickUpMonsterNum = gachaBox.pickUpMonsterIdList.Count;
        var ssrMonsterWithoutPickUpNum = monsterList
            .Where(m => m.rarity == MonsterRarity.SSR)
            .Where(m => gachaBox.gachaBoxTypeList.Any(gachaBoxType => m.gachaBoxTypeList.Contains(gachaBoxType)))
            .Count()
            - pickUpMonsterNum;
        var gachaEmissionTypeList = gachaBoxDetailList
            .SelectMany(m =>
            {
                switch (m.gachaExecuteType)
                {
                    case GachaExecuteType.One:
                        return new List<GachaEmissionType>() { GachaEmissionType.Normal };
                    case GachaExecuteType.OneUpperSSR1:
                        return new List<GachaEmissionType>() { GachaEmissionType.UpperSsr };
                    case GachaExecuteType.Ten:
                        return new List<GachaEmissionType>() { GachaEmissionType.Normal};
                    case GachaExecuteType.TenUpperSR1:
                        return new List<GachaEmissionType>() { GachaEmissionType.Normal, GachaEmissionType.UpperSr };
                    case GachaExecuteType.TenUpperSSR1:
                        return new List<GachaEmissionType>() { GachaEmissionType.Normal, GachaEmissionType.UpperSsr };
                    default:
                        return new List<GachaEmissionType>();
                }
            })
            .Distinct()
            .OrderBy(type => (int)type)
            .ToList();

        gachaEmissionTypeList.ForEach((gachaEmissionType, index) =>
        {
            var tabItem = UIManager.Instance.CreateContent<GachaEmissionRateTabItem>(_tabItemBase);
            var title = GetName(gachaEmissionType);
            var emissionPercentByRarity = GetEmisionPercentByRarity(gachaBox, gachaEmissionType);
            var eachPickUpMonsterEmissionRate = emissionPercentByRarity.pickUpSsrPercent;
            var eachSsrMonsterWithoutPickUpEmissionRate = (emissionPercentByRarity.ssrAllPercent - (eachPickUpMonsterEmissionRate * pickUpMonsterNum)) / ssrMonsterWithoutPickUpNum;
            var pickUpMonsterEmissionRateTextList = gachaBox.pickUpMonsterIdList
                .Select(id => monsterList.First(m => m.id == id))
                .Select(m => $"　{m.name}: {eachPickUpMonsterEmissionRate}％")
                .ToList();
            var content =
                "<color=\"#FFE42D\">■提供割合について</color>\n" +
                $"SSR: {emissionPercentByRarity.ssrAllPercent}％\n" +
                $"{(pickUpMonsterEmissionRateTextList.Any() ? $"{string.Join("\n", pickUpMonsterEmissionRateTextList)}\n" : "")}" +
                $"　{(pickUpMonsterEmissionRateTextList.Any() ? $"その他:" : "各:")} {eachSsrMonsterWithoutPickUpEmissionRate}％\n" +
                $"SR: {emissionPercentByRarity.srAllPercent}％\n" +
                $"R: {emissionPercentByRarity.rAllPercent}％";

            tabItem.Init(title, _toggleGroup, index == 0, gachaEmissionTypeList.Count == 1);
            tabItem.toggle.OnValueChangedIntentAsObservable()
                .Do(isOn =>
                {
                    tabItem.OnValueChenged(isOn);
                    if (isOn)
                    {
                        _contentText.text = content;
                    }
                })
                .Subscribe()
                .AddTo(tabItem);
        });
    }

    private EmisionPercentByRarity GetEmisionPercentByRarity(GachaBoxMB gachaBox, GachaEmissionType gachaEmissionType)
    {
        switch (gachaEmissionType)
        {
            case GachaEmissionType.Normal:
                return gachaBox.normalEmissionPercentByRarity;
            case GachaEmissionType.UpperSr:
                return gachaBox.upperSrEmissionPercentByRarity;
            case GachaEmissionType.UpperSsr:
                return gachaBox.upperSsrEmissionPercentByRarity;
            case GachaEmissionType.None:
            default:
                return gachaBox.normalEmissionPercentByRarity;
        }
    }

    private enum GachaEmissionType { 
        None = 0,

        /// <summary>
        /// 通常
        /// </summary>
        Normal = 1,

        /// <summary>
        /// SR以上確定
        /// </summary>
        UpperSr = 2,

        /// <summary>
        /// SSR以上確定
        /// </summary>
        UpperSsr = 3,
    }

    private string GetName(GachaEmissionType gachaEmissionType)
    {
        switch (gachaEmissionType)
        {
            case GachaEmissionType.Normal:
                return "通常";
            case GachaEmissionType.UpperSr:
                return "SR以上確定";
            case GachaEmissionType.UpperSsr:
                return "SSR以上確定";
            case GachaEmissionType.None:
            default:
                return "";
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
