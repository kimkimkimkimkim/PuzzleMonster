using System;
using UniRx;

public interface IVisualFxItem
{
    IObservable<Unit> PlayFxObservable();
}
