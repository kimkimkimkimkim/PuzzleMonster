using GameBase;
using UniRx;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[ResourcePath("UI/Parts/Parts-GachaAnimation")]
public class GachaAnimation: MonoBehaviour
{
    [SerializeField] protected FadeImage _fadeImage;
    [SerializeField] protected Button _startButton;
    [SerializeField] protected GameObject _startButtonBase;
    [SerializeField] protected GameObject _cameraAnimationStartPositionObject;
    [SerializeField] protected GameObject _cameraAnimationEndPositionObject;
    [SerializeField] protected GameObject _cameraObject;
    [SerializeField] protected GameObject _monumentObject;
    [SerializeField] protected GameObject _portalObject;
    [SerializeField] protected ParticleSystem _shockPS;
    [SerializeField] protected ParticleSystem _verticalPS;
    [SerializeField] protected ParticleSystem _groundPS;
    [SerializeField] protected ParticleSystem _spherePS;

    private const float PORTAL_DISTANCE = 5.3f;
    private const float CAMERA_ANIMATION_END_POSITION_DISTANCE = 3.5f;
    private const float CAMERA_FIRST_BACK_DISTANCE = 1.0f;
    private const float CAMERA_SECOND_BACK_DISTANCE = 1.0f;
    private const float SHOCK_PS_FROM_SCALEZ = 2.0f;
    private const float SHOCK_PS_TO_SCALEZ = 4.6f;
    private Vector3 SHOCK_PS_FROM_SCALE = new Vector3(1.0f, 1.0f, SHOCK_PS_FROM_SCALEZ);
    private Vector3 SHOCK_PS_TO_SCALE = new Vector3(1.0f, 1.0f, SHOCK_PS_TO_SCALEZ);

    // 時間関係の定数値
    private const float FADE_ANIMATION_TIME = 0.3f;
    private const float CAMERA_FIRST_BACK_ANIMATION_TIME = 1.0f;
    private const float CAMERA_FRONT_ANIMATION_TIME = 2.5f;
    private const float CAMERA_STOP_TIME = 0.4f;
    private const float CAMERA_SECOND_BACK_ANIMATION_TIME = 3.5f;
    private const float SHOCK_PS_ANIMATION_DELAY = CAMERA_FIRST_BACK_ANIMATION_TIME + CAMERA_FRONT_ANIMATION_TIME + CAMERA_STOP_TIME + 0.1f;
    private const float SHOCK_PS_ANIMATION_TIME = 3.0f;
    private const float VERTICAL_PS_ANIMATION_DELAY = CAMERA_FIRST_BACK_ANIMATION_TIME + CAMERA_FRONT_ANIMATION_TIME + CAMERA_STOP_TIME - 0.2f;
    private const float SPHERE_PS_ANIMATION_DELAY = CAMERA_FIRST_BACK_ANIMATION_TIME + CAMERA_FRONT_ANIMATION_TIME;
    private const float SPHERE_PS_ANIMATION_TIME = 5.0f;
    private const float TOTAL_ANIMATION_TIME = CAMERA_FIRST_BACK_ANIMATION_TIME + CAMERA_FRONT_ANIMATION_TIME + CAMERA_STOP_TIME + CAMERA_SECOND_BACK_ANIMATION_TIME - 1.2f;

    private Sequence cameraAnimationSequence;
    private Sequence shockPSAnimationSequence;
    private Sequence verticalPSAnimationSequence;
    private Sequence spherePSAnimationSequence;

    private void Awake()
    {
        _fadeImage.Range = 1.0f; 
    }

    public IObservable<Unit> PlayGachaAnimationObservable()
    {
        Init();

        var direction = _monumentObject.transform.position - _cameraAnimationStartPositionObject.transform.position;

        return _fadeImage.PlayFadeAnimationObservable(false, FADE_ANIMATION_TIME)
            .SelectMany(_ => _startButton.OnClickAsObservable())
            .Do(_ => _startButtonBase.SetActive(false))
            .SelectMany(_ =>
            {
                // カメラアニメーション
                var cameraFirstBackPosition = _cameraAnimationStartPositionObject.transform.position - direction.normalized * CAMERA_FIRST_BACK_DISTANCE;
                var cameraSecondBackPosition = _cameraAnimationEndPositionObject.transform.position - direction.normalized * CAMERA_SECOND_BACK_DISTANCE;
                cameraAnimationSequence = DOTween.Sequence()
                    .Append(_cameraObject.transform.DOMove(cameraFirstBackPosition, CAMERA_FIRST_BACK_ANIMATION_TIME))
                    .Append(_cameraObject.transform.DOMove(_cameraAnimationEndPositionObject.transform.position, CAMERA_FRONT_ANIMATION_TIME))
                    .AppendInterval(CAMERA_STOP_TIME)
                    .Append(_cameraObject.transform.DOMove(cameraSecondBackPosition, CAMERA_SECOND_BACK_ANIMATION_TIME));

                // 衝撃波エフェクトアニメーション
                shockPSAnimationSequence = DOTween.Sequence()
                    .AppendInterval(SHOCK_PS_ANIMATION_DELAY)
                    .Append(_shockPS.transform.DOScale(SHOCK_PS_TO_SCALE, SHOCK_PS_ANIMATION_TIME));

                // 垂直エフェクトアニメーション
                verticalPSAnimationSequence = DOTween.Sequence()
                    .AppendInterval(VERTICAL_PS_ANIMATION_DELAY)
                    .AppendCallback(() => _verticalPS.Play());

                // 球エフェクトアニメーション
                spherePSAnimationSequence = DOTween.Sequence();
                    //.AppendInterval(SPHERE_PS_ANIMATION_DELAY)
                    //.AppendCallback(() => _spherePS.Play())
                    //.AppendInterval(SPHERE_PS_ANIMATION_TIME);

                return Observable.Timer(TimeSpan.FromSeconds(TOTAL_ANIMATION_TIME));
            })
            .SelectMany(_ => _fadeImage.PlayFadeAnimationObservable(true, FADE_ANIMATION_TIME))
            .Do(_ =>
            {
                if (cameraAnimationSequence.IsActive()) cameraAnimationSequence.Kill();
                if (shockPSAnimationSequence.IsActive()) shockPSAnimationSequence.Kill();
                if (verticalPSAnimationSequence.IsActive()) verticalPSAnimationSequence.Kill();
                if (spherePSAnimationSequence.IsActive()) spherePSAnimationSequence.Kill();

                Destroy(this.gameObject);
            });
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init()
    {
        // オブジェクトの向きを取得
        var direction = _monumentObject.transform.position - _cameraAnimationStartPositionObject.transform.position;
        var look = Quaternion.LookRotation(direction, Vector3.up);

        // カメラとポータルを取得した向きに
        _cameraObject.transform.rotation = look;
        _portalObject.transform.rotation = look;

        // カメラ、ポータル、カメラの最終地点を設定した距離に配置
        var portalPosition = _cameraAnimationStartPositionObject.transform.position + direction.normalized * PORTAL_DISTANCE;
        var endPosition = _monumentObject.transform.position - direction.normalized * CAMERA_ANIMATION_END_POSITION_DISTANCE;
        _portalObject.transform.position = portalPosition;
        _cameraAnimationEndPositionObject.transform.position = endPosition;
        _cameraObject.transform.position = _cameraAnimationStartPositionObject.transform.position;

        // 衝撃波エフェクトとグラウンドエフェクトを再生
        _shockPS.transform.localScale = SHOCK_PS_FROM_SCALE;
        _shockPS.Play();
        _groundPS.Play();
    } 
}
