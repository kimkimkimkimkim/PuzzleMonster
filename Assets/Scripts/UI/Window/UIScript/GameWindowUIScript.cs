using System.Collections.Generic;
using System.Linq;
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

    private bool canTap = true;
    private List<DropItem> selectedDropList = new List<DropItem>();
    private List<CommandData> commandList = new List<CommandData>();
    private List<CommandData> activateCommandList = new List<CommandData>();

    public override void Init(WindowInfo info)
    {
        canTap = false;
        commandList = GameUtil.CreateCommandList();
        _board.Initialize(BOARD_MARGIN,DROP_SPACE,MAX_ROW_NUM,COLUMN_NUM).Do(_ => canTap = true).Subscribe();
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
            commandList.ForEach((command,index) =>
            {
                var isActivate = activateCommandIdList.Any(id => id == command.id);
                _skillActiveObjectList[index].SetActive(isActivate);
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

        _board.DeleteDropObservable(selectedDropList)
            .SelectMany(_ => _board.FillDropObservable())
            .Do(_ => canTap = true)
            .Subscribe();

        selectedDropList.Clear();
        _skillActiveObjectList.ForEach(b => b.SetActive(false));
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