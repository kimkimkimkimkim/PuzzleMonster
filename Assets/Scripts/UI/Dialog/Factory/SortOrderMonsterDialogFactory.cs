using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;
using PM.Enum.Monster;
using PM.Enum.SortOrder;

public class SortOrderMonsterDialogFactory
{
    public static IObservable<SortOrderMonsterDialogResponse> Create(SortOrderMonsterDialogRequest request)
    {
        return Observable.Create<SortOrderMonsterDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("initialFilterAttribute", request.initialFilterAttribute);
            param.Add("initialSortOrderType", request.initialSortOrderType);
            param.Add("onClickOk", new Action<List<MonsterAttribute>,SortOrderTypeMonster>((filterAttribute, sortOrderType) => {
                observer.OnNext(new SortOrderMonsterDialogResponse() {
                    dialogResponseType = DialogResponseType.Yes,
                    filterAttribute = filterAttribute,
                    sortOrderType = sortOrderType,
                });
                observer.OnCompleted();
            }));
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new SortOrderMonsterDialogResponse() { dialogResponseType  = DialogResponseType.No });
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<SortOrderMonsterDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}