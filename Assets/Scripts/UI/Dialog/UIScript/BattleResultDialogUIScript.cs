using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.Battle;
using TMPro;

[ResourcePath("UI/Dialog/Dialog-BattleResult")]
public class BattleResultDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected Button _previousWaveButton;
    [SerializeField] protected Button _nextWaveButton;
    [SerializeField] protected TextMeshProUGUI _waveNumText;
    [SerializeField] protected InfiniteScroll _playerInfiniteScroll;
    [SerializeField] protected InfiniteScroll _enemyInfiniteScroll;
    [SerializeField] protected GameObject _winTitleBase;
    [SerializeField] protected GameObject _loseTitleBase;
    [SerializeField] protected GameObject _closeButtonBase;
    [SerializeField] protected GameObject _okButtonBase;
    [SerializeField] protected GameObject _previousWaveButtonBase;
    [SerializeField] protected GameObject _nextWaveButtonBase;

    private List<BattleMonsterInfo> playerBattleMonsterList;
    private List<BattleMonsterInfo> enemyBattleMonsterList;
    private List<List<BattleMonsterInfo>> enemyBattleMonsterListByWave;
    private int maxWaveNum = 0;
    private int currentWaveIndex = 0;
    private int maxScoreValue = 0;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var onClickOk = (Action)info.param["onClickOk"];
        var winOrLose = (WinOrLose)info.param["winOrLose"];
        playerBattleMonsterList = (List<BattleMonsterInfo>)info.param["playerBattleMonsterList"];
        enemyBattleMonsterListByWave = (List<List<BattleMonsterInfo>>)info.param["enemyBattleMonsterListByWave"];
        maxWaveNum = enemyBattleMonsterListByWave.Count;

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

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickOk != null)
                {
                    onClickOk();
                    onClickOk = null;
                }
            })
            .Subscribe();

        _previousWaveButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                currentWaveIndex--;
                RefreshWaveUI();
                RefreshEnemyScroll();
            })
            .Subscribe();

        _nextWaveButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                currentWaveIndex++;
                RefreshWaveUI();
                RefreshEnemyScroll();
            })
            .Subscribe();

        RefreshWaveUI();
        GetMaxScoreValue();
        RefreshWinOrLoseUI(winOrLose);
        RefreshPlayerScroll();
        RefreshEnemyScroll();
    }

    private void RefreshWaveUI()
    {
        _waveNumText.text = $"Wave {currentWaveIndex + 1}";
        _previousWaveButtonBase.SetActive(currentWaveIndex > 0);
        _nextWaveButtonBase.SetActive(currentWaveIndex < maxWaveNum - 1);
    }

    private void GetMaxScoreValue()
    {
        var maxScoreValue = 0;
        playerBattleMonsterList.ForEach(b =>
        {
            if (b.totalGiveDamage > maxScoreValue) maxScoreValue = b.totalGiveDamage;
            if (b.totalHealing > maxScoreValue) maxScoreValue = b.totalHealing;
            if (b.totalTakeDamage > maxScoreValue) maxScoreValue = b.totalTakeDamage;
        });
        enemyBattleMonsterListByWave.ForEach(list =>
        {
            list.ForEach(b =>
            {
                if (b.totalGiveDamage > maxScoreValue) maxScoreValue = b.totalGiveDamage;
                if (b.totalHealing > maxScoreValue) maxScoreValue = b.totalHealing;
                if (b.totalTakeDamage > maxScoreValue) maxScoreValue = b.totalTakeDamage;
            });
        });

        this.maxScoreValue = maxScoreValue;
    }

    private void RefreshWinOrLoseUI(WinOrLose winOrLose)
    {
        _winTitleBase.SetActive(winOrLose == WinOrLose.Win);
        _loseTitleBase.SetActive(winOrLose == WinOrLose.Lose);

        _okButtonBase.SetActive(winOrLose == WinOrLose.Win);
        _closeButtonBase.SetActive(winOrLose == WinOrLose.Lose);
    }

    private void RefreshPlayerScroll()
    {
        _playerInfiniteScroll.Clear();

        _playerInfiniteScroll.Init(playerBattleMonsterList.Count, OnUpdatePlayerItem);
    }

    private void OnUpdatePlayerItem(int index, GameObject item)
    {
        if ((playerBattleMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<BattleScoreScrollItem>();
        var battleMonster = playerBattleMonsterList[index];

        scrollItem.SetInfo(battleMonster, maxScoreValue);
    }

    private void RefreshEnemyScroll()
    {
        _enemyInfiniteScroll.Clear();

        enemyBattleMonsterList = enemyBattleMonsterListByWave[currentWaveIndex];

        _enemyInfiniteScroll.Init(enemyBattleMonsterList.Count, OnUpdateEnemyItem);
    }

    private void OnUpdateEnemyItem(int index, GameObject item)
    {
        if ((enemyBattleMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<BattleScoreScrollItem>();
        var battleMonster = enemyBattleMonsterList[index];

        scrollItem.SetInfo(battleMonster, maxScoreValue);
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
