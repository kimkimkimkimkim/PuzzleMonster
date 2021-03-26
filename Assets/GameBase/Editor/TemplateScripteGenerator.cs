using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

public class TemplateScriptGenerator : EditorWindow
{

    // 作成したスクリプトの出力先を指定（例: Assets/Scripts/Template）
    private string OUTPUT_DIRECTORY_PATH = "Assets/Scripts/UI";

    private UIType[] uiTypeArray = System.Enum.GetValues(typeof(UIType)).Cast<UIType>().ToArray();
    private int selectedIndex;
    private string className;


    [MenuItem("Tools/TemplateScriptGenerator")]
    static void Init()
    {
        var window = GetWindow<TemplateScriptGenerator>(typeof(TemplateScriptGenerator));
        window.Show();
    }

    public void OnGUI()
    {
        // スクリプトタイプを選択
        EditorGUILayout.LabelField("作成したいスクリプトのタイプを選択してください");
        selectedIndex = EditorGUILayout.Popup(selectedIndex, uiTypeArray.Select(t => t.ToString()).ToArray());

        EditorGUILayout.Space();

        // クラス名を入力
        EditorGUILayout.LabelField("クラス名を入力してください");
        EditorGUILayout.LabelField("※Window,Dialogはクラス名にWindowやDialogを含めないように (例:Home, Common)");
        EditorGUILayout.LabelField("※Partsはクラス名全てを入力 (例:MonsterListScrollItem)");
        className = EditorGUILayout.TextField(className);

        EditorGUILayout.Space();

        // スクリプトを作成
        EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(className));
        if (GUILayout.Button("スクリプトを作成", new GUILayoutOption[] { }))
        {
            CreateScript();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CreateScript()
    {
        if (selectedIndex < 0 || selectedIndex >= uiTypeArray.Length) return;

        var uiType = uiTypeArray[selectedIndex];
        switch (uiType)
        {
            case UIType.Window:
            case UIType.Dialog:
                CreateUITemlateScript(uiType, ScriptType.Interface);
                CreateUITemlateScript(uiType, ScriptType.UIScript);
                CreateUITemlateScript(uiType, ScriptType.Factory);
                break;
            case UIType.Parts:
                CreatePartsTemplateScripte();
                break;
            default:
                break;
        }
    }

    private void CreateUITemlateScript(UIType uiType, ScriptType scriptType)
    {
        var uiTypeString = uiType.ToString();
        var scriptTypeString = scriptType.ToString();

        // テンプレートコードの作成
        string templateFilePath = $"System/ScriptTemplate/{uiTypeString}/{scriptTypeString}";
        var textAsset = Resources.Load<TextAsset>(templateFilePath);
        var templateCode = textAsset.text.Replace("#CLASS_NAME#", className);

        // ディレクトリやパスの作成
        var outputFilePath = $"{OUTPUT_DIRECTORY_PATH}/{uiTypeString}/{scriptTypeString}/{className}{uiTypeString}{scriptTypeString}.cs";
        var outputDirectoryPath = Path.GetDirectoryName(outputFilePath);
        CreateDirectory(outputDirectoryPath);
        var assetPath = AssetDatabase.GenerateUniqueAssetPath(outputFilePath);

        // スクリプトを作成
        File.WriteAllText(assetPath, templateCode);
        AssetDatabase.Refresh();
    }

    private void CreatePartsTemplateScripte()
    {
        // テンプレートコードの作成
        string templateFilePath = $"System/ScriptTemplate/Parts/WithAttributes";
        var textAsset = Resources.Load<TextAsset>(templateFilePath);
        var templateCode = textAsset.text.Replace("#CLASS_NAME#", className);

        // ディレクトリやパスの作成
        var outputFilePath = $"{OUTPUT_DIRECTORY_PATH}/Parts/{className}.cs";
        var outputDirectoryPath = Path.GetDirectoryName(outputFilePath);
        CreateDirectory(outputDirectoryPath);
        var assetPath = AssetDatabase.GenerateUniqueAssetPath(outputFilePath);

        // スクリプトを作成
        File.WriteAllText(assetPath, templateCode);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 指定されたパスのフォルダを生成する
    /// </summary>
    /// <param name="path">フォルダパス（例: Assets/Sample/FolderName）</param>
    private static void CreateDirectory(string path)
    {
        var target = "";
        var splitChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        foreach (var dir in path.Split(splitChars))
        {
            var parent = target;
            target = Path.Combine(target, dir);
            if (!AssetDatabase.IsValidFolder(target))
            {
                AssetDatabase.CreateFolder(parent, dir);
            }
        }
    }

    private enum UIType
    {
        Window,
        Dialog,
        Parts,
    }

    private enum ScriptType
    {
        Interface,
        UIScript,
        Factory,
    }
}