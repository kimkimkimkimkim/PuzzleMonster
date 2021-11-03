using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public static class RectTransformExtensions
    {
        /// <summary>
        /// 上下左右ストレッチのUIのサイズを設定する
        /// </summary>
        public static void SetStretchedRectOffset(this RectTransform rectT, float left, float top, float right, float bottom)
        {
            rectT.offsetMin = new Vector2(left, bottom);
            rectT.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>
        /// 位置を変えずにピボットのみを変更する
        /// </summary>
        public static void SetPivot(this RectTransform self, Vector2 v)
        {
            var size = self.rect.size;
            var deltaPivot = self.pivot - v;
            var deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            self.pivot = v;
            self.localPosition -= deltaPosition;
        }
    }
}

