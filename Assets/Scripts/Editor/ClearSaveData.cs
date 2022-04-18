using UnityEditor;
using UnityEngine;

public partial class ClearSaveData : EditorWindow
{
    [MenuItem("Tools/ClearSaveData")]
    static void Init()
    {
        var window = GetWindow<ClearSaveData>(typeof(ClearSaveData));
        window.Show();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("セーブデータ削除", new GUILayoutOption[] { }))
        {
            SaveDataUtil.Clear();
        }
    }
}