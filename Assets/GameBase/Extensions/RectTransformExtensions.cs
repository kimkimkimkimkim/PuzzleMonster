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
    }
}

