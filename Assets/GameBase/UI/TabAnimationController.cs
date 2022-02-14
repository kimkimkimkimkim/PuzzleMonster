using DG.Tweening;
using PM.Enum.Item;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBase
{
    public class TabAnimationController : MonoBehaviour
    {
        [SerializeField] protected List<TextMeshProUGUI> _tabTextList;
        [SerializeField] protected List<ToggleWithValue> _tabList;
        [SerializeField] protected Transform _tabBar;
        [SerializeField] protected bool _isCustomColor;
        [SerializeField] protected Color _onColor;
        [SerializeField] protected Color _offColor;

        private const float TAB_MOVING_TIME = 0.3f;

        private string ON_COLOR_CODE = TextColorType.Focus.Color();
        private string OFF_COLOR_CODE = TextColorType.White.Color();

        private void Start()
        {
            SetTabChangeAnimation();
        }

        /// <summary>
        /// 文字色の制御と選択バーの制御
        /// </summary>
        private void SetTabChangeAnimation()
        {
            var onColor = Color.white;
            var offColor = Color.white;
            ColorUtility.TryParseHtmlString(ON_COLOR_CODE, out onColor);
            ColorUtility.TryParseHtmlString(OFF_COLOR_CODE, out offColor);

            if (_isCustomColor)
            {
                onColor = _onColor;
                offColor = _offColor;
            }

            _tabList.ForEach(tab =>
            {
                tab.OnValueChangedAsObservable()
                    .Where(isOn => isOn)
                    .Do(_ => _tabTextList.ForEach(text => text.color = offColor))
                    .Do(_ => _tabTextList[tab.value].color = onColor)
                    .SelectMany(_ => _tabBar.DOMoveX(tab.gameObject.transform.position.x, TAB_MOVING_TIME).SetEase(Ease.OutQuint).OnCompleteAsObservable())
                    .Subscribe();
            });
        }
    }
}