using UnityEditor;
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

/// <summary>
/// 指定パスに存在するExcelファイルを参考にマスターデータ用のJsonを作成しPlayFabにアップロードする
/// </summary>
public class MasterDataPublisher : EditorWindow {
    public static int NAME_ROW_INDEX = 1; // マスタ名が記述されたセルの行インデックス
    public static int NAME_COLUMN_INDEX = 1; // マスタ名が記述されたセルの列インデックス
    public static int HIERARCHY_1_ROW_INDEX = 3; // 階層1が記述されたセルの行インデックス（変数名）
    public static int HIERARCHY_2_ROW_INDEX = 4; // 階層2が記述されたセルの行インデックス（数字だったらリストの要素番号、数字以外だったらオブジェクトの変数名）
    public static int HIERARCHY_3_ROW_INDEX = 5; // 階層3が記述されたセルの行インデックス（数字だったらリストの要素番号、数字以外だったらオブジェクトの変数名）
    public static int TYPE_ROW_INDEX = 6; // 型が記述されたセルの行インデックス
    public static int START_DATA_ROW_INDEX = 9; // データ記述が開始される行インデックス
    public static int START_DATA_COLUMN_INDEX = 1; // データ記述が開始される行インデックス

    private string filePath = $"Assets/Excel/MasterData.xlsx";

    [MenuItem("Tools/MasterDataPublisher")]
    static void Init() {
        var window = GetWindow<MasterDataPublisher>(typeof(MasterDataPublisher));
        window.Show();
    }

    public void OnGUI() {
        if (GUILayout.Button("チェック", new GUILayoutOption[] { })) {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var book = new XSSFWorkbook(fs);

            // シートを取得
            var sheet = book.GetSheetAt(0);

            // マスタ名を取得
            var name = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;

            var rowIndex = START_DATA_ROW_INDEX;
            var jsonStr = "[";
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "") {
                // 各データに対する処理開始
                var columnIndex = 1;
                jsonStr += "\n  {";
                while (GetTypeStr(sheet, columnIndex) != "") {
                    // 各値に対する処理開始
                    var type = GetTypeStr(sheet, columnIndex);

                    // 階層1が空ならスキップ
                    var key1 = GetHierarchyStr(sheet, HIERARCHY_1_ROW_INDEX, columnIndex);
                    if (key1 == "") {
                        columnIndex++;
                        continue;
                    }

                    // 階層1が空じゃないならKeyValue文字列を取得
                    var keyValueStr = GetKeyValueStr(sheet, rowIndex, columnIndex);
                    if (keyValueStr != "") {
                        jsonStr += $"\n    {keyValueStr}";
                        jsonStr += ",";
                    }
                    columnIndex++;
                }
                jsonStr = jsonStr.Remove(jsonStr.Length - 1);
                jsonStr += "\n  },";
                rowIndex++;
            }
            jsonStr = jsonStr.Remove(jsonStr.Length - 1);
            jsonStr += "\n]";

            Debug.Log(jsonStr);
        }
    }

    /// <summary>
    /// セル情報を取得
    /// </summary>
    private ICell GetCell(ISheet sheet, int rowIndex, int columnIndex) {
        return sheet.GetRow(rowIndex).GetCell(columnIndex);
    }

    /// <summary>
    /// 指定したセルの値を文字列で返します
    /// 元々文字列ならダブルクォーテーションをつける
    /// 空なら空文字を返す
    /// </summary>
    private string GetValueStr(ISheet sheet, int rowIndex, int columnIndex) {
        var type = sheet?.GetRow(TYPE_ROW_INDEX)?.GetCell(columnIndex)?.StringCellValue;
        if (type == null) return "";

        var cell = sheet?.GetRow(rowIndex)?.GetCell(columnIndex);
        if (cell.CellType == CellType.Blank) return "";

        return GetValueStr(cell, type);
    }

    /// <summary>
    /// 指定したセルの値を指定した型で変換した値を文字列で返します
    /// 元々文字列ならダブルクォーテーションをつける
    /// 空なら空文字を返す
    /// </summary>
    private string GetValueStr(ICell cell, string type) {
        if (cell == null) return "";

        try {
            switch (type) {
                case "int":
                case "float":
                    return cell.NumericCellValue.ToString();
                case "boolean":
                    return cell.BooleanCellValue.ToString();
                case "string":
                    return $"\"{cell.StringCellValue}\"";
                default:
                    return cell.StringCellValue;
            }
        } catch {
            return "";
        }
    }

    /// <summary>
    /// 指定した列の型名を返します
    /// </summary>
    private string GetTypeStr(ISheet sheet, int columnIndex) {
        var type = sheet?.GetRow(TYPE_ROW_INDEX)?.GetCell(columnIndex)?.StringCellValue ?? "";
        return type;
    }

    /// <summary>
    /// 階層行の値を文字列で返します
    /// </summary>
    private string GetHierarchyStr(ISheet sheet, int rowIndex, int columnIndex) {
        if (rowIndex != HIERARCHY_1_ROW_INDEX && rowIndex != HIERARCHY_2_ROW_INDEX && rowIndex != HIERARCHY_3_ROW_INDEX) return "";

        var cell = sheet?.GetRow(rowIndex)?.GetCell(columnIndex);

        switch (cell.CellType) {
            case CellType.String:
                return $"\"{cell.StringCellValue}\"";
            case CellType.Numeric:
                return $"\"{cell.NumericCellValue.ToString()}\"";
            case CellType.Blank:
            default:
                return "";
        }
    }

    /// <summary>
    /// 指定したセルのKeyValue文字列を返します
    /// 指定する列の階層1の行に値が無い場合は空文字を返します
    /// </summary>
    private string GetKeyValueStr(ISheet sheet, int rowIndex, int columnIndex) {
        // 指定したセルの階層1の行に値が無い場合は空文字を返す
        var key1 = GetHierarchyStr(sheet, HIERARCHY_1_ROW_INDEX, columnIndex);
        if (key1 == "") return "";

        var key2 = GetHierarchyStr(sheet, HIERARCHY_2_ROW_INDEX, columnIndex);
        var cell2 = GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex);
        switch (cell2.CellType) {
            case CellType.Blank:
                // 空ならそのままKeyValue文字列を作成
                var value = GetValueStr(sheet, rowIndex, columnIndex);
                return value != "" ? $"{key1}:{value}" : "";
            case CellType.Numeric:
                // 数値ならリストとして処理
                return "list";
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
    private string GetObjectValueStr(ISheet sheet, int rowIndex, int columnIndex) {
        var cell2 = GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex);
        var cell3 = GetCell(sheet, HIERARCHY_3_ROW_INDEX, columnIndex);

        var hierarchyRowIndex =
            cell2.CellType == CellType.String ? HIERARCHY_2_ROW_INDEX :
            cell3.CellType == CellType.String ? HIERARCHY_3_ROW_INDEX :
            0;
        if (hierarchyRowIndex == 0) return "";

        var jsonStr = "{";
        var lastColumnIndex = sheet.GetRow(TYPE_ROW_INDEX).LastCellNum - 1;
        do {
            var key = GetHierarchyStr(sheet, hierarchyRowIndex, columnIndex);
            var value = GetValueStr(sheet, rowIndex, columnIndex);
            if (value != "") {
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
    private string GetListValueStr(ISheet sheet, int rowIndex, int columnIndex) {
        // 階層2の値が数値でなければ空文字を返す
        if (GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex).CellType != CellType.Numeric) return "";

        var jsonStr = "[";
        var lastColumnIndex = sheet.GetRow(TYPE_ROW_INDEX).LastCellNum - 1;
        do {
            do {

                columnIndex++;
            } while (GetCell(sheet, HIERARCHY_2_ROW_INDEX, columnIndex).CellType == CellType.Blank && columnIndex <= lastColumnIndex);
        } while (GetCell(sheet, HIERARCHY_1_ROW_INDEX, columnIndex).CellType == CellType.Blank && columnIndex <= lastColumnIndex);
        jsonStr = jsonStr.Remove(jsonStr.Length - 1);
        jsonStr += "]";

        return jsonStr;
    }
}