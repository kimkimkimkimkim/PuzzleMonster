using UnityEditor;
using System.IO;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Item;

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
        
        // NoneアイテムをItemに追加
        allJsonStr += GetNoneItemDataJson() + ",";

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

                    if (key == "id") id = value;
                    if (key == "name") name = value;
                    columnIndex++;
                }

                var jsonStr = GetItemDataJson(masterName, id, name);
                allJsonStr += jsonStr + ",";
                rowIndex++;
            }
        }

        // BundleMBをBundleに追加
        for (var i = 0; i < sheetNum; i++)
        {
            var sheet = book.GetSheetAt(i);
            var masterName = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;
            if (masterName != "BundleMB") continue;

            var rowIndex = START_DATA_ROW_INDEX;

            // 各レコードに対する処理開始
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
            {
                var id = "";
                var name = "";
                var itemList = new List<ItemMI>();

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
                    if (key == "itemList")
                    {
                        var listValue = GetListValueStr(sheet,HIERARCHY_2_ROW_INDEX, rowIndex, columnIndex);
                        listValue = listValue.Replace("\\","");
                        itemList = JsonConvert.DeserializeObject<ItemMI[]>(listValue).ToList();
                    }
                    columnIndex++;
                }

                var jsonStr = GetBundleDataJson(masterName, id, name, itemList);
                allJsonStr += jsonStr + ",";
                rowIndex++;
            }
        }

        // ContainerMBをBundleに追加
        for (var i = 0; i < sheetNum; i++)
        {
            var sheet = book.GetSheetAt(i);
            var masterName = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;
            if (masterName != "ContainerMB") continue;

            var rowIndex = START_DATA_ROW_INDEX;

            // 各レコードに対する処理開始
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
            {
                var id = "";
                var name = "";
                var description = "";
                var itemList = new List<ItemMI>();

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
                    if (key == "description") description = value;
                    if (key == "itemList")
                    {
                        var listValue = GetListValueStr(sheet,HIERARCHY_2_ROW_INDEX, rowIndex, columnIndex);
                        listValue = listValue.Replace("\\", "");
                        itemList = JsonConvert.DeserializeObject<ItemMI[]>(listValue).ToList();
                    }
                    columnIndex++;
                }

                var jsonStr = GetContainerDataJson(masterName, id, name, description, itemList);
                allJsonStr += jsonStr + ",";
                rowIndex++;
            }
        }

        // Catalogの追加終了
        allJsonStr = allJsonStr.Remove(allJsonStr.Length - 1);
        allJsonStr += "],\"DropTables\":[";

        // DropTableMBをDropTableに追加
        for (var i = 0; i < sheetNum; i++)
        {
            var sheet = book.GetSheetAt(i);
            var masterName = sheet.GetRow(NAME_ROW_INDEX).GetCell(NAME_COLUMN_INDEX).StringCellValue;
            if (masterName != "DropTableMB") continue;

            var rowIndex = START_DATA_ROW_INDEX;

            // 各レコードに対する処理開始
            while (GetValueStr(sheet, rowIndex, START_DATA_COLUMN_INDEX) != "")
            {
                var id = "";
                var itemList = new List<ProbabilityItemMI>();

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
                    if (key == "itemList")
                    {
                        var listValue = GetListValueStr(sheet,HIERARCHY_2_ROW_INDEX, rowIndex, columnIndex);
                        listValue = listValue.Replace("\\", "");
                        itemList = JsonConvert.DeserializeObject<ProbabilityItemMI[]>(listValue).ToList();
                    }
                    columnIndex++;
                }

                var jsonStr = GetDropTableDataJson(masterName, id, itemList);
                allJsonStr += jsonStr + ",";
                rowIndex++;
            }
        }

        // DropTableの追加終了
        allJsonStr = allJsonStr.Remove(allJsonStr.Length - 1);
        allJsonStr += "]}";

        var parsedJson = JsonConvert.DeserializeObject(allJsonStr);
        var formattedJsonStr = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        return formattedJsonStr;
    }
    
    private string GetNoneItemDataJson()
    {
        return "{" +
            "\"ItemId\": \"" + "None1" + "\"," +
            "\"ItemClass\": \"" + "None" + "\"," +
            "\"CatalogVersion\": \"" + catalogVersion + "\"," +
            "\"DisplayName\": \"" + "はずれアイテム" + "\"," +
            "\"Description\": null," +
            "\"VirtualCurrencyPrices\": {}," +
            "\"RealCurrencyPrices\": {}," +
            "\"Tags\": []," +
            "\"CustomData\": null," +
            "\"Consumable\": {" +
                "\"UsageCount\": null," +
                "\"UsagePeriod\": 3," +
                "\"UsagePeriodGroup\": null" +
            "}," +
            "\"Container\": null," +
            "\"Bundle\": null," +
            "\"CanBecomeCharacter\": false," +
            "\"IsStackable\": false," +
            "\"IsTradable\": false," +
            "\"ItemImageUrl\": null," +
            "\"IsLimitedEdition\": false," +
            "\"InitialLimitedEditionCount\": 0," +
            "\"ActivatedMembership\": null" +
        "}";
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
            "\"CanBecomeCharacter\": false," +
            "\"IsStackable\": true," +
            "\"IsTradable\": false," +
            "\"ItemImageUrl\": null," +
            "\"IsLimitedEdition\": false," +
            "\"InitialLimitedEditionCount\": 0," +
            "\"ActivatedMembership\": null" +
        "}";
    }

    private string GetBundleDataJson(string masterName, string id,string name, List<ItemMI> itemList)
    {
        var itemClass = masterName.Substring(0, masterName.Length - 2);
        var itemId = $"{itemClass}{id}";

        // bundledItemsの作成
        var bundledItemIdList = new List<string>();
        itemList.Where(m => m.itemType == ItemType.Monster || m.itemType == ItemType.Property).ToList().ForEach(m =>
        {
            var _itemId = ItemUtil.GetItemId(m.itemType, m.itemId);
            for (var i = 0; i < m.num; i++)
            {
                bundledItemIdList.Add($"\"{_itemId}\"");
            }
        });
        var bundledItems = $"[{string.Join(",", bundledItemIdList)}]";

        // bundledResultTablesの作成
        var bundledDropTableIdList = new List<string>();
        itemList.Where(m => m.itemType == ItemType.DropTable).ToList().ForEach(m =>
        {
            var _itemId = ItemUtil.GetItemId(m.itemType, m.itemId);
            for (var i = 0; i < m.num; i++)
            {
                bundledDropTableIdList.Add($"\"{_itemId}\"");
            }
        });
        var bundledResultTables = $"[{string.Join(",", bundledDropTableIdList)}]";

        // bundledVirtualCurrenciesの作成
        var bundledVirtualCurrencyDataList = itemList.Where(m => m.itemType == ItemType.VirtualCurrency).Select(m =>
        {
            var virtualCurrency = (VirtualCurrencyType)m.itemId;
            return $"\"{virtualCurrency}\":{m.num}";
        }).ToList();
        var bundledVirtualCurrencies = bundledVirtualCurrencyDataList.Any() ? $"{{{string.Join(",", bundledVirtualCurrencyDataList)}}}" : "null";

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
                "\"UsageCount\": null," +
                "\"UsagePeriod\": 3," +
                "\"UsagePeriodGroup\": null" +
            "}," +
            "\"Container\": null," +
            "\"Bundle\": {" +
                "\"BundledItems\": "+ bundledItems + "," +
                "\"BundledResultTables\": "+ bundledResultTables +"," +
                "\"BundledVirtualCurrencies\": "+ bundledVirtualCurrencies +
            "}," +
            "\"CanBecomeCharacter\": false," +
            "\"IsStackable\": false," +
            "\"IsTradable\": false," +
            "\"ItemImageUrl\": null," +
            "\"IsLimitedEdition\": false," +
            "\"InitialLimitedEditionCount\": 0," +
            "\"ActivatedMembership\": null" +
        "}";
    }

    private string GetContainerDataJson(string masterName, string id, string name, string description, List<ItemMI> itemList)
    {
        var itemClass = masterName.Substring(0, masterName.Length - 2);
        var itemId = $"{itemClass}{id}";

        // itemContentsの作成
        var itemContentsIdList = new List<string>();
        itemList.Where(m => m.itemType == ItemType.Monster || m.itemType == ItemType.Property).ToList().ForEach(m =>
        {
            var _itemId = ItemUtil.GetItemId(m.itemType, m.itemId);
            for (var i = 0; i < m.num; i++)
            {
                itemContentsIdList.Add($"\"{_itemId}\"");
            }
        });
        var itemContents = $"[{string.Join(",", itemContentsIdList)}]";

        // resultTableContentsの作成
        var resultTableContentsIdList = new List<string>();
        itemList.Where(m => m.itemType == ItemType.DropTable).ToList().ForEach(m =>
        {
            var _itemId = ItemUtil.GetItemId(m.itemType, m.itemId);
            for (var i = 0; i < m.num; i++)
            {
                resultTableContentsIdList.Add($"\"{_itemId}\"");
            }
        });
        var resultTableContents = $"[{string.Join(",", resultTableContentsIdList)}]";

        // virtualCurrencyContentsの作成
        var virtualCurrencyContentsDataList = itemList.Where(m => m.itemType == ItemType.VirtualCurrency).Select(m =>
        {
            var virtualCurrency = (VirtualCurrencyType)m.itemId;
            return $"\"{virtualCurrency}\":{m.num}";
        }).ToList();
        var virtualCurrencyContents = virtualCurrencyContentsDataList.Any() ? $"{{{string.Join(",", virtualCurrencyContentsDataList)}}}" : "null";

        return "{" +
            "\"ItemId\": \"" + itemId + "\"," +
            "\"ItemClass\": \"" + itemClass + "\"," +
            "\"CatalogVersion\": \"" + catalogVersion + "\"," +
            "\"DisplayName\": \"" + name + "\"," +
            "\"Description\": \"" + description + "\"," +
            "\"VirtualCurrencyPrices\": {}," +
            "\"RealCurrencyPrices\": {}," +
            "\"Tags\": []," +
            "\"CustomData\": null," +
            "\"Consumable\": {" +
                "\"UsageCount\": 1," +
                "\"UsagePeriod\": null," +
                "\"UsagePeriodGroup\": null" +
            "}," +
            "\"Container\": {" +
                "\"KeyItemId\": null," +
                "\"ItemContents\": " + itemContents + "," +
                "\"ResultTableContents\": " + resultTableContents + "," +
                "\"VirtualCurrencyContents\": " + virtualCurrencyContents +
            "}," +
            "\"Bundle\": null," +
            "\"CanBecomeCharacter\": false," +
            "\"IsStackable\": false," +
            "\"IsTradable\": false," +
            "\"ItemImageUrl\": null," +
            "\"IsLimitedEdition\": false," +
            "\"InitialLimitedEditionCount\": 0," +
            "\"ActivatedMembership\": null" +
        "}";
    }

    private string GetDropTableDataJson(string masterName, string id, List<ProbabilityItemMI> probabilityItemList)
    {
        var itemClass = masterName.Substring(0, masterName.Length - 2);
        var itemId = $"{itemClass}{id}";

        // nodesの作成
        var probabilityItemDataList = probabilityItemList.Select(m => {
            var _itemType = GetItemType(m);
            var _itemId = ItemUtil.GetItemId(m.itemType, m.itemId);
            return "{" +
                "\"ResultItemType\": \"" + _itemType + "\"," +
                "\"ResultItem\": \"" + _itemId + "\"," +
                "\"Weight\": " + m.weight.ToString() +
            "}";
        });
        var nodes = $"[{string.Join(",", probabilityItemDataList)}]";

        return "{" +
            "\"TableId\": \"" + itemId + "\"," +
            "\"Nodes\": " + nodes +
        "}";
    }
    
    private string GetItemType(ItemMI item){
        switch(item.itemType){
            case ItemType.None:
            case ItemType.Monster:
            case ItemType.Property:
            case ItemType.Bundle:
                return "ItemId";
            case ItemType.DropTable:
                return "TableId";
            default:
                return "";
        }
    }
}
