using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Cinemachine;

public enum HumanParts
{
    none,
    head,
    handL,
    handR,
    footL,
    footR,
    body,
    waist
}

public class Human : CatchableObj
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("アニメーション")] private Animator _animator = default;
    [SerializeField, Tooltip("アニメーション")] private Animator _ghostAnimator = null;
    [SerializeField, Tooltip("レンダラー")] private List<SkinnedMeshRenderer> _skinRenderers = default;
    [SerializeField, Tooltip("レンダラー")] private List<BreakableHumanoidParts> _breakableHumanoidParts = default;
    [SerializeField, Tooltip("死亡時マテリアル")] private Material _dieMaterial = default;
    // [SerializeField, Tooltip("リジッドボディ")] private Rigidbody _rigidBody = default;
    [SerializeField, Tooltip("コライダー")] private Collider _collider = default;
    [SerializeField, Tooltip("カメラ振動")] private CinemachineImpulseSource _cinemachineImpulseSource = default;
    [SerializeField, Tooltip("レイヤー")] private int _childLayer = 20;
    [SerializeField, Tooltip("Ragdoll根本の位置")] private Transform _basePos;
    [SerializeField, Tooltip("パーツ追従")] private HumanPartsFollow _humanPartsFollow = null;
    [SerializeField, Tooltip("振り向く頭")] private Transform _lookAtHead = null;
    [SerializeField, Tooltip("声")] private AudioSource _audioSouce = default;
    [SerializeField, Tooltip("声リスト")] private List<AudioClip> _listAudioClip = default;
    // private bool _isBroken = false;
    private UnityEvent _onCatchCallback = default;
    private UnityEvent _onReleaseCallback = default;
    private UnityEvent _onDesableAnimationCallback = default;
    private UnityEvent _onChangePartsMassCallback = default;
    private Dictionary<HumanParts, HumanChild> _humanPartsDictionary = null;
    private Vector3 _initHeadAngle = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    protected override void StartUnique()
    {
        _initHeadAngle = _lookAtHead.localEulerAngles;
    }
    protected override void UpdateUnique()
    {
        Vector3 currentAngle = _lookAtHead.localEulerAngles;
        Vector3 targetAngle = _initHeadAngle;

        if(GameDataManager.IsCatchSomething() && !_isBroken && !IsCatch() )
        {
            _lookAtHead.LookAt(GameDataManager.GetLookAtPos());
            targetAngle = _lookAtHead.localEulerAngles;

            if( ( 90 < targetAngle.x && targetAngle.x < 270 )||
                ( 90 < targetAngle.y && targetAngle.y < 270 ) ||
                ( 90 < targetAngle.z && targetAngle.z < 270 ))
                {
                    targetAngle = _initHeadAngle;
                }
        }
        else
        {
            targetAngle = _initHeadAngle;
        }
        // Vector3 subAngle = (targetAngle + currentAngle) / 2;
        // targetAngle = (targetAngle + currentAngle) / 2;
        // Debug.Log("subAngle1:" + subAngle + ", " + subAngle.magnitude);
        // if( 1f < subAngle.magnitude)
        //     subAngle = subAngle.normalized;
        // Debug.Log("subAngle2:" + subAngle + ", " + subAngle.magnitude);

        _lookAtHead.localEulerAngles = targetAngle;
    }
    protected override void OnCatchUnique()
    { 
        DesableAnimation();
        _onCatchCallback?.Invoke();

        // ぐてっとさせる
        SetIsPartsFollow(false);
        ChangePartsMass();
    }

    protected override void OnReleaseUnique()
    { 
        ChangePartsMass();
        _onReleaseCallback?.Invoke();
    }

    protected override void OnBreakUnique()
    {
        DesableAnimation();
        for(int i = 0; i < _skinRenderers.Count; i++)
        {
            if(_skinRenderers[i] != null)
                _skinRenderers[i].material = _dieMaterial;
        }
        for(int i = 0; i < _breakableHumanoidParts.Count; i++)
        {
            _breakableHumanoidParts[i].SetMaterial(_dieMaterial);
        }

        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        // _onDoReleaseCallback?.Invoke();
        ChangePartsMass();

        if(!_isBroken)
        {
            _cinemachineImpulseSource.GenerateImpulse(new Vector3(0.4f, 0.4f, 0));
            // 声を出す
            int index = UnityEngine.Random.Range(0, _listAudioClip.Count);
            _audioSouce.PlayOneShot(_listAudioClip[index]);
            // バイブレーションさせる
            VibrationManager.VibrateLong();
        }
            
        _isBroken = true;

        // ぐてっとさせる
        SetIsPartsFollow(false);
    }
    public bool IsDead(){ return _isBroken; }

    public void AddCallbackOnCatch( UnityAction callback )
    {
        if(_onCatchCallback == null)
            _onCatchCallback = new UnityEvent();
        _onCatchCallback.AddListener(callback);
    }
    public void AddCallbackOnRelease( UnityAction callback )
    {
        if(_onReleaseCallback == null)
            _onReleaseCallback = new UnityEvent();
        _onReleaseCallback.AddListener(callback);
    }
    public void AddCallbackOnDesableAnimation( UnityAction callback )
    {
        if(_onDesableAnimationCallback == null)
            _onDesableAnimationCallback = new UnityEvent();
        _onDesableAnimationCallback.AddListener(callback);
    }
    

    public void DesableAnimation() {
        _animator.enabled = false;
        _onDesableAnimationCallback?.Invoke();

        // if(_rigidBody != null)
        // {
        //     _rigidBody.useGravity = false;
        //     _rigidBody.isKinematic = true;
        // }

        Rigidbody rigidBody = GetRigidbody();
        if(rigidBody != null)
        {
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
        }
        
        if(_collider != null)
            _collider.enabled = false;
    }
    public bool IsEnableAnimation(){ return _animator.enabled; }
    public void SetChildLayer(int layerMask){ _childLayer = layerMask; } 
    public int GetChildLayer(){ return _childLayer; } 
    // public Rigidbody GetRigidbody(){ return _rigidBody; }
    public void SetAnimatorController(RuntimeAnimatorController _animeController){
        _animator.runtimeAnimatorController = _animeController;
        if(_ghostAnimator != null)
            _ghostAnimator.runtimeAnimatorController = _animeController;
    }
    public void SetPos(Vector3 pos){
        _basePos.position = pos;
    }
    // パーツ登録
    public void SetParts(HumanParts key, HumanChild parts)
    {
        if(_humanPartsDictionary == null)
            _humanPartsDictionary = new Dictionary<HumanParts, HumanChild>();

        _humanPartsDictionary.Add(key, parts);
    }
    // パーツ取得
    public HumanChild GetParts(HumanParts key)
    { 
        if(_humanPartsDictionary == null)
            return null;
        if(_humanPartsDictionary.TryGetValue(key, out HumanChild ret))
            return ret;
        else
            return null; 
    }

    public void SetIsPartsFollow(bool isFollow)
    { 
        // 死んでるのに起きあがろうとしたら無視する。
        if(_isBroken && isFollow)
            isFollow = false;
        // 本体がアニメーションしてるのにゴースト追従を始めようとしたら無視する。
        if(_animator.enabled)
            isFollow = false;
        if(_humanPartsFollow != null)
            _humanPartsFollow.SetIsFollow(isFollow);
    }
    public void SetFollowBasePos( Vector3 pos )
    {
        _humanPartsFollow.SetFollowBasePos(pos);
    }
    public bool IsFollow(){ return _humanPartsFollow.IsFollow(); }

    public Transform GetGhost()
    {
        if( _ghostAnimator == null )
            return null;
        return _ghostAnimator.transform;
    }

    
    public void AddCallbackChangePartsMass( UnityAction callback )
    {
        if(_onChangePartsMassCallback == null)
            _onChangePartsMassCallback = new UnityEvent();
        _onChangePartsMassCallback.AddListener(callback);
    }

    public void ChangePartsMass()
    {
        _onChangePartsMassCallback?.Invoke();
    }
    // ---------- Private関数 ----------
}
