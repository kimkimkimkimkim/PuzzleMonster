using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-MonsterPartyListScrollItem")]
public class MonsterPartyListScrollItem : ScrollItem
{
    [SerializeField] protected Sprite _emptySprite;
    [SerializeField] protected List<Image> _monsterImageList;

    private int partyId;

    public void SetMonsterImage(int partyId, List<long> monsterIdList) {
        this.partyId = partyId;
        _monsterImageList.ForEach(image => image.sprite = _emptySprite);

        var observableList = monsterIdList.Select((id, index) =>
            {
                if (0 <= index && index < _monsterImageList.Count)
                {
                    return PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, id)
                        .Where(res => res != null)
                        .Do(res =>
                        {
                            // 正しいスクロールアイテムの画像を変更するようにpartyIdで判定
                            if(this.partyId == partyId) _monsterImageList[index].sprite = res;
                        })
                        .AsUnitObservable();
                }
                else
                {
                    return Observable.ReturnUnit();
                }
            });

        Observable.WhenAll(observableList).Subscribe();
    }
}