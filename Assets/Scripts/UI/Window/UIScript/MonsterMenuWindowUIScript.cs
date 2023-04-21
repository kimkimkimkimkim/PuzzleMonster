using System.Linq;
using GameBase;
using PM.Enum.Item;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterMenu")]
public class MonsterMenuWindowUIScript : WindowBase {
    [SerializeField] protected Button _boxButton;
    [SerializeField] protected Button _formationButton;
    [SerializeField] protected Button _itemBoxButton;
    [SerializeField] protected Button _staminaHealButton;
    [SerializeField] protected IconItem _staminaHealItemIcon;

    public override void Init(WindowInfo info) {
        base.Init(info);

        _boxButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterBoxWindowFactory.Create(new MonsterBoxWindowRequest() {
                userMontserList = ApplicationContext.userData.userMonsterList,
            }))
            .Subscribe();

        _formationButton.OnClickIntentAsObservable()
            .SelectMany(_ => MonsterPartyListWindowFactory.Create(new MonsterPartyListWindowRequest() { }))
            .Subscribe();

        _itemBoxButton.OnClickIntentAsObservable()
            .SelectMany(_ => ItemBoxWindowFactory.Create(new ItemBoxWindowRequest()))
            .Subscribe();

        _staminaHealButton.OnClickIntentAsObservable()
            .SelectMany(_ => {
                var currentStamina = ApplicationContext.userData.stamina;
                var maxStamina = MasterRecord.GetMasterOf<StaminaMB>().GetAll().First(m => m.rank == ApplicationContext.userData.rank).stamina;
                var userStaminaRecovery = ApplicationContext.userData.userPropertyList.FirstOrDefault(u => u.propertyId == (long)PropertyType.StaminaRecovery);
                var userStaminaRecoveryNum = userStaminaRecovery != null ? userStaminaRecovery.num : 0;

                if (userStaminaRecoveryNum <= 0) {
                    // スタミナ回復薬ない場合
                    var staminaRecoveryName = MasterRecord.GetMasterOf<PropertyMB>().Get((long)PropertyType.StaminaRecovery).name;
                    var title = "確認";
                    var content = $"{staminaRecoveryName}が無いため\nスタミナを回復することができません";
                    return CommonDialogFactory.Create(new CommonDialogRequest() {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = title,
                        content = content,
                    }).AsUnitObservable();
                } else if (currentStamina > maxStamina) {
                    // スタミナ回復可能状態じゃない
                    var title = "確認";
                    var content = "スタミナがオーバーしているため\nスタミナを回復することができません";
                    return CommonDialogFactory.Create(new CommonDialogRequest() {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = title,
                        content = content,
                    }).AsUnitObservable();
                } else {
                    // それ以外ならスタミナ回復
                    return ApiConnection.UseStaminaRecovery()
                        .SelectMany(res => {
                            var title = "確認";
                            var content = "スタミナが回復しました";
                            return CommonDialogFactory.Create(new CommonDialogRequest() {
                                commonDialogType = CommonDialogType.YesOnly,
                                title = title,
                                content = content,
                            });
                        })
                        .Do(res => RefreshUI())
                        .AsUnitObservable();
                }
            })
            .Subscribe();

        RefreshUI();
    }

    private void RefreshUI() {
        var staminaRecovery = ApplicationContext.userData.userPropertyList.FirstOrDefault(u => u.propertyId == (long)PropertyType.StaminaRecovery);
        var staminaRecoveryNum = staminaRecovery != null ? staminaRecovery.num : 0;
        _staminaHealItemIcon.SetIcon(ItemType.Property, (long)PropertyType.StaminaRecovery);
        _staminaHealItemIcon.SetNumText(staminaRecoveryNum.ToString());
    }

    public override void Open(WindowInfo info) {
    }

    public override void Back(WindowInfo info) {
    }

    public override void Close(WindowInfo info) {
        base.Close(info);
    }
}
