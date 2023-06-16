using GameBase;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using PM.Enum.UI;

[ResourcePath("UI/Window/Window-MonsterFormation")]
public class MonsterFormationWindowUIScript : MonsterFormationBaseWindowUIScript {
    [SerializeField] protected Button _okButton;

    public override void Init(WindowInfo info) {
        base.Init(info);

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ => {
                if (!currentUserMonsterParty.userMonsterIdList.Any(id => id != null)) {
                    return CommonDialogFactory.Create(new CommonDialogRequest() {
                        commonDialogType = CommonDialogType.YesOnly,
                        content = "モンスターを1体以上選択してください",
                    }).AsUnitObservable();
                } else {
                    return Observable.ReturnUnit()
                        .SelectMany(res => {
                            var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.id == currentUserMonsterParty.id);
                            if (userMonsterParty != null && userMonsterParty.IsSame(currentUserMonsterParty)) {
                                // 現在選択中のパーティ情報が存在し何も変更がなければそのまま進む
                                return Observable.ReturnUnit();
                            } else {
                                // 変更が加えられていた場合はユーザーデータを更新する
                                return ApiConnection.UpdateUserMosnterFormation(currentPartyIndex, currentUserMonsterParty.userMonsterIdList)
                                    .Do(resp => currentUserMonsterParty = resp.userMonsterParty)
                                    .AsUnitObservable();
                            }
                        })
                        .SelectMany(resp => CommonDialogFactory.Create(new CommonDialogRequest() {
                            commonDialogType = CommonDialogType.YesOnly,
                            title = "確認",
                            content = "パーティを保存しました",
                        }))
                        .AsUnitObservable();
                }
            })
            .Subscribe();
    }
}
