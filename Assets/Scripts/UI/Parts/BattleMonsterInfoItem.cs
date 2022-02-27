using GameBase;
using PM.Enum.Monster;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleMonsterInfoItem")]
public class BattleMonsterInfoItem : MonoBehaviour
{
    [SerializeField] GameObject _baseObject;
    [SerializeField] Image _monsterImage;
    [SerializeField] Image _backgroundImage;
    [SerializeField] MonsterGradeParts _monsterGrade;

    public void Set(UserMonsterInfo userMonster)
    {
        if(userMonster == null)
        {
            _baseObject.SetActive(false);
            return;
        }

        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

        _backgroundImage.color = monster.attribute.Color();
        _monsterGrade.SetGradeImage(monster.initialGrade); // TODO: userMonster‚ÌƒOƒŒ[ƒh‚ðÝ’è
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, userMonster.monsterId)
            .Do(sprite =>
            {
                if (sprite != null) _monsterImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);
    }
}