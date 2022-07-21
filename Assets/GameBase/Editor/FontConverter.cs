using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

namespace GameBase
{
    public class FontConverter : EditorWindow
    {
        private RenderType renderType = RenderType.Text;
        private FontConverterView<Font> textFontConverterView;
        private FontConverterView<TMP_FontAsset> tmproFontConverterView;

        [MenuItem("Tools/FontConverter")]
        static void Init()
        {
            GetWindow<FontConverter>("FontConverter");
        }

        public void OnEnable()
        {
            textFontConverterView = new FontConverterView<Font>(GetFoundComponentDictionary<Text>());
            tmproFontConverterView = new FontConverterView<TMP_FontAsset>(GetFoundComponentDictionary<TextMeshProUGUI>());
        }

        private void OnGUI()
        {
            var titleList = new string[Enum.GetNames(typeof(RenderType)).Length];
            for (int i = 0; i < titleList.Length; i++)
            {
                var title = Enum.ToObject(typeof(RenderType), i).ToString();
                titleList[i] = title;
            }
            renderType = (RenderType)GUILayout.Toolbar((int)renderType, titleList, EditorStyles.toolbarButton);

            switch (renderType)
            {
                case RenderType.Text:
                    textFontConverterView.Render();
                    break;
                case RenderType.TextMeshPro:
                    tmproFontConverterView.Render();
                    break;
                default:
                    break;
            }
        }

        private Dictionary<string, List<GameObject>> GetFoundComponentDictionary<T>() where T : MonoBehaviour
        {
            var dict = new Dictionary<string, List<GameObject>>();
            var guids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/Resources/UI" });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var loadAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var key = loadAsset.name;
                SearchComponent<T>(loadAsset, key, dict);
            }

            return dict;
        }

        private void SearchComponent<T>(GameObject obj, string key, Dictionary<string, List<GameObject>> dict) where T : MonoBehaviour
        {
            if (obj.GetComponent<T>())
            {
                if (!dict.ContainsKey(key)) dict[key] = new List<GameObject>();
                dict[key].Add(obj);
            }

            foreach (Transform child in obj.transform)
            {
                SearchComponent<T>(child.gameObject, key, dict);
            }
        }

        private enum RenderType
        {
            Text,
            TextMeshPro
        }
    }

    public class FontConverterView<T> where T : UnityEngine.Object
    {

        private Dictionary<string, List<GameObject>> dict;
        private Vector2 scrollPosition;
        private T selectedFont;

        public FontConverterView(Dictionary<string, List<GameObject>> dict)
        {
            this.dict = dict;
        }

        public void Render()
        {
            if (dict.Count > 0)
            {
                EditorGUILayout.Space();
                selectedFont = (T)EditorGUILayout.ObjectField("設定したいフォント", selectedFont, typeof(T), false);
                if (GUILayout.Button("一括設定"))
                {
                    if (selectedFont == null)
                    {
                        EditorUtility.DisplayDialog("エラー", "フォントが設定されていません", "ok");
                    }
                    else
                    {
                        foreach (var keyValue in dict)
                        {
                            foreach (var obj in keyValue.Value)
                            {
                                SetFont(obj, selectedFont);

                                //変更があったことを記録
                                EditorUtility.SetDirty(obj);
                            }
                        }
                        AssetDatabase.SaveAssets();
                    }
                }

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                foreach (var keyValue in dict)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(keyValue.Key);
                    foreach (var obj in keyValue.Value)
                    {
                        EditorGUILayout.ObjectField(obj.name, GetFont(obj), typeof(T), false);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("指定したコンポーネントがアタッチされたオブジェクトはありません");
            }
        }

        private T GetFont(GameObject obj)
        {
            if (typeof(T) == typeof(Font))
            {
                return obj.GetComponent<Text>().font as T;
            }
            else if (typeof(T) == typeof(TMP_FontAsset))
            {
                return obj.GetComponent<TextMeshProUGUI>().font as T;
            }
            else
            {
                return default(T);
            }
        }

        private void SetFont(GameObject obj, T font)
        {
            if (typeof(T) == typeof(Font))
            {
                obj.GetComponent<Text>().font = font as Font;
            }
            else if (typeof(T) == typeof(TMP_FontAsset))
            {
                obj.GetComponent<TextMeshProUGUI>().font = font as TMP_FontAsset;
            }
        }
    }
}