using System;
using System.Collections.Generic;

namespace GameBase
{
    public class SampleTestAction : ITestAction
    {
        public List<TestActionData> GetTestActionDataList()
        {
            /*
            var testActionDataList = new List<TestActionData>();

            testActionDataList.Add(new TestActionData()
            {
                title = "サンプル",
                action = new Action(() =>
                {
                    // === 処理 === //
                }),
            });

            return testActionDataList;
            */
            return new List<TestActionData>();           
        }
    }
}
