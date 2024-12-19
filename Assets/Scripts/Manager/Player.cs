using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public enum PlayerState{
    stop,
    move,
    battle,
}

public class Player : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("hoge")] private CinemachineDollyCart _cinemachineDollyCart = default;
    [SerializeField, Tooltip("hoge")] private float _baseSpeed = 0f;
    [SerializeField, Tooltip("hoge")] private Transform _lookAtTransform = null;
    private Vector3 _initPos = default;
    private Vector3 _initLookPos = default;
    private PlayerState _state = PlayerState.stop;
    private PlayerState _beforeState = PlayerState.stop;
    public PlayerState State{ get{ return _state; } }
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake()
    {
        _initPos = this.transform.position;
        _cinemachineDollyCart.m_Speed = 0f;
        _initLookPos = _lookAtTransform.transform.localPosition;
    }
    // private void Start(){
        
    // }

    // private void Update(){

    // }
    // ---------- Public関数 ------------------------- 
    public CinemachineDollyCart GetMovePath(){ return _cinemachineDollyCart; }
    public void StopPathMove(){ _cinemachineDollyCart.m_Speed = 0f; }
    public void ContinuePathMove()
    { 
        _cinemachineDollyCart.m_Speed = _baseSpeed; 
    }
    public void InitPos(){ this.transform.position = _initPos; }
    public Vector3 GetInitPos(){ return _initPos; }
    public void SetState(PlayerState state)
    { 
        if(state == _state)
            return;
        _beforeState = _state;
        _state = state; 

        // Debug.Log("ステータス変更！:" + state);

        switch(state)
        {
            case PlayerState.stop:
                StopPathMove();
                break;
            case PlayerState.battle:
                _cinemachineDollyCart.m_Speed = _baseSpeed / 4f; 
                break;
            case PlayerState.move:
                ContinuePathMove();
                break;
        }
    }
    public void SetBeforeState(){
        SetState(_beforeState);
    }

    // プレイヤーの向き更新
    public void SetLookAtTarget( Transform lookAtTarget )
    {
        if(_lookAtTransform == null)
        {
            GameObject gameObject = new GameObject("CameraFollowPos");
            _lookAtTransform = gameObject.transform;
        }

        if( lookAtTarget != null )
        {
            _lookAtTransform.parent = lookAtTarget;
            _lookAtTransform.localPosition = Vector3.zero;
        }
        else
        {
            _lookAtTransform.parent = this.transform;
            _lookAtTransform.localPosition = _initLookPos;
        }
    }
    // ---------- Private関数 ------------------------
}
