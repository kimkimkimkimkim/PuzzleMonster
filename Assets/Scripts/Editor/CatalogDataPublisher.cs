using UnityEditor;
using System.IO;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 指定パスに存在するExcelファイルを参考にPlayFabData用のJsonを作成
/// </summary>
public partial class PlayFabDataPublisher : EditorWindow
{
    private string catalogVersion = "1.0.0";
    private string catalogDataFileName = "catalogData.json";

    private string GetCatalogDataJson() {
        var fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var book = new XSSFWorkbook(fs);
        var sheetNum = book.NumberOfSheets;

        var allJsonStr = "{\"CatalogVersion\": \""+ catalogVersion + "\",\"Catalog\":[";

        // MonsterMBをItemに追加
        for (var i = 0; i < sheetNum; i++)
        {
            var sheet = book.GetSheetAt(i);
            var masterName = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;
            if (masterName != "MonsterMB") continue;

            var rowIndex = START_DATA_ROW_INDEX;

            // 各レコードに対する処理開始
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
            {
                var id = "";
                var name = "";

                var columnIndex = START_DATA_COLUMN_INDEX;

                // 各カラムに対する処理開始
                while (GetTypeStr(sheet, columnIndex) != "")
                {
                    // 階層1が空ならスキップ
                    var key = GetCellValueStr(sheet, HIERARCHY_1_ROW_INDEX, columnIndex);
                    if (key == "")
                    {
                        columnIndex++;
                        continue;
                    }

                    var value = GetCellValueStr(sheet, rowIndex, columnIndex);

                    if (key == "id") id = value;
                    if (key == "name") name = value;
                    columnIndex++;
                }

                var jsonStr = GetItemDataJson(masterName, id, name);
                allJsonStr += jsonStr + ",";
                rowIndex++;
            }
        }

        // PropertyMBをItemに追加
        for (var i = 0; i < sheetNum; i++)
        {
            var sheet = book.GetSheetAt(i);
            var masterName = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;
            if (masterName != "PropertyMB") continue;

            var rowIndex = START_DATA_ROW_INDEX;

            // 各レコードに対する処理開始
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
            {
                var id = "";
                var name = "";


                var columnIndex = START_DATA_COLUMN_INDEX;

                // 各カラムに対する処理開始
                while (GetTypeStr(sheet, columnIndex) != "")
                {
                    // 階層1が空ならスキップ
                    var key = GetCellValueStr(sheet, HIERARCHY_1_ROW_INDEX, columnIndex);
                    if (key == "")
                    {
                        columnIndex++;
                        continue;
                    }

                    var value = GetCellValueStr(sheet, rowIndex, columnIndex);

                    if (key == "\"id\"") id = value;
                    if (key == "\"name\"") name = value;
                    columnIndex++;
                }

                var jsonStr = GetItemDataJson(masterName, id, name);
                allJsonStr += jsonStr + ",";
                rowIndex++;
            }
        }

        // BundleMBをBundleに追加

        // Catalogの追加終了
        allJsonStr = allJsonStr.Remove(allJsonStr.Length - 1);
        allJsonStr += "],\"DropTables\":[";

        // DropTableMBをDropTableに追加

        // DropTableの追加終了
        // allJsonStr = allJsonStr.Remove(allJsonStr.Length - 1);
        allJsonStr += "]}";

        var parsedJson = JsonConvert.DeserializeObject(allJsonStr);
        var formattedJsonStr = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        return formattedJsonStr;
    }

    private string GetItemDataJson(string masterName, string id,string name)
    {
        var itemClass = masterName.Substring(0, masterName.Length - 2);
        var itemId = $"{itemClass}{id}";
        return "{" +
            "\"ItemId\": \"" + itemId + "\"," +
            "\"ItemClass\": \"" + itemClass + "\"," +
            "\"CatalogVersion\": \"" + catalogVersion + "\"," +
            "\"DisplayName\": \"" + name + "\"," +
            "\"Description\": null," +
            "\"VirtualCurrencyPrices\": {}," +
            "\"RealCurrencyPrices\": {}," +
            "\"Tags\": []," +
            "\"CustomData\": null," +
            "\"Consumable\": {" +
                "\"UsageCount\": 1," +
                "\"UsagePeriod\": null," +
                "\"UsagePeriodGroup\": null" +
            "}," +
            "\"Container\": null," +
            "\"Bundle\": null," +
            "\"CanBecomeCharacter\": true," +
            "\"IsStackable\": true," +
            "\"IsTradable\": false," +
            "\"ItemImageUrl\": null," +
            "\"IsLimitedEdition\": false," +
            "\"InitialLimitedEditionCount\": 0," +
            "\"ActivatedMembership\": null" +
            "}";
    }

}
