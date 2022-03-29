using GameBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class StackableDialogManager : SingletonMonoBehaviour<StackableDialogManager>
{
    private List<StackDialogInfo> stackList = new List<StackDialogInfo>();
    private bool _isCalling;

    public IObservable<T> Push<T>(IObservable<T> observable, int priority, string type = "") 
    {
        return Observable.Create<Unit>(observer => {
            stackList.Add(new StackDialogInfo()
            {
                priority = priority,
                callback = () => {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                },
                type = type
            });
            return Disposable.Empty;
        })
        .SelectMany(_ => observable)
        .Do(_ => _isCalling = false);
    }

    private void PopAndApply()
    {
        var stack = stackList.OrderBy(s => s.priority).FirstOrDefault();
        if (stack != null)
        {
            _isCalling = true;
            stack.callback();
            stackList.Remove(stack);
        }
    }

    public void Call()
    {
        if (Validate()) PopAndApply();
    }

    private bool Validate()
    {
        return !_isCalling && stackList.Count > 0;
    }

    /// <summary>
    /// 指定されたタイプのダイアログがスタックされているかを返します
    /// </summary>
    public bool isStacked(string type)
    {
        return stackList.Any(stack => stack.type == type);
    }

    /// <summary>
    /// 指定されたタイプのスタックをすべて削除します
    /// </summary>
    public void Remove(string type)
    {
        stackList.RemoveAll(stack => stack.type == type);
    }

    private class StackDialogInfo
    {
        public int priority { get; set; }
        public Action callback { get; set; }
        public string type { get; set; }
    }
}