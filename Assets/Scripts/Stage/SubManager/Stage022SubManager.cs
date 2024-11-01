using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private bool _isInitialize = false;
    private bool _isShot = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);
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
            if(collitionChatchableObj == null)
                return;

            _switchRenderer.material = _onSwitchMaterial;
            _showEffect.Play();

            _shotBall.gameObject.SetActive(true);
            _shotBall.isKinematic = false;
            _shotBall.useGravity = true;

            _shotBall.transform.position = _shotPos.position;
            _shotBall.transform.eulerAngles = _shotPos.eulerAngles;
            Vector3 velocity = _shotBall.transform.forward * _frontVelocity;
            velocity += _shotBall.transform.up * _upVelocity;
            _shotBall.velocity = velocity;

            // Debug.Log("_shotBall.transform.eulerAngles:" + _shotBall.transform.eulerAngles + ", " + _shotBall.transform.forward + ", " + _shotBall.transform.up);
            Debug.Log(":" + _shotBall.transform.forward * _frontVelocity + ", " + _shotBall.transform.up * _upVelocity + ", " + velocity);

            Vector3 pos = _switchTrigger.transform.position;
            pos.y -= _moveY;
            _switchTrigger.transform.position = pos;

            _isShot = true;
        });
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
}
