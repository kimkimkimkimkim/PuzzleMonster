using UnityEditor;
using UnityEngine.UI;

namespace GameBase
{
    public class ToggleWithValue : Toggle
    {
        public int value;
    }

    [CustomEditor(typeof(ToggleWithValue))]
    public class ToggleWithValueEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 各変数の型通りに表示する
            base.OnInspectorGUI();
        }
    }
}