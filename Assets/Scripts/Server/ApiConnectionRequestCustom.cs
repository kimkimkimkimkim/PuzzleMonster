using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayFab.Json;
using PM.Enum.Data;
using PM.Enum.Item;
using UniRx;

public partial class ApiConnection
{
    public static IObservable<ExecuteGachaApiResponse> ExecuteGacha(ExecuteGachaApiRequest request)
    {
        // 通貨が足りるかチェック
        var gachaBoxDetail = MasterRecord.GetMasterOf<GachaBoxDetailMB>().Get(request.gachaBoxDetailId);
        var isEnough = gachaBoxDetail.requiredItemList.All(item =>
        {
            // 通貨
            if(item.itemType == ItemType.VirtualCurrency) {
                switch (item.itemId)
                {
                    case (long)VirtualCurrencyType.OB:
                        return ApplicationContext.userVirtualCurrency.orb >= item.num;
                    case (long)VirtualCurrencyType.CN:
                        return ApplicationContext.userVirtualCurrency.coin >= item.num;
                    default:
                        return false;
                }
            }

            return true;
        });
        if(!isEnough) return Observable.Return<ExecuteGachaApiResponse>(new ExecuteGachaApiResponse() { isSuccess = false });

        // 対象モンスターを取得
        var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().Where(m =>
        {
            // モンスターのガチャボックスタイプが１つでもガチャのガチャボックスタイプに含まれていたら対象モンスター
            return m.gachaBoxTypeList.Any(t => gachaBoxDetail.gachaBoxTypeList.Contains(t));
        }).ToList();
        var rewardMonsterList = new List<MonsterMB>();
        var seed = Environment.TickCount;
        for (var i = 0; i < gachaBoxDetail.dropNum; i++)
        {
            //シード値を変えながらRandomクラスのインスタンスを作成する
            var rnd = new Random(seed++);
            var monster = monsterList[rnd.Next(0, monsterList.Count)];
            rewardMonsterList.Add(monster);
        }

        return GetUserData()
            .SelectMany(res =>
            {
                var userMonsterList = res.userMonsterList;

                // 更新するユーザーデータの作成
                // TODO : かぶったらカケラを付与するように
                userMonsterList.AddRange(ItemUtil.GetUserMonsterList(rewardMonsterList));
                return UpdateUserData(new Dictionary<UserDataKey, object>() { { UserDataKey.userMonsterList, userMonsterList } });
            })
            .Select(_ => {
                // モンスターマスタリストからItemMIリストに変換
                var rewardItemList = rewardMonsterList.Select(m => new ItemMI()
                {
                    itemType = ItemType.Monster,
                    itemId = m.id,
                    num = 1,
                }).ToList();
                return new ExecuteGachaApiResponse()
                {
                    isSuccess = true,
                    rewardItemList = rewardItemList
                };
            });
    }
}
