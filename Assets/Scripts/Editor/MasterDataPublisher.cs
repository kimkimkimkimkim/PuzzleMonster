using UnityEditor;
using System.IO;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;

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

        var allJsonStr = "{";
        for (var i = 0; i < sheetNum; i++)
        {
            // シートを取得
            var sheet = book.GetSheetAt(i);

            // マスタ名を取得
            var name = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;

            // 名前が"無視"だったら無視する
            if (name == "無視") continue;

            var rowIndex = START_DATA_ROW_INDEX;
            var jsonStr = "[";
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
            {
                // 各データに対する処理開始
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

            allJsonStr += $"\"{name}\":\"{jsonStr}\",";
        }
        allJsonStr = allJsonStr.Remove(allJsonStr.Length - 1);
        allJsonStr += "}";

        var parsedJson = JsonConvert.DeserializeObject(allJsonStr);
        var formattedJsonStr = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        return formattedJsonStr;
    }
}