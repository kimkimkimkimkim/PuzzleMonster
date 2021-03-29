using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using System.Reflection;

namespace GameBase
{
    [ResourcePath("UI/Dialog/Dialog-TestAction")]
    public class TestActionDialogUIScript : DialogBase
    {
        [SerializeField] protected Button _closeButton;
        [SerializeField] protected GameObject _scrollViewContent;

        private List<TestActionData> testActionDataList = new List<TestActionData>();

        public override void Init(DialogInfo info)
        {
            var onClickClose = (Action)info.param["onClickClose"];

            _closeButton.OnClickIntentAsObservable()
                .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
                .Do(_ =>
                {
                    if (onClickClose != null)
                    {
                        onClickClose();
                        onClickClose = null;
                    }
                })
                .Subscribe();

            testActionDataList = GetTestActionDataListFromSubClasses();
            CreateTestActionButton();
        }

        /// <summary>
        /// ITestActionを継承したクラスすべてのActionDataSetListを結合して取得する
        /// </summary>
        private List<TestActionData> GetTestActionDataListFromSubClasses()
        {
            var type = typeof(ITestAction);
            return Assembly.GetAssembly(type).GetTypes()
                .Where(m => m.GetInterface("ITestAction") != null && !m.IsAbstract)
                .Select(t => (ITestAction)Activator.CreateInstance(t))
                .SelectMany(instance => instance.GetTestActionDataList()).ToList();
        }

        /// <summary>
        /// リストの値にしたがってテストアクションボタンを作成
        /// </summary>
        private void CreateTestActionButton()
        {
            testActionDataList.ForEach(testActionData =>
            {
                var testActionScrollItem = UIManager.Instance.CreateContent<TestActionScrollItem>(_scrollViewContent.transform);
                testActionScrollItem.SetTitle(testActionData.title);
                testActionScrollItem.SetOnClickAction(testActionData.action);
            });
        }

        public override void Back(DialogInfo info)
        {
        }
        public override void Close(DialogInfo info)
        {
        }
        public override void Open(DialogInfo info)
        {
        }
    }

    public class TestActionData
    {
        // タイトル
        public string title { get; set; }

        // 実行処理
        public Action action { get; set; }
    }

    public interface ITestAction
    {

        /// <summary>
        /// テストしたい内容をList<ActionDataSet>に追加して返します。
        /// </summary>
        List<TestActionData> GetTestActionDataList();
    }
}
