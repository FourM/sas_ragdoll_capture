using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 虎ステージ
/// </summary>
public class Stage10SubManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージ")] private GameStage _stage;
    [SerializeField, Tooltip("檻")] private Transform _fence = default;
    [SerializeField, Tooltip("猛獣")] private Transform _beast = default;
    [SerializeField, Tooltip("目的地")] private Transform _targetPos = default;
    [SerializeField, Tooltip("アニメーションカーブ")] private AnimationCurve _animCurve = default;
    [SerializeField, Tooltip("アニメーション時間")] private float _animTime = 2f;
    [SerializeField, Tooltip("檻が開いた判定")] private float _openPosY = 3.5f;
    [SerializeField, Tooltip("檻が開いた判定")] private float _openPosX = 2.5f;
    [SerializeField, Tooltip("虎アニメーター")] private Animator _animator = null;
    [SerializeField, Tooltip("虎アニメーター")] private ChildTrigger _tigerChlidTrigger = null;
    [SerializeField, Tooltip("虎の攻撃音")] private AudioSource _soundTigerAttack = null;
    [SerializeField, Tooltip("虎の攻撃音")] private float _soundPlayDelay = 0.3f;
    private bool _isInitialize = false;
    private bool _isBeastDash = false;
    private bool _isBeastAttack = false;
    private Vector3 _fenceInitPos = default;
    private Transform _target = null;
    private Tween _beastMoveTween = null;
    private Human _human = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);
    }
    private void Initialize()
    {
        _fenceInitPos = _fence.position;
        _human = _stage.GetHuman(0);
        _target = _human.GetParts(HumanParts.head).transform;

        _tigerChlidTrigger.AddCallbackOnTriggerEnter((Collider other)=>
        {
            // Debug.Log(":" + other.gameObject.layer + ", ");
            if(other.gameObject.layer == LayerMask.NameToLayer("Human1"))
            {
                // Debug.Log("Human1だ！" + other.gameObject.layer + ", ");
                _isBeastAttack = true;
                _animator.SetBool("isRunning", false);
                _animator.SetBool("isAttacking", true);
                if(_beastMoveTween != null)
                {
                    _beastMoveTween.Kill();
                    _beastMoveTween = null;
                }
                // 虎の鳴き声を出す
                if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
                {
                    DOVirtual.DelayedCall(_soundPlayDelay, ()=>
                    {
                        if(_soundTigerAttack != null)
                            _soundTigerAttack.PlayOneShot(_soundTigerAttack.clip);
                    }); 
                }
            }
        });
    }
    private void FixedUpdate(){
        float fenceSubPosX = Mathf.Abs(_fence.position.x - _fenceInitPos.x);

        Vector3 pos = _targetPos.position;
        pos.x = _target.position.x;
        pos.z = _target.position.z;
        _targetPos.position = pos;

        if(!_isBeastDash && (_openPosY <= _fence.position.y || _openPosX <= fenceSubPosX))
        {
            _beastMoveTween = _beast.DOMove(_targetPos.position, _animTime).SetEase(_animCurve).OnComplete(()=>
            {
                _animator.SetBool("isRunning", false);
            });
            _isBeastDash = true;
            _animator.SetBool("isRunning", true);
        }

        if(_human.IsBroken() && _isBeastDash )
        {
            _animator.SetBool("isAttacking", false);
        }

        Vector3 direction = _target.position - _beast.position;
        direction.y = 0;

        var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        _beast.rotation = Quaternion.Lerp(_beast.rotation, lookRotation, 0.1f);

        // if(!_human.IsBroken() && _isBeastDash && !_isBeastAttack)
        // {
        //     _beast.position += _beast.forward * 0.3f;
        // }   
    }

    private void OnDisable() {
        if(_beastMoveTween != null)
        {
            _beastMoveTween.Kill();
        }
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
}
