using System.Collections.Generic;
using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterMenu")]
public class MonsterMenuWindowUIScript : WindowBase
{
    [SerializeField] protected Button _boxButton;
    [SerializeField] protected Button _formationButton;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _boxButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterBoxWindowFactory.Create(new MonsterBoxWindowRequest()
            {
                userMontserList = ApplicationContext.userInventory.userMonsterList,
            }))
            .Subscribe();

        _formationButton.OnClickIntentAsObservable()
            .SelectMany(_ => {
                var userMonsterList = ApplicationContext.userInventory.userMonsterList;
                var partyId = 1;
                var initialUserMonsterIdList = ApplicationContext.userData.userMonsterPartyList?.FirstOrDefault(u => u.partyId == partyId)?.userMonsterIdList ?? new List<string>();
                var initialUserMonsterList = initialUserMonsterIdList.Select(id => userMonsterList.First(u => u.id == id)).ToList();
                return MonsterFormationWindowFactory.Create(new MonsterFormationWindowRequest()
                {
                    userMontserList = userMonsterList,
                    initialUserMonsterList = initialUserMonsterList,
                });
            })
            .Subscribe();
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
