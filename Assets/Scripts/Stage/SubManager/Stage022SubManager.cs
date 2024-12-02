using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// スイッチ押して大砲打つステージ
/// </summary>
public class Stage022SubManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージ")] private GameStage _stage;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _switchTrigger = null;
    [SerializeField, Tooltip("ボタンがONになった時のレンダラー")] private Renderer _switchRenderer = null;
    [SerializeField, Tooltip("ボタンがONになった時のマテリアル")] private Material _onSwitchMaterial = null;
    [SerializeField, Tooltip("発射エフェクト")] private ParticleSystem _showEffect = null;
    [SerializeField, Tooltip("球")] private Rigidbody _shotBall = null;
    [SerializeField, Tooltip("打ち出す力：前")] private float _frontVelocity;
    [SerializeField, Tooltip("打ち出す力：上")] private float _upVelocity;
    [SerializeField, Tooltip("球発射位置")] private Transform _shotPos = null;
    [SerializeField, Tooltip("ボタン押して下がる距離")] private float _moveY = 0.5f;
    [SerializeField, Tooltip("大砲")] private Transform _cannnonBody;
    [SerializeField, Tooltip("大砲")] private AnimationCurve _cannnonMoveEase;
    [SerializeField, Tooltip("大砲")] private float _cannonMoveduration;
    [SerializeField, Tooltip("大砲")] private float _cannonShotStandbyDuration;
    [SerializeField, Tooltip("大砲")] private float _angle;
    [SerializeField, Tooltip("大砲の発射音")] private AudioSource _soundCannonShot = null;
    [SerializeField, Tooltip("大砲の駆動音")] private AudioSource _soundCannonMove = null;
    [SerializeField, Tooltip("大砲の駆動音")] private AudioSource _soundCannonMove2 = null;
    [SerializeField, Tooltip("スイッチオン")] private AudioSource _soundSwitchOn = null;
    private bool _isInitialize = false;
    private bool _isShot = false;
    private Sequence _sequence = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);
    }
    private void OnDisable() {
        if(_sequence != null)
            _sequence.Kill();
    }
    private void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;

        _shotBall.isKinematic = true;
        _shotBall.useGravity = false;
        _shotBall.velocity = Vector3.zero;
        _shotBall.gameObject.SetActive(false);


        _switchTrigger.AddCallbackOnTriggerEnter((Collider other)=>
        {
            if(_isShot)
                return;
            
            CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(other.gameObject);   
            if(collitionChatchableObj == null && other.gameObject.layer != LayerMask.NameToLayer("MissWeb"))
                return;

            // スイッチの見た目を変える
            _switchRenderer.material = _onSwitchMaterial;
            Vector3 pos = _switchTrigger.transform.position;
            pos.y -= _moveY;
            _switchTrigger.transform.position = pos;

            Sequence sequence = DOTween.Sequence();

            // 音を出す
            if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
                _soundSwitchOn.PlayOneShot(_soundSwitchOn.clip);
            sequence.AppendInterval(0.2f);
            // 音を出す
            if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
            {
                sequence.AppendCallback(()=>{
                    _soundCannonMove.PlayOneShot(_soundCannonMove.clip);
                    DOVirtual.DelayedCall(_cannonMoveduration - 0.1f, ()=>
                    {
                        _soundCannonMove2.PlayOneShot(_soundCannonMove2.clip);
                    });
                });
            }
            sequence.Append(_cannnonBody.DOLocalRotate(new Vector3(_angle, 0, 0), _cannonMoveduration).SetEase(_cannnonMoveEase).OnComplete(()=>
            {
                if(_soundCannonMove != null)
                    _soundCannonMove.Stop();
            }));
            sequence.AppendInterval(_cannonShotStandbyDuration);

            // 発射あ！！
            sequence.AppendCallback(()=>
            {
                _showEffect.Play();

                _shotBall.gameObject.SetActive(true);
                _shotBall.isKinematic = false;
                _shotBall.useGravity = true;

                _shotBall.transform.position = _shotPos.position;
                _shotBall.transform.eulerAngles = _shotPos.eulerAngles;
                Vector3 velocity = _shotBall.transform.forward * _frontVelocity;
                velocity += _shotBall.transform.up * _upVelocity;
                _shotBall.velocity = velocity;

                // 音を出す
                if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
                    _soundCannonShot.PlayOneShot(_soundCannonShot.clip);
            });

            // Debug.Log("_shotBall.transform.eulerAngles:" + _shotBall.transform.eulerAngles + ", " + _shotBall.transform.forward + ", " + _shotBall.transform.up);
            // Debug.Log(":" + _shotBall.transform.forward * _frontVelocity + ", " + _shotBall.transform.up * _upVelocity + ", " + velocity);
            _isShot = true;
        });
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
}
