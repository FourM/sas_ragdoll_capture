using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Events;

public enum PlayerState{
    stop,
    move,
    battle,
    down,
    dash,
    battleStop,
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
    [SerializeField, Tooltip("プレイヤーの仮想の体")] private Transform _playerBody = null;
    [SerializeField, Tooltip("プレイヤー倒れる角度")] private float _downAngle = -11f;
    [SerializeField, Tooltip("プレイヤー倒れる時間")] private float _downDulation = 1.0f;
    [SerializeField, Tooltip("プレイヤー倒れるディレイ")] private float _downDelay = 0.3f;
    [SerializeField, Tooltip("おててのアニメーション")] private Animator _handAnimator = default;
    [SerializeField, Tooltip("おててのアニメーション")] private Animation _handAnimation = default;
    [SerializeField, Tooltip("敵の弾の目標位置")] private Transform _bulletTargetPos = default;
    [SerializeField, Tooltip("カメラ振動")] private CinemachineImpulseSource _cinemachineImpulseSource = default;
    private string[] character_anim_parameter = {"Idle", "Down"};
    private Vector3 _initPos = default;
    private Vector3 _initLookPos = default;
    private PlayerState _state = PlayerState.stop;
    private PlayerState _beforeState = PlayerState.stop;
    public PlayerState State{ get{ return _state; } }
    private int _webNum = 4;
    public int WebNum{ get{ return _webNum; } }
    private UnityEvent _onWebNumEmplty = null;
    private bool _isEnemyLook = false;
    public bool IsEnemyLook{ get{ return _isEnemyLook; } }
    private bool _isDash = false;
    private float _dashSpd = 1f;
    private bool _isEnemyAttackWait = false;
    public bool IsEnemyAttackWait
    {   
        get{ return _isEnemyAttackWait; } 
        set{ 
            _isEnemyAttackWait = value;
            if( !_down && !value)
                _cinemachineDollyCart.enabled = true;
            else
                _cinemachineDollyCart.enabled = false;
        } 
    }
    private bool _down = false;
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
    private void FixedUpdate(){
        if(_state == PlayerState.dash)
        {
            _dashSpd += 0.05f;
            if(2.3f < _dashSpd)
                _dashSpd = 2.5f;
            _cinemachineDollyCart.m_Speed = _baseSpeed * _dashSpd; 
        }
    }
    // ---------- Public関数 ------------------------- 
    public void Reset()
    {
        _playerBody.localPosition = new Vector3(0, -1.8f, 0);
        _playerBody.localEulerAngles = Vector3.zero;
        _cinemachineDollyCart.enabled = true;
        // アニメーションに関係する手をゲーム起動時に再生成していて、アニメーションから手の参照が切れているため、再スキャンする
        _handAnimator.Rebind();
        _handAnimator.Update(0);  // これも重要
        _onWebNumEmplty = new UnityEvent();
        _webNum = SaveDataManager.GetLevelWebNum() + 3;
        SetState(PlayerState.stop);
    }
    public CinemachineDollyCart GetMovePath(){ return _cinemachineDollyCart; }
    public void StopPathMove(){ _cinemachineDollyCart.m_Speed = 0f; }
    public void ContinuePathMove(bool isDash = false)
    { 
        _isDash = isDash;
        if(!isDash)
        {
            _cinemachineDollyCart.m_Speed = _baseSpeed; 
        }
        else
        {
            _cinemachineDollyCart.m_Speed = _baseSpeed * _dashSpd; 
        }   
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
                _cinemachineDollyCart.m_Speed = _baseSpeed / 4.5f; 
                break;
            case PlayerState.battleStop:
                _cinemachineDollyCart.m_Speed = 0f; 
                break;
            case PlayerState.move:
                ContinuePathMove();
                break;
            case PlayerState.dash:
                ContinuePathMove(true);
                break;
            case PlayerState.down:
                break;
        }
        _dashSpd = 1.05f;
        ChangeAction();
    }
    public void SetBeforeState(){
        SetState(_beforeState);
    }

    // プレイヤーの向き更新
    public void SetLookAtTarget( Transform lookAtTarget, bool isEnemyLook = false)
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
            _isEnemyLook = isEnemyLook;
        }
        else
        {
            _lookAtTransform.parent = this.transform;
            _lookAtTransform.localPosition = _initLookPos;
            _isEnemyLook = false;
        }
    }

    public void Down(TweenCallback onComplete)
    {
        _down = true;
        Vector3 effectPos = this.transform.position;
        effectPos += this.transform.forward * 0.508f;
        effectPos += this.transform.up * -0.46f;
        effectPos += this.transform.right * 0.11f;

        _cinemachineDollyCart.enabled = false;
        EffectManager.instance.PlayEffect(effectPos, effectType.impact);
        float cameraHeight = 1.8f;
        Vector3 pos = this.transform.position;
        pos -= Camera.main.transform.forward * cameraHeight;
        float posY = pos.y - cameraHeight + 0.1f;

        // カメラ振動
        _cinemachineImpulseSource.GenerateImpulse(new Vector3(0.6f, 0.6f, 0));

        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(_downDelay * 0.1f);
        sequence.Append(_playerBody.DOShakePosition(_downDelay * 0.8f, 0.06f, 30, 1, false, true));
        sequence.AppendInterval(_downDelay * 0.9f);
        sequence.AppendCallback(()=>{
            SetState(PlayerState.down);
            _lookAtTransform.parent = this.transform;
            _lookAtTransform.DOLocalMove(new Vector3(0, 10f, 1f), _downDulation + 0.1f).SetEase(Ease.InBack);
            this.transform.DOMoveX( pos.x, _downDulation).SetEase(Ease.InBack);
            this.transform.DOMoveZ( pos.z, _downDulation).SetEase(Ease.InBack);
        });
        sequence.AppendInterval(0.2f);
        sequence.Append(this.transform.DOMoveY( posY, _downDulation).SetEase(Ease.InQuad));
        sequence.AppendInterval(0.1f);
        sequence.AppendCallback(()=>{ onComplete(); });
        // sequence.Append(_playerBody.DOLocalRotate(new Vector3(_downAngle, 0, 0), _downDulation).SetEase(Ease.InBack).OnComplete(onComplete));
    }

    // アニメーション変更
    public void ChangeAction()
    {
        foreach(string animParamin in character_anim_parameter)
        {
            _handAnimator.SetBool(animParamin, false);
        }
        switch(_state)
        {
            case PlayerState.down:
                _handAnimator.SetBool("Down", true);
                _handAnimator.Play("Base Layer.Down");
                break;
            default:
                _handAnimator.SetBool("Idle", true);
                _handAnimator.Play("Base Layer.Idle");
                break;
        }
    }

    public void AddWebNum(int addNum){
        // _webNum += addNum;

        // Debug.Log("いとお:" + _webNum);
    }

    public Transform GetBulletTargetTransform(){ return _bulletTargetPos; }
    // ---------- Private関数 ------------------------
}
