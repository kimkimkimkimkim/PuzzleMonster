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
    public static List<long> GetActivateCommandIdList(List<DropIndex> selectedIndexList,List<CommandMB> commandList)
    {
        var activateCommandIdList = new List<long>();
        commandList.ForEach(command =>
        {
            if (IsCommandMatch(selectedIndexList, command)) activateCommandIdList.Add(command.id);
        });

        return activateCommandIdList;
    }

    /// <summary>
    /// コマンド発動条件を満たしているかどうかを返します
    /// </summary>
    public static bool IsCommandMatch(List<DropIndex> selectedDropIndexList,CommandMB command)
    {
        if (selectedDropIndexList.Count <= command.directionList.Count) return false;

        for(var i = 0; i < selectedDropIndexList.Count; i++)
        {
            var index = selectedDropIndexList[i];
            for(var j = 0; j < command.directionList.Count; j++)
            {
                var direction = command.directionList[j];
                var targetIndex = GetDropIndex(index, direction);
                var existsTargetIndex = selectedDropIndexList.Any(idx => idx == targetIndex);

                // もし対象のインデックスが存在しなければ即リターン
                if (!existsTargetIndex) break;

                // もしコマンドに必要な全てのインデックスがあった場合はtrueを返す
                if (j == command.directionList.Count - 1 && existsTargetIndex) return true;

                // 基準のインデックスを更新
                index = targetIndex;
            }
        }

        // 一度も条件を満たさずここまで来たらfalseを返す
        return false;
    }


    /// <summary>
    /// 指定したインデックスから指定した方向にあるドロップのインデックスを返します
    /// </summary>
    private static DropIndex GetDropIndex(DropIndex index,Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new DropIndex(index.column, index.row + 1);
            case Direction.Down:
                return new DropIndex(index.column, index.row - 1);
            case Direction.UpperLeft:
                return new DropIndex(index.column - 1, GetUpperRowIndex(index));
            case Direction.UpperRight:
                return new DropIndex(index.column + 1, GetUpperRowIndex(index));
            case Direction.LowerLeft:
                return new DropIndex(index.column - 1, GetLowerRowIndex(index));
            case Direction.LowerRight:
                return new DropIndex(index.column + 1, GetLowerRowIndex(index));
            default:
                return index;
        }
    }

    /// <summary>
    /// 指定したインデックスの右下、左下のインデックスの行インデックスを返します
    /// 負の値になることもある
    /// </summary>
    private static int GetLowerRowIndex(DropIndex index)
    {
        return index.column % 2 == 0 ? index.row - 1 : index.row;
    }

    /// <summary>
    /// 指定したインデックスの右上、左上のインデックスの行インデックスを返します
    /// 盤面の範囲外の値になることもある
    /// </summary>
    private static int GetUpperRowIndex(DropIndex index) 
    {
        return index.column % 2 == 0 ? index.row : index.row + 1;
    }

    /// <summary>
    /// UserMonsterInfoからBattleMonsterInfoを生成して返します
    /// </summary>
    public static BattleMonsterInfo GetBattleMonster(UserMonsterInfo userMonster)
    {
        return new BattleMonsterInfo()
        {
            currentHp = userMonster.hp,
            baseAttack = userMonster.attack,
        };
    }

    /// <summary>
    /// 開発用のコマンドリストを作成する
    /// </summary>
    public static List<CommandMB> CreateCommandList()
    {
        var command1 = new CommandMB()
        {
            id = 1,
            directionList = new List<Direction>(){
                Direction.Up,
                Direction.Up,
            },
        };

        var command2 = new CommandMB()
        {
            id = 2,
            directionList = new List<Direction>(){
                Direction.UpperRight,
                Direction.LowerRight,
            },
        };

        var command3 = new CommandMB()
        {
            id = 3,
            directionList = new List<Direction>(){
                Direction.LowerRight,
                Direction.UpperRight,
            },
        };

        var command4 = new CommandMB()
        {
            id = 4,
            directionList = new List<Direction>(){
                Direction.UpperRight,
                Direction.UpperRight,
            },
        };

        var command5 = new CommandMB()
        {
            id = 5,
            directionList = new List<Direction>(){
                Direction.UpperLeft,
                Direction.UpperLeft,
            },
        };

        var command6 = new CommandMB()
        {
            id = 6,
            directionList = new List<Direction>(){
                Direction.LowerRight,
                Direction.Down,
                Direction.LowerLeft,
                Direction.UpperLeft,
                Direction.Up,
            },
        };

        var commandList = new List<CommandMB>()
        {
            command1,
            command2,
            command3,
            command4,
            command5,
            command6,
        };
        return commandList;
    }
}
