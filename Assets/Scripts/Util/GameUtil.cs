using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBase;
using System.Linq;

public static class GameUtil
{
    /// <summary>
    /// 指定したインデックスに隣接するインデックスのリストを返します
    /// 盤面範囲外のインデックスも含む
    /// </summary>
    public static List<DropIndex> GetNeighboringIndexList(DropIndex index)
    {
        var lowerLeftIndex = index.column % 2 == 0 ? new DropIndex(index.column - 1, index.row - 1) : new DropIndex(index.column - 1, index.row);
        var upperRightIndex = index.column % 2 == 0 ? new DropIndex(index.column + 1, index.row) : new DropIndex(index.column + 1, index.row + 1);

        var neighboringIndexList = new List<DropIndex>();
        neighboringIndexList.Add(lowerLeftIndex); // 左下
        neighboringIndexList.Add(new DropIndex(lowerLeftIndex.column, lowerLeftIndex.row + 1)); // 左上
        neighboringIndexList.Add(new DropIndex(index.column, index.row + 1)); // 上
        neighboringIndexList.Add(upperRightIndex); // 右上
        neighboringIndexList.Add(new DropIndex(upperRightIndex.column, upperRightIndex.row - 1)); // 右下
        neighboringIndexList.Add(new DropIndex(index.column, index.row - 1)); // 下
        return neighboringIndexList;
    }

    /// <summary>
    /// 選択した座標から発動可能なコマンドのIDを返します
    /// </summary>
    public static List<long> GetActivateCommandIdList(List<DropIndex> selectedIndexList,List<CommandData> commandList)
    {
        var selectedDirectionList = GetDirectionList(selectedIndexList);

        var activateCommandIdList = new List<long>();
        commandList.ForEach(command =>
        {
            if (IsCommandMatch(selectedDirectionList, command)) activateCommandIdList.Add(command.id);
        });

        return activateCommandIdList;
    }

    /// <summary>
    /// 指定した方向リストからコマンドがマッチしたかを返します
    /// </summary>
    public static bool IsCommandMatch(List<Direction> selectedDirectionList,CommandData command)
    {
        var commandDirectionNum = command.directionList.First().Count;
        if (selectedDirectionList.Count < commandDirectionNum) return false;

        for(var i=0; i <= selectedDirectionList.Count - commandDirectionNum; i++)
        {
            for(var j=0; j < command.directionList.Count; j++)
            {
                var isMatch = selectedDirectionList.GetRange(i, commandDirectionNum).SequenceEqual(command.directionList[j]);
                if (isMatch) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 選択したドロップインデックスから方向リストを返します
    /// </summary>
    private static List<Direction> GetDirectionList(List<DropIndex> selectedIndexList)
    {
        var directionList = new List<Direction>();
        selectedIndexList.ForEach((dropIndex,i) =>
        {
            if(i >= 1)
            {
                var previousIndex = selectedIndexList[i - 1];
                var direction = GetDirection(previousIndex, dropIndex);
                directionList.Add(direction);
            }
        });
        return directionList;
    }

    /// <summary>
    /// 2つのインデックスから方向を返します
    /// </summary>
    private static Direction GetDirection(DropIndex beforeIndex, DropIndex afterIndex)
    {
        if(afterIndex.column < beforeIndex.column)
        {
            // 左
            var lowerRowIndex = beforeIndex.column % 2 == 0 ? beforeIndex.row - 1 : beforeIndex.row;
            if(afterIndex.row <= lowerRowIndex)
            {
                return Direction.LowerLeft;
            }
            else
            {
                return Direction.UpperLeft;
            }
        }
        else if(afterIndex.column > beforeIndex.column)
        {
            // 右
            var lowerRowIndex = beforeIndex.column % 2 == 0 ? beforeIndex.row - 1 : beforeIndex.row;
            if (afterIndex.row <= lowerRowIndex)
            {
                return Direction.LowerRight;
            }
            else
            {
                return Direction.UpperRight;
            }
        }
        else
        {
            //　同じ
            if(afterIndex.row > beforeIndex.row)
            {
                return Direction.Up;
            }
            else 
            {
                return Direction.Down;
            }
        }
    }

    /// <summary>
    /// 開発用のコマンドリストを作成する
    /// </summary>
    public static List<CommandData> CreateCommandList()
    {
        var command1 = new CommandData()
        {
            id = 1,
            directionList = new List<List<Direction>>()
            {
                new List<Direction>()
                {
                    Direction.Up,
                    Direction.Up,
                },
                new List<Direction>()
                {
                    Direction.Down,
                    Direction.Down,
                },
            }
        };

        var command2 = new CommandData()
        {
            id = 2,
            directionList = new List<List<Direction>>()
            {
                new List<Direction>()
                {
                    Direction.UpperRight,
                    Direction.UpperRight,
                },
                new List<Direction>()
                {
                    Direction.LowerLeft,
                    Direction.LowerLeft,
                },
            }
        };

        var command3 = new CommandData()
        {
            id = 3,
            directionList = new List<List<Direction>>()
            {
                new List<Direction>()
                {
                    Direction.UpperRight,
                    Direction.LowerRight,
                },
                new List<Direction>()
                {
                    Direction.UpperLeft,
                    Direction.LowerLeft,
                },
            }
        };

        var command4 = new CommandData()
        {
            id = 4,
            directionList = new List<List<Direction>>()
            {
                new List<Direction>()
                {
                    Direction.LowerRight,
                    Direction.UpperRight,
                },
                new List<Direction>()
                {
                    Direction.LowerLeft,
                    Direction.UpperLeft,
                },
            }
        };

        var command5 = new CommandData()
        {
            id = 5,
            directionList = new List<List<Direction>>()
            {
                new List<Direction>()
                {
                    Direction.LowerRight,
                    Direction.Down,
                    Direction.LowerLeft,
                    Direction.UpperLeft,
                    Direction.Up,
                },
                new List<Direction>()
                {
                    Direction.Down,
                    Direction.LowerLeft,
                    Direction.UpperLeft,
                    Direction.Up,
                    Direction.UpperRight,
                },
                new List<Direction>()
                {
                    Direction.LowerLeft,
                    Direction.UpperLeft,
                    Direction.Up,
                    Direction.UpperRight,
                    Direction.LowerRight,
                },
                new List<Direction>()
                {
                    Direction.UpperLeft,
                    Direction.Up,
                    Direction.UpperRight,
                    Direction.LowerRight,
                    Direction.Down,
                },
                new List<Direction>()
                {
                    Direction.Up,
                    Direction.UpperRight,
                    Direction.LowerRight,
                    Direction.Down,
                    Direction.LowerLeft,
                },
                new List<Direction>()
                {
                    Direction.UpperRight,
                    Direction.LowerRight,
                    Direction.Down,
                    Direction.LowerLeft,
                    Direction.UpperLeft,
                },
                new List<Direction>()
                {
                    Direction.LowerLeft,
                    Direction.Down,
                    Direction.LowerRight,
                    Direction.UpperRight,
                    Direction.Up,
                },
                new List<Direction>()
                {
                    Direction.Down,
                    Direction.LowerRight,
                    Direction.UpperRight,
                    Direction.Up,
                    Direction.UpperLeft,
                },
                new List<Direction>()
                {
                    Direction.LowerRight,
                    Direction.UpperRight,
                    Direction.Up,
                    Direction.UpperLeft,
                    Direction.LowerLeft,
                },
                new List<Direction>()
                {
                    Direction.UpperRight,
                    Direction.Up,
                    Direction.UpperLeft,
                    Direction.LowerLeft,
                    Direction.Down,
                },
                new List<Direction>()
                {
                    Direction.Up,
                    Direction.UpperLeft,
                    Direction.LowerLeft,
                    Direction.Down,
                    Direction.LowerRight,
                },
                new List<Direction>()
                {
                    Direction.UpperLeft,
                    Direction.LowerLeft,
                    Direction.Down,
                    Direction.LowerRight,
                    Direction.UpperRight,
                },
            }
        };

        var commandList = new List<CommandData>()
        {
            command1,
            command2,
            command3,
            command4,
            command5,
        };
        return commandList;
    }
}
