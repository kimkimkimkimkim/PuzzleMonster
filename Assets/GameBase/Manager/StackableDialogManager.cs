using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace GameBase
{
    public class StackableDialogManager: SingletonMonoBehaviour<StackableDialogManager>
    {
        private List<StackDialogInfo> stackList = new List<StackDialogInfo>();
        private bool isCalling;

        public IObservable<T> Push<T>(IObservable<T> observable, int priority, string type = "") where T : IStackableDialogResponse
        {
            var stack = new StackDialogInfo()
            {
                priority = priority,
                type = type
            };

            return Observable.Create<Unit>(observer => {
                stack.callback = () => {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                };
                stackList.Add(stack);
                return Disposable.Empty;
            })
                .SelectMany(_ => observable)
                .Do(_ => {
                    isCalling = false;
                    stackList.Remove(stack);
                });
        }

        private void PopAndApply()
        {
            var stack = stackList.OrderBy(s => s.priority).FirstOrDefault();
            if (stack != null)
            {
                isCalling = true;
                stack.callback();
            }
        }

        public void Call()
        {
            if (Validate()) PopAndApply();
        }

        public bool IsCalling()
        {
            return isCalling;
        }

        private bool Validate()
        {
            return !isCalling && stackList.Count > 0;
        }

        /// <summary>
        /// 指定されたタイプのダイアログがスタックされているかを返します
        /// </summary>
        public bool IsStacked(string type)
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
}