using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;

namespace GameBase
{
    public class InfiniteScroll : UIBehaviour
    {
        private const float DEF_MAX_FRAME_RATE_TIME = 1f / 60f;
        private const float MIN_ANIMATION_RATIO = 0.1f;
        private const float MAX_ANIMATION_RATIO = 0.75f;
        private const float DEF_ANIMATION_RATIO = 0.3f;
        private const float ANIMATION_SKIP_POINT = 0.1f;
        private const float SCROLL_UNLOCK_POINT = 50f;
        private const float SCREEN_OUT_POINT = 1280f;
        private const float STOP_SCROLL_TRIGGER_INSENSETIVITY = 0.1f * 0.1f;

        [System.NonSerialized]
        public LinkedList<RectTransform> itemList = new LinkedList<RectTransform>();

        #region inspector properties

        public RectTransform itemPrototype;

        //表示するアイテム最大行数
        public int lineCount = 9;

        public int inlineItemCount = 1;
        public Direction direction;

        public float firstItemMargin;
        public float linePadding;
        public float inlineItemPadding;
        public float lastItemMargin;

        #endregion inspector properties

        #region animation

        public bool enableAnimation = true;
        public AnimationType animationType;
        public AnimationStartWith animationStartType;

        [Range(MIN_ANIMATION_RATIO, MAX_ANIMATION_RATIO)]
        public float animationSpeed = DEF_ANIMATION_RATIO;

        #endregion animation

        /// <summary>
        /// 以前のフレームとの位置差
        /// </summary>
        protected float _diffPreFramePosition = 0;

        /// <summary>
        /// 一番前のIndex
        /// </summary>
        public int FirstItemIndex { get; private set; }

        /// <summary>
        /// 最後のIndex
        /// </summary>
        public int LastItemIndex { get; private set; }

        private Action<int, GameObject> _onUpdateItem;
        private int _maxDataCount;

        public enum Direction
        {
            Vertical,
            Horizontal,
        }

        public enum AnimationType
        {
            CrossSide,
            SameSide,
        }

        public enum AnimationStartWith
        {
            FirstItem,
            LastItem
        }

        private RectTransform _rectTransform;

        protected RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private float anchoredPosition
        {
            get { return direction == Direction.Vertical ? -rectTransform.anchoredPosition.y : rectTransform.anchoredPosition.x; }
        }

        private float _lineSpace = -1;

        /// <summary>
        /// ラインの座標
        /// </summary>
        public float lineSpace
        {
            get
            {
                if (itemPrototype != null && !isCalculatedLineSpace)
                {
                    var value = (direction == Direction.Vertical ? itemPrototype.sizeDelta.y : itemPrototype.sizeDelta.x) + linePadding;
                    if (value <= 0)
                    {
                        Debug.Log("スクロールのlineSpaceは0以下になっています。");
                        value = 1f;
                    }
                    _lineSpace = value;
                    isCalculatedLineSpace = true;
                }
                return _lineSpace;
            }
        }

        private ScrollRect _scrollRect;
        private float _inlineItemSpace = -1;
        private float _animatableDistance;

        private Vector3 _oldPosition;
        private bool isCalculatedInlineItemSpace;
        private bool isCalculatedLineSpace;
        /// <summary>
        /// プレハブを更新した際にSpace再計算するための初期化処理
        /// </summary>
        public void resetSpace()
        {
            isCalculatedLineSpace = false;
            isCalculatedInlineItemSpace = false;
        }

        /// <summary>
        /// ライン内のアイテムの座標
        /// </summary>
        public float inlineItemSpace
        {
            get
            {
                if (itemPrototype != null && !isCalculatedInlineItemSpace)
                {
                    var value = (direction == Direction.Vertical ? itemPrototype.sizeDelta.x : itemPrototype.sizeDelta.y) + inlineItemPadding;
                    if (value <= 0)
                    {
                        Debug.Log("スクロールのinlineItemSpaceは0以下になっています。");
                        value = 1f;
                    }
                    _inlineItemSpace = value;
                    isCalculatedInlineItemSpace = true;
                }
                return _inlineItemSpace;
            }
        }

        public bool IsStopped { get; private set; }

        /// <summary>
        /// スクロール初期化。
        /// </summary>
        /// <param name="maxDataCount">最大データ数</param>
        /// <param name="updator">レンダー可能な座標にセットされた時呼ばれるAction</param>
        /// <param name="updator">レンダー可能な座標にセットされた時呼ばれるAction</param>
        /// <param name="isStartPosReversal">スクロール生成時、スクロール表示時点を反対へ更新（vertical : 下、horizontal : 右）</param>
        public void Init(int maxDataCount, Action<int, GameObject> updator, bool isStartPosReversal = false)
        {
            _onUpdateItem = updator;
            const float anchorFixedAxisValue = 0.5f;
            const float anchorMovingAxisValue = 1.0f;
            Vector2 anchor = direction == Direction.Horizontal ?
                new Vector2(0, anchorMovingAxisValue) :
                new Vector2(anchorFixedAxisValue, anchorMovingAxisValue);

            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;

            _scrollRect = transform.parent.GetComponent<ScrollRect>();
            if (_scrollRect != null)
            {
                _scrollRect.horizontal = direction == Direction.Horizontal;
                _scrollRect.vertical = direction == Direction.Vertical;
                _scrollRect.content = rectTransform;
                _scrollRect.enabled = enableAnimation ? false : true;
            }

            if (enableAnimation)
            {
                _animatableDistance = SCREEN_OUT_POINT;
            }
            else
            {
                _animatableDistance = 0f;
            }

            //アイテムコンテナのサイズ調整
            ChangeMaxDataCount(maxDataCount);

            if (_scrollRect != null && isStartPosReversal)
            {
                if (direction == Direction.Vertical)
                {
                    _scrollRect.verticalNormalizedPosition = 0;
                }
                else
                {
                    _scrollRect.horizontalNormalizedPosition = 0;
                }
            }

            //新しいGameObject生成
            for (int i = 0; i < lineCount * inlineItemCount; i++)
            {
                var item = Instantiate(itemPrototype, transform, false) as RectTransform;
                item.anchorMin = anchor;
                item.anchorMax = anchor;
                item.name = i.ToString();

                item.anchoredPosition = CalcItemPosition(i, i);
                itemList.AddLast(item);

                _onUpdateItem(i, item.gameObject);
                RestrictScroll(i, item.gameObject);
            }

            FirstItemIndex = 0;
            LastItemIndex = lineCount * inlineItemCount;

            IsStopped = true;
        }

        /// <summary>
        /// 最大アイテム数を調整し、その数合わせコンテナのサイズを調整する
        /// </summary>
        /// <param name="maxDataCount"></param>
        public float ChangeMaxDataCount(int maxDataCount)
        {
            _maxDataCount = maxDataCount;
            var sizeDelta = rectTransform.sizeDelta;
            var lineCount = maxDataCount / inlineItemCount;
            if (maxDataCount % inlineItemCount > 0) lineCount++;
            if (direction == Direction.Vertical)
            {
                sizeDelta.y = lineSpace * lineCount + firstItemMargin + lastItemMargin;
            }
            else
            {
                sizeDelta.x = lineSpace * lineCount + firstItemMargin + lastItemMargin;
            }
            rectTransform.sizeDelta = sizeDelta;

            transform.hasChanged = true;

            return rectTransform.sizeDelta.y;
        }

        /// <summary>
        /// スクロールの設定、アイテムを削除。
        /// </summary>
        public void Clear()
        {
            _diffPreFramePosition = 0;
            FirstItemIndex = 0;

            //既存のGameObject削除
            foreach (var item in itemList)
                Destroy(item.gameObject);
            itemList.Clear();
        }

        void Update()
        {
            if (transform.hasChanged)
            {
                //下から出現するアイテムを更新
                while (anchoredPosition - _diffPreFramePosition < -lineSpace * 2)
                {
                    _diffPreFramePosition -= lineSpace;

                    for (var i = 0; i < inlineItemCount; i++)
                    {
                        if (itemList.First == null)
                            return;

                        //アイテム取得
                        RectTransform item = itemList.First.Value;
                        itemList.RemoveFirst();
                        itemList.AddLast(item);

                        //座標更新
                        LastItemIndex = FirstItemIndex + lineCount * inlineItemCount;
                        item.anchoredPosition = CalcItemPosition(LastItemIndex, lineCount * inlineItemCount);
                        _onUpdateItem(LastItemIndex, item.gameObject);
                        RestrictScroll(LastItemIndex, item.gameObject);

                        //現在アイテム番号更新
                        FirstItemIndex++;
                    }
                }

                //上から出現するアイテムを更新
                while (anchoredPosition - _diffPreFramePosition + firstItemMargin > 0)
                {
                    _diffPreFramePosition += lineSpace;

                    for (var i = 0; i < inlineItemCount; i++)
                    {
                        if (itemList.Last == null)
                            return;

                        //アイテム取得
                        RectTransform item = itemList.Last.Value;
                        itemList.RemoveLast();
                        itemList.AddFirst(item);

                        //現在アイテム番号更新
                        FirstItemIndex--;

                        //座標更新
                        item.anchoredPosition = CalcItemPosition(FirstItemIndex, 0);
                        _onUpdateItem(FirstItemIndex, item.gameObject);
                        RestrictScroll(FirstItemIndex, item.gameObject);
                    }
                }
                transform.hasChanged = false;
            }

            //動いている
            if ((_oldPosition - transform.position).sqrMagnitude >= STOP_SCROLL_TRIGGER_INSENSETIVITY)
            {
                IsStopped = false;
            }
            //（ほぼ）止まっている
            else
            {
                IsStopped = true;
            }

            _oldPosition = transform.position;

            //アニメーション
            if (_animatableDistance > 0f)
            {
                _animatableDistance *= 1f - Mathf.Clamp((Time.deltaTime / DEF_MAX_FRAME_RATE_TIME) * animationSpeed, animationSpeed, MAX_ANIMATION_RATIO);
                if (_animatableDistance < SCROLL_UNLOCK_POINT)
                {
                    if (_scrollRect != null &&
                        _scrollRect.enabled == false)
                    {
                        _scrollRect.enabled = true;
                    }
                }

                if (_animatableDistance < ANIMATION_SKIP_POINT)
                    _animatableDistance = 0f;

                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList.First.Value;
                    itemList.RemoveFirst();
                    itemList.AddLast(item);
                    var calculatedPosition = CalcItemPosition(FirstItemIndex + i, i);
                    item.anchoredPosition = new Vector2(calculatedPosition.x, item.anchoredPosition.y);
                }
            }
        }

        /// <summary>
        /// 現在表示エリアのコンテンツを再読込する
        /// </summary>
        public void UpdateCurrentDisplayItems()
        {
            var length = FirstItemIndex + (lineCount * inlineItemCount);
            for (var i = FirstItemIndex; i < length; i++)
            {
                if (itemList.First == null)
                    return;

                //アイテム取得
                RectTransform item = itemList.First.Value;
                itemList.RemoveFirst();
                itemList.AddLast(item);

                item.anchoredPosition = CalcItemPosition(i, lineCount * inlineItemCount);
                _onUpdateItem(i, item.gameObject);
                RestrictScroll(i, item.gameObject);
            }
        }

        /// <summary>
        /// スクロール表示位置を端へ更新
        /// </summary>
        /// <param name="isFirstContentsPosition">true : verticalは「上」horizontalは「左」, false : verticalは「下」horizontalは「右」</param>
        public void ChangeScrollPositionEdge(bool isFirstContentsPosition)
        {
            //スクロール表示位置を一番下へ更新
            var normalizedPosition = isFirstContentsPosition ? 1 : 0;

            if (direction == Direction.Vertical)
            {
                _scrollRect.verticalNormalizedPosition = normalizedPosition;
            }
            else
            {
                _scrollRect.horizontalNormalizedPosition = normalizedPosition;
            }

            //表示すべき箇所を更新
            UpdateCurrentDisplayItems();
        }

        /// <summary>
        /// Indexからアイテムの座標を取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 CalcItemPosition(int index, int animateIndex = -1)
        {
            float linePos = lineSpace * (index / inlineItemCount) + firstItemMargin;
            float inlineItemPos = ((float)(index % inlineItemCount) - ((float)(inlineItemCount - 1) / 2)) * inlineItemSpace;

            if (animateIndex > -1)
            {
                var startIndex = animationStartType == AnimationStartWith.FirstItem ? animateIndex : lineCount * inlineItemCount - animateIndex;
                var distance = (startIndex + 1) * _animatableDistance;

                if (animationType == AnimationType.CrossSide)
                {
                    inlineItemPos += distance;
                }
                else
                {
                    linePos += distance;
                }
            }
            return direction == Direction.Vertical ? new Vector2(inlineItemPos, -linePos) : new Vector2(linePos, inlineItemPos);
        }

        public float GetNormalizedPosition()
        {
            return direction == Direction.Vertical ? _scrollRect.verticalNormalizedPosition : _scrollRect.horizontalNormalizedPosition;
        }

        public IObservable<Vector2> OnChangedPositionObservable()
        {
            return _scrollRect.OnValueChangedAsObservable();
        }

        private void RestrictScroll(int index, GameObject item)
        {
            if (index < 0 || index >= _maxDataCount)
            {
                item.SetActive(false);
                return;
            }
            item.SetActive(true);
        }

        /// <summary>
        /// 指定したインデックスのアイテムを先頭に表示 isReverseがtrueの時は Vertical:下,Horizon:右 からのアイテムを後尾に表示
        /// </summary>
        public void FocusIndexItem(int listCount, int index, bool isVertical, bool isReverse = false)
        {
            if (isVertical)
            {
                var itemHeight = itemPrototype.rect.height;
                var onDisplayItemHeight = (_scrollRect.viewport.rect.height - firstItemMargin - lastItemMargin) / (itemHeight + linePadding);
                var posY = 1f;
                var scrollValue = listCount - onDisplayItemHeight;
                if (scrollValue > 0) posY = 1f - ((1f / (scrollValue)) * index);
                if (isReverse) posY = 1f - posY;
                _scrollRect.verticalNormalizedPosition = Mathf.Clamp(posY, 0f, 1f);
            }
            else
            {
                var itemWidth = itemPrototype.rect.width;
                var onDisplayItemWidth = (_scrollRect.viewport.rect.width - firstItemMargin - lastItemMargin) / (itemWidth + linePadding);
                var posX = 1f;
                var scrollValue = listCount - onDisplayItemWidth;
                if (scrollValue > 0) posX = ((1f / scrollValue) * index);
                if (isReverse) posX = 1f - posX;
                _scrollRect.horizontalNormalizedPosition = Mathf.Clamp(posX, 0f, 1f);
            }

            UpdateCurrentDisplayItems();
        }
    }
}
