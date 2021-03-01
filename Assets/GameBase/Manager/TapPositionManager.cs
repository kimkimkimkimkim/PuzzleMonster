using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public class TapPositionManager : SingletonMonoBehaviour<TapPositionManager>
    {
        [SerializeField] protected Canvas _canvas;
        [SerializeField] protected RectTransform _canvasRT;

        /// <summary>
        /// クリックした座標(スクリーン座標)からローカル座標(Canvasの中心を原点とする座標)を返します
        /// </summary>
        public Vector2 GetLocalPositionFromInput()
        {
            var camera = _canvas.worldCamera;
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) camera = null;

            var localPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRT, Input.mousePosition, camera, out localPosition);
            return localPosition;
        }

        /// <summary>
        /// クリックした座標(スクリーン座標)からRectTransform平面上のワールド座標(transform.position)を返します
        /// </summary>
        public Vector3 GetWorldPositionFromInput()
        {
            var camera = _canvas.worldCamera;

            // Overlayの場合はScreenPointToWorldPointInRectangleにnullを渡さないといけないので書き換える
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)camera = null;

            // クリック位置に対応するRectTransform上のワールド座標を計算する
            var worldPosition = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRT, Input.mousePosition, camera, out worldPosition);
            return worldPosition;
        }

        /// <summary>
        /// ワールド座標からスクリーン座標を返します
        /// </summary>
        /// <returns>The screen position.</returns>
        /// <param name="worldPosition">World position.</param>
        public Vector3 GetScreenPosition(Vector3 worldPosition)
        {
            // Cameraにnullを入れるとOverlay用のCameraのスクリーン座標が得られる
            return RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, worldPosition);
        }
    }
}
