using System;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Game")]
public class GameWindowUIScript : WindowBase
{
    private const float BOARD_MARGIN = 0.0f;
    private const float DROP_SPACE = 24f; // ドロップ間の距離
    private const int MAX_ROW_NUM = 6;
    private const int COLUMN_NUM = 7;
    private const int MAX_SELECTABLE_DROP_NUM = 10;

    [SerializeField] protected BoardItem _board;
    [SerializeField] protected Text _limiNumText;
    [SerializeField] protected List<GameObject> _skillActiveObjectList;
    [SerializeField] protected List<EnemyMonsterItem> _enemyMonsterItemList;
    [SerializeField] protected PlayerHpGaugeItem _playerHpGaugeItem;
    [SerializeField] protected BattleAnimationManager _battleAnimationManager;
    [SerializeField] protected Button _retryButton;
    [SerializeField] protected GameObject _retryButtonBase;

    private bool canTap = true;
    private List<DropItem> selectedDropList = new List<DropItem>();
    private List<CommandMB> commandList = new List<CommandMB>();
    private List<int> activateCommandIndexList = new List<int>();
    //private BattleManager gameManager;

    public override void Init(WindowInfo info)
    {
        _retryButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                _battleAnimationManager.Initialize();
                Refresh();
            })
            .Subscribe();

        Refresh();
    }

    private void Refresh()
    {
        // 初期化
        canTap = false;
        commandList = GameUtil.CreateCommandList();
        _retryButtonBase.SetActive(false);
        _skillActiveObjectList.ForEach(b => b.SetActive(false));
        _board.Initialize(BOARD_MARGIN, DROP_SPACE, MAX_ROW_NUM, COLUMN_NUM).Do(_ => canTap = true).Subscribe();

        // モンスターの初期設定
        var enemyUserMonsterList = new List<UserMonsterInfo>()
        {
            new UserMonsterInfo()
            {
                customData = new UserMonsterCustomData(){
                    hp = 1000,
                    attack = 100,
                }
            },
        };
        var playerUserMonsterList = new List<UserMonsterInfo>()
        {
            new UserMonsterInfo()
            {
                customData = new UserMonsterCustomData(){
                    hp = 100,
                    attack = 100,
                }
            },
            new UserMonsterInfo()
            {
                customData = new UserMonsterCustomData(){
                    hp = 100,
                    attack = 100,
                }
            },
            new UserMonsterInfo()
            {
                customData = new UserMonsterCustomData(){
                    hp = 100,
                    attack = 100,
                }
            },
            new UserMonsterInfo()
            {
                customData = new UserMonsterCustomData(){
                    hp = 100,
                    attack = 100,
                }
            },
            new UserMonsterInfo()
            {
                customData = new UserMonsterCustomData(){
                    hp = 100,
                    attack = 100,
                }
            },
            new UserMonsterInfo()
            {
                customData = new UserMonsterCustomData(){
                    hp = 100,
                    attack = 100,
                }
            },
        };
        _enemyMonsterItemList.ForEach(e => e.Init(1000));

        //gameManager = new BattleManager(this, enemyUserMonsterList, playerUserMonsterList);
        //_playerHpGaugeItem.Init(gameManager.GetPlayerCurrentHp());
    }

    private void Update()
    {
        if (canTap)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            {
                MoveLimitNumText();
                SelectDrop();
            }

            if (Input.GetMouseButtonUp(0))
            {
                canTap = false;
                OnPointerUp();
            }
        }
    }

    // 必要に応じてドロップの選択状態を更新
    private void SelectDrop()
    {
        var drop = _board.GetNearestDrop();
        if (drop == null || drop.GetDropType() == DropType.Disturb) return;

        var isChanged = false; // 選択ドロップリストに変化があったかどうか
        if (!selectedDropList.Contains(drop) && selectedDropList.Count < MAX_SELECTABLE_DROP_NUM)
        {
            // 距離などにより選択不可の場合ははじく
            if (selectedDropList.Any() && !selectedDropList.Last().CanSelect(drop)) return;

            // 未選択状態のドロップなので選択
            selectedDropList.Add(drop);
            drop.ShowGrayOutPanel(true);
            isChanged = true;
        }
        else if(selectedDropList.Count >= 2 && drop == selectedDropList[selectedDropList.Count-2])
        {
            // 1つ前に選択したドロップなら直近のドロップを非選択状態にする
            var targetDrop = selectedDropList.LastOrDefault();
            targetDrop.ShowGrayOutPanel(false);
            selectedDropList.Remove(targetDrop);
            isChanged = true;
        }

        // リストに変化があった時の処理
        if (isChanged)
        {
            // コマンド判定
            var activateCommandIdList = GameUtil.GetActivateCommandIdList(selectedDropList.Select(d => d.GetIndex()).ToList(), commandList);
            activateCommandIndexList.Clear();
            commandList.ForEach((command,index) =>
            {
                var isActivate = activateCommandIdList.Any(id => id == command.id);
                _skillActiveObjectList[index].SetActive(isActivate);
                if(isActivate) activateCommandIndexList.Add(index);
            });

            // 残り個数テキストの制御
            var remainNum = MAX_SELECTABLE_DROP_NUM - selectedDropList.Count();
            if (remainNum > 3)
            {
                _limiNumText.gameObject.SetActive(false);
            }
            else
            {
                _limiNumText.gameObject.SetActive(true);
                _limiNumText.text = $"あと {remainNum}";
            }
        }
    }

    private void MoveLimitNumText()
    {
        float offset = 50.0f;
        var inputPosition = TapPositionManager.Instance.GetLocalPositionFromInput();
        _limiNumText.transform.localPosition = new Vector2(inputPosition.x,inputPosition.y + offset);
    }

    private void OnPointerUp() {
        _limiNumText.gameObject.SetActive(false);

        /*
        _board.DeleteDropObservable(selectedDropList)
            .SelectMany(_ => gameManager.OnEndDropOperationObservable(activateCommandIndexList))
            .SelectMany(winOrLose =>
            {
                switch (winOrLose)
                {
                    case WinOrLose.Win:
                    case WinOrLose.Lose:
                        return _battleAnimationManager.PlayWinOrLoseAnimation(winOrLose)
                            .Do(_ => _retryButtonBase.SetActive(true))
                            .Select(_ => false);
                    case WinOrLose.None:
                    default:
                        return Observable.Return<bool>(true);
                }
            })
            .Where(isOk => isOk)
            .Do(_ => _skillActiveObjectList.ForEach(b => b.SetActive(false)))
            .SelectMany(_ => _board.FillDropObservable())
            .Do(_ => canTap = true)
            .Subscribe();
            */          

        // 初期化
        selectedDropList.Clear();
    }

    /// <summary>
    /// 敵への攻撃アニメーション
    /// </summary>
    public IObservable<Unit> PlayAttackToEnemyAnimationObservable(int enemyMonsterIndex,int enemyMonsterHp,int playerMonsterIndex)
    {
        if (enemyMonsterIndex < 0 || enemyMonsterIndex >= _enemyMonsterItemList.Count) return Observable.ReturnUnit();

        return _enemyMonsterItemList[enemyMonsterIndex].PlayHpGaugeAnimationObservable(enemyMonsterHp);
    }

    /// <summary>
    /// 敵からの攻撃アニメーション
    /// </summary>
    public IObservable<Unit> PlayAttackToPlayerAnimationObservable(int enemyMonsterIndex,int playerHp)
    {
        if (enemyMonsterIndex < 0 || enemyMonsterIndex >= _enemyMonsterItemList.Count) return Observable.ReturnUnit();

        return _playerHpGaugeItem.PlayHpGaugeAnimation(enemyMonsterIndex, playerHp);
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
    }
}