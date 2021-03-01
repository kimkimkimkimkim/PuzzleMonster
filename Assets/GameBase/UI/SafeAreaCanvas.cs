using UnityEngine;

namespace GameBase
{
    /// <summary>
    /// canvas直下の〇〇Baseにアタッチすることで
    /// 端末ごとのsafeAreaを考慮したUIサイズに自動で変更します
    /// </summary>
    public class SafeAreaCanvas : MonoBehaviour
    {
        private RectTransform _panel;

        private void Awake()
        {
            _panel = GetComponent<RectTransform>();
            UpdateSafeArea();
        }

        private void UpdateSafeArea()
        {
            var safeAreaRect = Screen.safeArea;
            var anchorMin = safeAreaRect.position;
            var anchorMax = safeAreaRect.position + safeAreaRect.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _panel.anchorMin = anchorMin;
            _panel.anchorMax = anchorMax;
        }
    }
}