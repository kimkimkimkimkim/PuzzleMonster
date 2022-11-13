using UnityEditor;

namespace GameBase
{
    [CustomEditor(typeof(ToggleWithValue))]
    public class ToggleWithValueEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 各変数の型通りに表示する
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(RewardAdButton))]
    public class RewardAdButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 各変数の型通りに表示する
            base.OnInspectorGUI();
        }
    }
}