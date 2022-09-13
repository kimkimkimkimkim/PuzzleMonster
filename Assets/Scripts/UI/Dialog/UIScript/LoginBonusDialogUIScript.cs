using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using static LoginBonusItem;

[ResourcePath("UI/Dialog/Dialog-LoginBonus")]
public class LoginBonusDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _fullScreenButton;
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Text _contentText;
    [SerializeField] protected GameObject _closeButtonBase;
    [SerializeField] protected GameObject _fullScreenButtonBase;
    [SerializeField] protected GameObject _contentTextBase;
    [SerializeField] protected GameObject _loginBonusItemBase30Day;
    [SerializeField] protected GameObject _loginBonusItemBase7Day;
    [SerializeField] protected List<LoginBonusItem> _loginBonusItemList30Day;
    [SerializeField] protected List<LoginBonusItem> _loginBonusItemList7Day;

    private UserLoginBonusInfo userLoginBonus;
    private LoginBonusMB loginBonus;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        userLoginBonus = (UserLoginBonusInfo)info.param["userLoginBonus"];
        loginBonus = MasterRecord.GetMasterOf<LoginBonusMB>().Get(userLoginBonus.loginBonusId);

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

        _fullScreenButton.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(2.0f), Scheduler.MainThreadIgnoreTimeScale)
            .Select((_,count) => count)
            .SelectMany(count =>
            {
                // Selectから渡ってくるcountは0からなのでプラス1してあげる
                return OnClickFullScreenButtonObservable(count + 1);
            })
            .Subscribe();

        _titleText.text = loginBonus.name;

        _contentTextBase.SetActive(true);
        _closeButtonBase.SetActive(false);
        _fullScreenButtonBase.SetActive(true);

        SetContentText(false);
        RefreshRewardUI(false);
    }

    /// <summary>
    /// フルスクリーンボタン押下時処理
    /// count: 1 ～
    /// </summary>
    private IObservable<Unit> OnClickFullScreenButtonObservable(int count)
    {
        var existsNextReward = userLoginBonus.loginDateList.Count < loginBonus.rewardItemList.Count;

        if(count == 1 && existsNextReward)
        {
            // 次の報酬にフォーカス
            SetContentText(true);
            RefreshRewardUI(true);
        }
        else
        {
            // 閉じるボタンを表示
            _contentTextBase.SetActive(false);
            _closeButtonBase.SetActive(true);
            _fullScreenButtonBase.SetActive(false);
        }
        return Observable.ReturnUnit();
    }

    private void SetContentText(bool isFocusNextDay)
    {
        var focusDateNum = isFocusNextDay ? userLoginBonus.loginDateList.Count + 1 : userLoginBonus.loginDateList.Count;
        var rewardItem = loginBonus.rewardItemList[focusDateNum - 1];
        var itemName = ClientItemUtil.GetName(rewardItem);

        var contentText = isFocusNextDay ?
            $"次のプレゼントは「{itemName}」です\n忘れずにログインしよう！" :
            $"本日のプレゼントは「{itemName}」です\nプレゼントボックスに送ってあります";
        _contentText.text = contentText;
    }

    private void RefreshRewardUI(bool isFocusNextDay)
    {
        var is7Day = loginBonus.rewardItemList.Count <= 7;
        var loginBonusItemList = is7Day ? _loginBonusItemList7Day : _loginBonusItemList30Day;

        loginBonusItemList.ForEach((item, index) =>
        {
            var dateNum = index + 1;
            var focusDateNum = isFocusNextDay ? userLoginBonus.loginDateList.Count + 1 : userLoginBonus.loginDateList.Count;

            if (dateNum <= loginBonus.rewardItemList.Count)
            {
                item.gameObject.SetActive(true);
                var state =
                    dateNum < focusDateNum ? LoginBonusItemState.Past :
                    dateNum == focusDateNum ? LoginBonusItemState.Today :
                    LoginBonusItemState.Future;

                item.SetUIObservable(userLoginBonus, dateNum, state).Subscribe();
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        });

        _loginBonusItemBase7Day.SetActive(is7Day);
        _loginBonusItemBase30Day.SetActive(!is7Day);
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
