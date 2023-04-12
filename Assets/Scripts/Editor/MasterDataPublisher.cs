using GameBase;
using UnityEditor;
using System.IO;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PM.Enum.Monster;
using System;
using System.Text.RegularExpressions;

/// <summary>
/// 指定パスに存在するExcelファイルを参考にPlayFabData用のJsonを作成
/// </summary>
public partial class PlayFabDataPublisher : EditorWindow
{
    private string masterDataFileName = "masterData.json";

    private string GetMasterDataJson()
    {
        var fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var book = new XSSFWorkbook(fs);
        var sheetNum = book.NumberOfSheets;

        // 全シートを順番に見ていく
        var allJsonStr = "{";
        for (var i = 0; i < sheetNum; i++)
        {
            // シートを取得
            var sheet = book.GetSheetAt(i);

            // マスタ名を取得
            var name = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;

            // 名前が"無視"だったら無視する
            if (name == "無視") continue;

            // 縦方向の順次処理
            // 一番左のカラム（ID）が指定していなかったら終了
            var rowIndex = START_DATA_ROW_INDEX;
            var jsonStr = "[";
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
            {
                // 横方向の順次処理
                // 型が指定していなかったら終了
                var columnIndex = START_DATA_COLUMN_INDEX;
                jsonStr += "{";
                while (GetTypeStr(sheet, columnIndex) != "")
                {
                    // 各値に対する処理開始
                    var type = GetTypeStr(sheet, columnIndex);

                    // 階層1が空ならスキップ
                    var key1 = GetHierarchyStr(sheet, HIERARCHY_1_ROW_INDEX, columnIndex);
                    if (key1 == "")
                    {
                        columnIndex++;
                        continue;
                    }

                    // 階層1が空じゃないならKeyValue文字列を取得
                    var keyValueStr = GetKeyValueStr(sheet, rowIndex, columnIndex);
                    if (keyValueStr != "")
                    {
                        jsonStr += $"{keyValueStr}";
                        jsonStr += ",";
                    }
                    columnIndex++;
                }
                jsonStr = jsonStr.Remove(jsonStr.Length - 1);
                jsonStr += "},";
                rowIndex++;
            }
            jsonStr = jsonStr.Remove(jsonStr.Length - 1);
            jsonStr += "]";

            // モンスターマスタの場合はアニメーションインデックスリストを取得する
            if (name == "MonsterMB")
            {
                var tempJsonStr = jsonStr.Replace("\\", "");
                var monsterList = JsonConvert.DeserializeObject<List<MonsterMB>>(tempJsonStr);
                monsterList = monsterList.Select(m =>
                {
                    var path = $"{Application.dataPath}/PlayFabData/MonsterSpriteInfo/{m.id}.txt";
                    string text = File.ReadAllText(path);

                    var monsterSpriteDataList = new List<MonsterSpriteDataMI>();
                    var textList = text.Split('\n').ToList();
                    var monsterStateNameList = Enum.GetNames(typeof(MonsterState)).ToList();

                    // Enumの値に加えて個別に追加
                    monsterStateNameList.Add("breathe");

                    var monsterStatePatternList = monsterStateNameList.Select(name => $"(?<name>{name})_(?<index>...).png").ToList();
                    var monsterStatePattern = string.Join("|", monsterStatePatternList);

                    // スプライト情報からステイトや座標などを取得
                    textList.ForEach((text, index) =>
                    {
                        var match = Regex.Match(text, monsterStatePattern, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            var monsterState = GetMonsterState(match.Groups["name"].ToString());
                            var success = int.TryParse(match.Groups["index"].ToString().TrimStart('0'), out int stateIndex);
                            var posAndSizeStr = textList[index + 3];
                            var posAndSizeMatch = Regex.Match(posAndSizeStr, "{{(?<x>.+),(?<y>.+)},{(?<w>.+),(?<h>.+)}}");
                            if (posAndSizeMatch.Success)
                            {
                                var x = int.Parse(posAndSizeMatch.Groups["x"].ToString());
                                var y = int.Parse(posAndSizeMatch.Groups["y"].ToString());
                                var w = int.Parse(posAndSizeMatch.Groups["w"].ToString());
                                var h = int.Parse(posAndSizeMatch.Groups["h"].ToString());
                                var xIndex = x / (w + 1);
                                var yIndex = y / (h + 1);
                                var monsterSpriteData = new MonsterSpriteDataMI()
                                {
                                    xIndex = xIndex,
                                    yIndex = yIndex,
                                    monsterState = monsterState,
                                    stateIndex = stateIndex,
                                };
                                monsterSpriteDataList.Add(monsterSpriteData);
                                m.spriteWidth = w;
                                m.spriteHeight = h;
                            }
                        }
                    });

                    // スプライトのインデックスを割り当てる
                    var indexDataList = monsterSpriteDataList.Select(m => (m.xIndex, m.yIndex, spriteAtlasIndex: 0)).Distinct().OrderBy(d => d.yIndex).ThenBy(d => d.xIndex).ToList();
                    monsterSpriteDataList.ForEach(i =>
                    {
                        var spriteAtlasIndex = indexDataList.FindIndex(d => d.xIndex == i.xIndex && d.yIndex == i.yIndex);
                        i.spriteAtlasIndex = spriteAtlasIndex;
                    });

                    // アニメーションインデックスリストを設定する
                    m.monsterSpriteDataList = monsterSpriteDataList;

                    return m;
                }).ToList();
                jsonStr = JsonConvert.SerializeObject(monsterList);
                jsonStr = jsonStr.Replace("\"", "\\\"");
            }

            allJsonStr += $"\"{name}\":\"{jsonStr}\",";
        }
        allJsonStr = allJsonStr.Remove(allJsonStr.Length - 1);
        allJsonStr += "}";

        var parsedJson = JsonConvert.DeserializeObject(allJsonStr);
        var formattedJsonStr = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        return formattedJsonStr;
    }

    private MonsterState GetMonsterState(string monsterStateName)
    {
        // Enumの値で検索
        foreach (MonsterState state in Enum.GetValues(typeof(MonsterState)))
        {
            if (Regex.IsMatch(state.ToString(), monsterStateName, RegexOptions.IgnoreCase)) return state;
        }

        // Enumの値以外を個別で検索
        if (monsterStateName == "breathe") return MonsterState.Breathing;

        return MonsterState.None;
    }
}