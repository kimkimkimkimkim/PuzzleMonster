using UnityEditor;
using UnityEngine;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;
using System;
using System.Text;

/// <summary>
/// 指定パスに存在するExcelファイルを参考にPlayFabData用のJsonを作成
/// </summary>
public class PlayFabDataPublisher : EditorWindow
{
    public static int NAME_ROW_INDEX = 1; // マスタ名が記述されたセルの行インデックス
    public static int NAME_COLUMN_INDEX = 1; // マスタ名が記述されたセルの列インデックス
    public static int HIERARCHY_1_ROW_INDEX = 3; // 階層1が記述されたセルの行インデックス（変数名）
    public static int HIERARCHY_2_ROW_INDEX = 4; // 階層2が記述されたセルの行インデックス（数字だったらリストの要素番号、数字以外だったらオブジェクトの変数名）
    public static int HIERARCHY_3_ROW_INDEX = 5; // 階層3が記述されたセルの行インデックス（数字だったらリストの要素番号、数字以外だったらオブジェクトの変数名）
    public static int TYPE_ROW_INDEX = 6; // 型が記述されたセルの行インデックス
    public static int START_DATA_ROW_INDEX = 9; // データ記述が開始される行インデックス
    public static int START_DATA_COLUMN_INDEX = 1; // データ記述が開始される行インデックス

    private DateTime date;
    private string filePath = $"Assets/PlayFabData/Excel/Data.xlsm";
    private string outputDirectoryPath(DateTime date) => $"Assets/PlayFabData/Json/{date.ToString("yyyyMMdd_HHmmss")}";
    private string masterDataFileName = "masterData.json";
    private string catalogDataFileName = "catalogData.json";
    private string storeDataFileName = "storeData.json";

    [MenuItem("Tools/PlayFabDataPublisher")]
    static void Init()
    {
        var window = GetWindow<PlayFabDataPublisher>(typeof(PlayFabDataPublisher));
        window.Show();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("マスタデータ出力", new GUILayoutOption[] { }))
        {
            date = DateTime.Now;

            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var book = new XSSFWorkbook(fs);
            var sheetNum = book.NumberOfSheets;

            var allJsonStr = "{";
            for (var i = 0; i < sheetNum; i++)
            {
                // シートを取得
                var sheet = book.GetSheetAt(i);

                // マスタ名を取得
                var name = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;

                var rowIndex = START_DATA_ROW_INDEX;
                var jsonStr = "[";
                while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
                {
                    // 各データに対する処理開始
                    var columnIndex = 1;
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

                allJsonStr += $"\"{name}\":\"{jsonStr}\",";
            }
            allJsonStr = allJsonStr.Remove(allJsonStr.Length - 1);
            allJsonStr += "}";

            var parsedJson = JsonConvert.DeserializeObject(allJsonStr);
            var formattedJsonStr = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

            Directory.CreateDirectory(outputDirectoryPath(date));

            File.WriteAllText($"{outputDirectoryPath(date)}/{masterDataFileName}", formattedJsonStr);

            EditorUtility.DisplayDialog("確認", "Jsonの出力が完了しました", "OK");
        }
    }

    /// <summary>
    /// セル情報を取得
    /// </summary>
    private ICell GetCell(ISheet sheet, int rowIndex, int columnIndex)
    {
        return sheet.GetRow(rowIndex).GetCell(columnIndex);
    }

    /// <summary>
    /// 指定したセルの値を文字列で返します
    /// 元々文字列ならダブルクォーテーションをつける
    /// 空なら空文字を返す
    /// </summary>
    private string GetValueStr(ISheet sheet, int rowIndex, int columnIndex)
    {
        var type = sheet?.GetRow(TYPE_ROW_INDEX)?.GetCell(columnIndex)?.StringCellValue;
        if (type == null) return "";

        var cell = sheet?.GetRow(rowIndex)?.GetCell(columnIndex);
        if (cell == null || cell.CellType == CellType.Blank) return "";

        return GetValueStr(cell, type);
    }

    /// <summary>
    /// 指定したセルの値を指定した型で変換した値を文字列で返します
    /// 元々文字列ならダブルクォーテーションをつける
    /// 空なら空文字を返す
    /// </summary>
    private string GetValueStr(ICell cell, string type)
    {
        if (cell == null) return "";

        try
        {
            switch (type)
            {
                case "int":
                case "float":
                    return cell.NumericCellValue.ToString();
                case "boolean":
                    return cell.BooleanCellValue.ToString().ToLower();
                case "string":
                    return $"\\\"{cell.StringCellValue}\\\"";
                default:
                    return cell.StringCellValue;
            }
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 指定した列の型名を返します
    /// </summary>
    private string GetTypeStr(ISheet sheet, int columnIndex)
    {
        var type = sheet?.GetRow(TYPE_ROW_INDEX)?.GetCell(columnIndex)?.StringCellValue ?? "";
        return type;
    }

    /// <summary>
    /// 階層行の値を文字列で返します
    /// </summary>
    private string GetHierarchyStr(ISheet sheet, int rowIndex, int columnIndex)
    {
        if (rowIndex != HIERARCHY_1_ROW_INDEX && rowIndex != HIERARCHY_2_ROW_INDEX && rowIndex != HIERARCHY_3_ROW_INDEX) return "";

        var cell = sheet?.GetRow(rowIndex)?.GetCell(columnIndex);

        switch (cell.CellType)
        {
            case CellType.String:
                return $"\\\"{cell.StringCellValue}\\\"";
            case CellType.Numeric:
                return $"\\\"{cell.NumericCellValue.ToString()}\\\"";
            case CellType.Blank:
            default:
                return "";
        }
    }

    /// <summary>
    /// 指定したセルのKeyValue文字列を返します
    /// 指定する列の階層1の行に値が無い場合は空文字を返します
    /// </summary>
    private string GetKeyValueStr(ISheet sheet, int rowIndex, int columnIndex)
    {
        // 指定したセルの階層1の行に値が無い場合は空文字を返す
        var key1 = GetHierarchyStr(sheet, HIERARCHY_1_ROW_INDEX, columnIndex);
        if (key1 == "") return "";

        var key2 = GetHierarchyStr(sheet, HIERARCHY_2_ROW_INDEX, columnIndex);
        var cell2 = GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex);
        switch (cell2.CellType)
        {
            case CellType.Blank:
                // 空ならそのままKeyValue文字列を作成
                var value = GetValueStr(sheet, rowIndex, columnIndex);
                return value != "" ? $"{key1}:{value}" : "";
            case CellType.Numeric:
                // 数値ならリストとして処理
                var listValue = GetListValueStr(sheet, rowIndex, columnIndex);
                return listValue != "" ? $"{key1}:{listValue}" : "";
            case CellType.String:
                // 文字列ならオブジェクトとして処理
                var objectValue = GetObjectValueStr(sheet, rowIndex, columnIndex);
                return objectValue != "" ? $"{key1}:{objectValue}" : "";
            default:
                return "";
        }
    }

    /// <summary>
    /// 値の中のオブジェクトをValue文字列として返します
    /// オブジェクトの最初の要素のセルインデックスを指定する
    /// </summary>
    private string GetObjectValueStr(ISheet sheet, int rowIndex, int columnIndex)
    {
        var cell2 = GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex);
        var cell3 = GetCell(sheet, HIERARCHY_3_ROW_INDEX, columnIndex);

        var hierarchyRowIndex =
            cell2.CellType == CellType.String ? HIERARCHY_2_ROW_INDEX :
            cell3.CellType == CellType.String ? HIERARCHY_3_ROW_INDEX :
            0;
        if (hierarchyRowIndex == 0) return "";

        var jsonStr = "{";
        var lastColumnIndex = sheet.GetRow(TYPE_ROW_INDEX).LastCellNum - 1;
        do
        {
            var key = GetHierarchyStr(sheet, hierarchyRowIndex, columnIndex);
            var value = GetValueStr(sheet, rowIndex, columnIndex);
            if (value != "")
            {
                jsonStr += $"{key}:{value},";
            }

            columnIndex++;
        } while (GetCell(sheet, hierarchyRowIndex - 1, columnIndex).CellType == CellType.Blank && columnIndex <= lastColumnIndex);
        jsonStr = jsonStr.Remove(jsonStr.Length - 1);
        jsonStr += "}";

        return jsonStr != "}" ? jsonStr : "";
    }

    /// <summary>
    /// 値の中のリストをValue文字列として返します
    /// リストの最初の要素のセルインデックスを指定する
    /// 現状リストの要素番号は階層2にしか来ない
    /// </summary>
    private string GetListValueStr(ISheet sheet, int rowIndex, int columnIndex)
    {
        // 階層2の値が数値でなければ空文字を返す
        if (GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex).CellType != CellType.Numeric) return "";

        var isObject = GetHierarchyStr(sheet, HIERARCHY_3_ROW_INDEX, columnIndex) != "";

        var jsonStr = "[";
        var lastColumnIndex = sheet.GetRow(TYPE_ROW_INDEX).LastCellNum - 1;
        do
        {
            var elementJsonStr = "";
            do
            {
                var key = GetHierarchyStr(sheet, HIERARCHY_3_ROW_INDEX, columnIndex);
                var value = GetValueStr(sheet, rowIndex, columnIndex);

                if (key == "")
                {
                    // キーが空文字ならオブジェクトじゃない普通の値
                    if (value != "")
                    {
                        elementJsonStr += $"{value},";
                    }
                }
                else
                {
                    // キーが空文字じゃないならオブジェクト
                    if (value != "")
                    {
                        elementJsonStr += $"{key}:{value},";
                    }
                }

                columnIndex++;
            } while (GetCell(sheet, HIERARCHY_1_ROW_INDEX, columnIndex)?.CellType == CellType.Blank && GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex)?.CellType == CellType.Blank && columnIndex <= lastColumnIndex);

            if (elementJsonStr != "")
            {
                elementJsonStr = elementJsonStr.Remove(elementJsonStr.Length - 1);
                if (isObject)
                {
                    elementJsonStr = elementJsonStr.Insert(0, "{");
                    elementJsonStr += "}";
                }
                jsonStr += $"{elementJsonStr},";
            }
        } while (GetCell(sheet, HIERARCHY_1_ROW_INDEX, columnIndex)?.CellType == CellType.Blank && columnIndex <= lastColumnIndex);
        jsonStr = jsonStr.Remove(jsonStr.Length - 1);
        jsonStr += "]";

        return jsonStr != "]" ? jsonStr : "";
    }
}