using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using static LoginBonusItem;

[ResourcePath("UI/Dialog/Dialog-LoginBonus")]
public class LoginBonusDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected GameObject _loginBonusItemBase30Day;
    [SerializeField] protected GameObject _loginBonusItemBase7Day;
    [SerializeField] protected List<LoginBonusItem> _loginBonusItemList30Day;
    [SerializeField] protected List<LoginBonusItem> _loginBonusItemList7Day;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var userLoginBonus = (UserLoginBonusInfo)info.param["userLoginBonus"];

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

        SetUI(userLoginBonus);
    }

    private void SetUI(UserLoginBonusInfo userLoginBonus)
    {
        var loginBonus = MasterRecord.GetMasterOf<LoginBonusMB>().Get(userLoginBonus.loginBonusId);
        var is7Day = loginBonus.rewardItemList.Count <= 7;
        var loginBonusItemList = is7Day ? _loginBonusItemList7Day : _loginBonusItemList30Day;

        _loginBonusItemBase7Day.SetActive(is7Day);
        _loginBonusItemBase30Day.SetActive(!is7Day);

        loginBonusItemList.ForEach((item, index) =>
        {
            var dateNum = index + 1;

            if (dateNum <= loginBonus.rewardItemList.Count)
            {
                item.gameObject.SetActive(true);
                var state =
                    dateNum < userLoginBonus.loginDateList.Count ? LoginBonusItemState.Past :
                    dateNum == userLoginBonus.loginDateList.Count ? LoginBonusItemState.Today :
                    LoginBonusItemState.Future;

                item.SetUIObservable(userLoginBonus, dateNum, state).Subscribe();
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        });
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
