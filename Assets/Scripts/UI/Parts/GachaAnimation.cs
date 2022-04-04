using GameBase;
using UniRx;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaAnimation")]
public class GachaAnimation: MonoBehaviour
{
    [SerializeField] protected Button _startButton;
    [SerializeField] protected GameObject _canvasBase;
    [SerializeField] protected GameObject _cameraAnimationStartPositionObject;
    [SerializeField] protected GameObject _cameraAnimationEndPositionObject;
    [SerializeField] protected GameObject _cameraObject;
    [SerializeField] protected GameObject _monumentObject;
    [SerializeField] protected GameObject _portalObject;

    private const float PORTAL_DISTANCE = 3.3f;
    private const float CAMERA_ANIMATION_END_POSITION_DISTANCE = 3.5f;
    private const float CAMERA_BACK_DISTANCE = 1.0f;
    private const float CAMERA_BACK_ANIMATION_TIME = 1.0f;
    private const float CAMERA_FRONT_ANIMATION_TIME = 1.5f;

    public IObservable<Unit> PlayGachaAnimationObservable()
    {
        SetPosition();

        var direction = _monumentObject.transform.position - _cameraAnimationStartPositionObject.transform.position;
        return Observable.ReturnUnit()
            .SelectMany(_ => _startButton.OnClickAsObservable())
            .Do(_ => _canvasBase.SetActive(false))
            .SelectMany(_ =>
            {
                var cameraBackPosition = _cameraAnimationStartPositionObject.transform.position - direction.normalized * CAMERA_BACK_DISTANCE;
                var cameraAnimationSequence = DOTween.Sequence()
                    .Append(_cameraObject.transform.DOMove(cameraBackPosition, CAMERA_BACK_ANIMATION_TIME))
                    .Append(_cameraObject.transform.DOMove(_cameraAnimationEndPositionObject.transform.position, CAMERA_FRONT_ANIMATION_TIME));
                return cameraAnimationSequence.OnCompleteAsObservable().AsUnitObservable();
            })
            .Delay(TimeSpan.FromSeconds(3))
            .Do(_ =>
            {
                Destroy(this.gameObject);
            });
    }

    /// <summary>
    /// それぞれのオブジェクトを正しい位置に配置
    /// </summary>
    private void SetPosition()
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
    } 
}
