using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;

public class BoardItem : MonoBehaviour
{
    private const float DROP_FADE_IN_ANIMATION_TIME = 0.2f;
    private const float DROP_MOVE_ANIMATION_TIME = 0.5f;
    private const Ease DROP_MOVE_ANIMATION_EASE = Ease.InOutSine;

    [SerializeField] RectTransform _boardRT;

    private float boardHeight;
    private float boardWidth;
    private float boardMargin;
    private float dropSpace;
    private int maxRowNum;
    private int columnNum;

    private int totalDropNum;
    private float dropBaseHeight;
    private float dropBaseWidth;
    private float dropHeight;
    private float dropWidth;

    private List<List<DropItem>> dropList = new List<List<DropItem>>();

    public IObservable<Unit> Initialize(float boardMargin,float dropSpace,int maxRowNum, int columnNum)
    {
        // ボードパラメータの取得・設定
        boardHeight = _boardRT.rect.height;
        boardWidth = _boardRT.rect.width;
        this.boardMargin = boardMargin;
        this.dropSpace = dropSpace;
        this.maxRowNum = maxRowNum;
        this.columnNum = columnNum;

        // ドロップパラメータの計算・設定
        dropBaseHeight = CalculateDropBaseHeight();
        dropBaseWidth = CalculateDropBaseWidth();
        dropHeight = CalculateDropHeight();
        dropWidth = CalculateDropWidth();

        // ドロップの生成
        InitializeList();

        var observableList = new List<IObservable<Unit>>();
        for (var c = 0; c < columnNum; c++)
        {
            var row = GetRowNum(c);
            for (var r = 0; r < row; r++) {
                observableList.Add(CreateDropObservable(new DropIndex(c, GetStartRowIndexWhenCreate(c) + r), new DropIndex(c, r)));
            }
        }

        return Observable.WhenAll(observableList);
    }

    #region Setting
    private float CalculateDropBaseHeight()
    {
        return (boardHeight - (2 * boardMargin)) / maxRowNum;
    }

    private float CalculateDropBaseWidth()
    {
        return (2 * (boardWidth - (2 * boardMargin))) / (columnNum + 1);
    }

    private float CalculateDropHeight()
    {
        return dropBaseHeight - (2 * (dropSpace / 2));
    }

    private float CalculateDropWidth()
    {
        return dropHeight;
    }

    // 盤面の左下から上方向にindexが進む
    // 盤面のPivotは左下
    private Vector2 GetDropPosition(int index)
    {
        var rIndex = GetRowIndex(index); // 下から数えたインデックス
        var cIndex = GetColumnIndex(index); // 左から数えたインデックス

        return GetDropPosition(cIndex, rIndex);
    }

    private Vector2 GetDropPosition(int cIndex,int rIndex)
    {
        var x = boardMargin + (dropBaseWidth / 2) + ((dropBaseWidth / 2) * cIndex);
        var y = cIndex % 2 == 0 ? boardMargin + (dropBaseHeight / 2) + dropBaseHeight * rIndex : boardMargin + dropBaseHeight + dropBaseHeight * rIndex;
        return new Vector2(x, y);
    }

    private int GetRowIndex(int index)
    {
        var row = maxRowNum;
        while (index >= row)
        {
            index -= row;
            row = row == maxRowNum ? maxRowNum - 1 : maxRowNum;
        }
        return index;
    }

    private int GetColumnIndex(int index)
    {
        var cIndex = 0;
        var row = maxRowNum;
        while (index >= row)
        {
            cIndex++;
            index -= row;
            row = row == maxRowNum ? maxRowNum - 1 : maxRowNum;
        }
        return cIndex;
    }

    private int GetRowNum(int columnIndex)
    {
        return columnIndex % 2 == 0 ? maxRowNum : maxRowNum - 1;
    }

    // 新規作成時の最小行インデックス
    private int GetStartRowIndexWhenCreate(int columnIndex)
    {
        var offset = 1;
        return GetRowNum(columnIndex) + offset;
    }

    // 盤面のドロップの個数に応じて要素defaultのリストを作成
    private void InitializeList()
    {
        dropList.Clear();

        for(var c = 0; c < columnNum; c++)
        {
            dropList.Add(new List<DropItem>());

            var row = GetRowNum(c);
            for(var r = 0; r < row; r++)
            {
                dropList[c].Add(default(DropItem));
            }
        }

    }

    private IObservable<Unit> CreateDropObservable(DropIndex beforeIndex,DropIndex afterIndex) {
        var drop = UIManager.Instance.CreateContent<DropItem>(_boardRT);
        var beforePosition = GetDropPosition(beforeIndex.column, beforeIndex.row);
        var afterPosition = GetDropPosition(afterIndex.column, afterIndex.row);

        drop.GetComponent<RectTransform>().sizeDelta = new Vector2(dropWidth, dropHeight);
        drop.SetInfo(afterIndex);
        drop.GetCanvasGroup().DOFade(0, 0).OnCompleteAsObservable().Subscribe();
        drop.transform.localPosition = beforePosition;

        dropList[afterIndex.column][afterIndex.row] = drop;

        return DOTween.Sequence()
            .Append(drop.GetCanvasGroup().DOFade(1, DROP_FADE_IN_ANIMATION_TIME))
            .Append(drop.transform.DOLocalMove(afterPosition,DROP_MOVE_ANIMATION_TIME).SetEase(DROP_MOVE_ANIMATION_EASE))
            .OnCompleteAsObservable()
            .AsUnitObservable();
    }

    private void CreateDrop(DropIndex index)
    {
        var drop = UIManager.Instance.CreateContent<DropItem>(_boardRT);
        var position = GetDropPosition(index.column, index.row);

        drop.GetComponent<RectTransform>().sizeDelta = new Vector2(dropWidth, dropHeight);
        drop.transform.localPosition = position;
        drop.SetInfo(index);

        dropList[index.column][index.row] = drop;
    }
    #endregion

    #region Game
    public DropItem GetNearestDrop()
    {
        var thresholdDistance = 0.3f;
        var minDistance = float.MaxValue;
        DropItem nearestDrop = null;

        // 入力値と盤面のピース位置との距離を計算し、一番距離が短いピースを探す
        dropList.ForEach(list =>
        {
            list.ForEach(drop =>
            {
                var inputWorldPosition = TapPositionManager.Instance.GetWorldPositionFromInput();
                var distance = Vector3.Distance(inputWorldPosition, drop.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestDrop = drop;
                }
            });
        });

        // 一番距離の短いピースが閾値より遠い場合はnullを返す
        if (minDistance > thresholdDistance) return null;

        return nearestDrop;
    }

    /// <summary>
    /// 選択したドロップを元に削除するお邪魔ドロップを返す
    /// </summary>
    public List<DropIndex> GetDeleteDisturbDropIndexList(List<DropIndex> selectedDropIndexList) {
        var deleteDisturbDropIndexList = new List<DropIndex>();

        // 選択したドロップに隣接するお邪魔ドロップをリストに追加
        selectedDropIndexList.ForEach(index =>
        {
            var neighboringIndexList = GameUtil.GetNeighboringIndexList(index)
                .Where(neighboringIndex =>
                {
                    // 盤面範囲外のインデックスは排除
                    if (neighboringIndex.column < 0) return false;
                    if (neighboringIndex.column >= columnNum) return false;
                    if (neighboringIndex.row < 0) return false;
                    if (neighboringIndex.row >= GetRowNum(neighboringIndex.column)) return false;
                    return true;
                }).ToList();

            neighboringIndexList.ForEach(neighboringIndex =>
            {
                var neighboringDrop = dropList[neighboringIndex.column][neighboringIndex.row];
                var isDisturb = neighboringDrop.GetDropType() == DropType.Disturb;

                // お邪魔ドロップかつリストに含まれていなければリストに追加
                if(isDisturb && !deleteDisturbDropIndexList.Any(deleteDisturbDropIndex => deleteDisturbDropIndex == neighboringIndex))
                {
                    deleteDisturbDropIndexList.Add(neighboringIndex);
                }
            });
        });

        return deleteDisturbDropIndexList;
    }

    // ドロップオブジェクトを削除しリスト内の該当のデータをnullにする
    public IObservable<Unit> DeleteDropObservable(List<DropItem> selectedDropList) 
    {
        var span = 0.1f;
        var wholeDeleteDropIndexList = new List<DropIndex>();
        return Observable.WhenAll(selectedDropList.Select((drop, index) =>
        {
            var deleteDropList = new List<DropItem>() { drop };
            var neighboringIndexList = GameUtil.GetNeighboringIndexList(drop.GetIndex()).Where(neighboringIndex =>
                {
                    // 盤面範囲外のインデックスは排除
                    if (neighboringIndex.column < 0) return false;
                    if (neighboringIndex.column >= columnNum) return false;
                    if (neighboringIndex.row < 0) return false;
                    if (neighboringIndex.row >= GetRowNum(neighboringIndex.column)) return false;
                    return true;
                }).ToList();
            neighboringIndexList.ForEach(neighboringIndex =>
            {
                var neighboringDrop = dropList[neighboringIndex.column][neighboringIndex.row];
                if(neighboringDrop.GetDropType() == DropType.Disturb && !wholeDeleteDropIndexList.Any(i => i == neighboringDrop.GetIndex()))
                {
                    deleteDropList.Add(neighboringDrop);
                    wholeDeleteDropIndexList.Add(neighboringIndex);
                }
            });

            return Observable.Timer(TimeSpan.FromSeconds(span * index))
                .Do(_ =>
                {
                    // nullにする
                    deleteDropList.ForEach(d =>
                    {
                        dropList[d.GetIndex().column][d.GetIndex().row] = null;
                    });
                })
                .SelectMany(_ => {
                    // アニメーション再生
                    return Observable.WhenAll(deleteDropList.Select(d =>
                    {
                        return d.PlayDeleteAnimationObservable();
                    }));
                });
        }));
    }


    // 削除したドロップを埋めるように新たにドロップを作成
    public IObservable<Unit> FillDropObservable()
    {
        var dropMoveList = new List<DropMoveInfo>();
        var deletedDropNumList = new List<int>();

        // 詰める
        dropList.ForEach((list, c) =>
        {
            var deletedDropNum = 0;
            list.ForEach((drop, r) =>
            {
                if(drop == null)
                {
                    deletedDropNum++;
                }
                else if (deletedDropNum != 0)
                {
                    // どの位置に移動させるかの情報をリストに追加
                    dropMoveList.Add(new DropMoveInfo(new DropIndex(c, r), new DropIndex(c, r - deletedDropNum)));
                }
            });
            deletedDropNumList.Add(deletedDropNum);
        });

        // 埋める
        deletedDropNumList.ForEach((n,c) =>
        {
            if (n > 0)
            {
                var rowNum = GetRowNum(c);
                for (var r = 0; r < n; r++)
                {
                    dropMoveList.Add(new DropMoveInfo(new DropIndex(c, GetStartRowIndexWhenCreate(c) + r), new DropIndex(c, rowNum - n + r)));
                }
            }
        });

        return Observable.WhenAll(dropMoveList.Select(dropMove =>
        {
            if (dropMove.beforeIndex.row >= GetStartRowIndexWhenCreate(dropMove.beforeIndex.column))
            {
                // 新規作成
                return CreateDropObservable(dropMove.beforeIndex, dropMove.afterIndex);
            }
            else
            {
                // 元々あるドロップの移動
                var drop = dropList[dropMove.beforeIndex.column][dropMove.beforeIndex.row];
                var position = GetDropPosition(dropMove.afterIndex.column, dropMove.afterIndex.row);
                drop.RefreshIndex(dropMove.afterIndex);
                dropList[dropMove.afterIndex.column][dropMove.afterIndex.row] = drop;

                // 元々いた場所をnullにする
                dropList[dropMove.beforeIndex.column][dropMove.beforeIndex.row] = null;

                return DOTween.Sequence().SetDelay(DROP_FADE_IN_ANIMATION_TIME)
                    .Append(drop.transform.DOLocalMove(position, DROP_MOVE_ANIMATION_TIME).SetEase(DROP_MOVE_ANIMATION_EASE))
                    .OnCompleteAsObservable()
                    .AsUnitObservable();
            }
        }));
    }

    private struct DropMoveInfo
    {
        public DropIndex beforeIndex;
        public DropIndex afterIndex;

        public DropMoveInfo(DropIndex beforeIndex,DropIndex afterIndex)
        {
            this.beforeIndex = beforeIndex;
            this.afterIndex = afterIndex;
        }
    }


    #endregion
}
