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
    [SerializeField, Tooltip("振り向くパーツ")] private List<Transform> _lookers = null;
    [SerializeField, Tooltip("声")] private AudioSource _audioSouce = default;
    [SerializeField, Tooltip("声リスト")] private List<AudioClip> _listAudioClip = default;
    [SerializeField, Tooltip("カメラ外、カメラ内イベント")] private ChildTrigger _visibleEventTrigger;
    // private bool _isBroken = false;
    private UnityEvent _onCatchCallback = default;
    private UnityEvent _onReleaseCallback = default;
    private UnityEvent _onDesableAnimationCallback = default;
    private UnityEvent _onChangePartsMassCallback = default;
    private UnityEvent<UnityAction<HumanChild>> _onPartsActiion = default;
    private Dictionary<HumanParts, HumanChild> _humanPartsDictionary = null;
    private List<Vector3> _lookerInitAngle = default;
    private bool _isGround = true;
    private Dictionary<GameObject, float> _stayObjectDic = null;
    private float _toughness = 1f;  // 死にやすさ。デフォルトは１
    private float _fallTime = 0;
    private bool _isOtherCatch = false;
    private bool _isVisible = true; // カメラに写っているか
    public bool IsVisible{ get{ return _isVisible; } }
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    protected override void StartUnique()
    {
        _lookerInitAngle = new List<Vector3>();

        for(int i = 0; i < _lookers.Count; i++)
        {
            _lookerInitAngle.Add(_lookers[i].localEulerAngles);
        }
        _stayObjectDic = new Dictionary<GameObject, float>();

        int index = PlayerPrefs.GetInt("newStageColor");

        if(_visibleEventTrigger != null)
        {
            _visibleEventTrigger.AddOnBecameVisible(()=>
            {
                _isVisible = true;
            });
            _visibleEventTrigger.AddOnBecameInVisible(()=>
            {
                _isVisible = false;
            });
        }
    }
    protected override void UpdateUnique()
    {
        for(int i = 0; i < _lookers.Count; i++)
        {
            LookAtTarget(_lookers[i], _lookerInitAngle[i], i);
        }

        // 直前に触れていたオブジェクトカウンターを減らす
        List<GameObject>  keyList = new List<GameObject>(_stayObjectDic.Keys);
        for( int i = 0; i < keyList.Count; i++ )
        {
            GameObject key = keyList[i];
            _stayObjectDic[key] -= Time.deltaTime;
            if( _stayObjectDic[key] <= 0f )
                _stayObjectDic.Remove(key);
        }
        
        if(!IsCatch() && (IsFollowBaseLock() || IsEnableAnimation()))
        {
            _isGround = true;
        }
        else
        {
            _isGround = false;
        }
        // 落下時間。接地してない＆捕まってないと増えていく
        if(!IsGround() && !IsCatch())
        {
            _fallTime += Time.deltaTime;
            // if( 0f < _fallTime)
            //     Debug.Log("_fallTime:"+_fallTime);
        }
        else
        {
            _fallTime = 0f;
        }
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
        // イベント用：プレイヤー画面を押している間に56された
        if( Input.GetMouseButton(0) )
            GameDataManager.SetIsDefeat(true);

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
        ChangePartsMass();

        if(!_isBroken)
        {
            _cinemachineImpulseSource.GenerateImpulse(new Vector3(0.4f, 0.4f, 0));

            if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
            {
                // 声を出す
                int index = UnityEngine.Random.Range(0, _listAudioClip.Count);
                _audioSouce.PlayOneShot(_listAudioClip[index]);
                // バイブレーションさせる
                VibrationManager.VibrateLong();
            }
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
    public void SetFollowBasePos( Vector3 pos, bool isMove )
    {
        _humanPartsFollow.SetFollowBasePos(pos, isMove);
    }
    public bool IsFollow(){ return _humanPartsFollow.IsFollow(); }
    public bool IsFollowBaseLock(){ return _humanPartsFollow.IsFollowBaseLock(); }

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

    // 触れているオブジェクト登録
    public void SetStayObjectDic(GameObject gameObject)
    {
        float setTime = 0.15f;
        if(_stayObjectDic.ContainsKey(gameObject))
        {
            _stayObjectDic[gameObject] = setTime;
        }
        else
        {
            _stayObjectDic.Add(gameObject, setTime);
        }
    }
    public bool IsStayObject(GameObject gameObject)
    {
        return _stayObjectDic.ContainsKey(gameObject);
    }


    // ギミック側などから、Humanの全パーツに処理をしたい時のコールバック追加、実行
    // HumanChildから初期設定時に登録
    public void AddCallbackOnPartsActiion( UnityAction<UnityAction<HumanChild>> callback )
    {
        if(_onPartsActiion == null)
            _onPartsActiion = new UnityEvent<UnityAction<HumanChild>>();
        _onPartsActiion.AddListener(callback);
    }
    // ギミック側が使う
    public void PartsActiion( UnityAction<HumanChild> partsActiion )
    {
        _onPartsActiion?.Invoke(partsActiion);
    }

    // 接地判定。ギミックなどが参考にする
    public bool IsGround(){ return _isGround; }
    // 接地判定。設定。HumanChildなどから設定する
    public void SetIsGround( bool isGround ){ _isGround = isGround; }

    public void SetToughness( float toughness ){ _toughness = toughness; }
    public float GetToughness()
    { 
        float ret = _toughness;
        ret -= _fallTime * 0.3f;
        // if( 0f < _fallTime)
        //     Debug.Log("_fallTime:" + _fallTime);
        if(ret <= 0.001f)
            ret = 0.001f;
        return ret; 
    }

    // 他の何かに捕まっているか
    public void SetIsOtherCatch( bool isOtherCatch ){ _isOtherCatch = isOtherCatch; }
    public bool IsOtherCatch(){ return _isOtherCatch; }
    // カメラに写っているか
    public void SetIsVisible(bool isVisible){ _isVisible = isVisible; }
    // ---------- Private関数 ----------
    private void LookAtTarget(Transform looker, Vector3 initAngle, int index)
    {
        Vector3 currentAngle = looker.localEulerAngles;
        Vector3 targetAngle = initAngle;
        float _plusRotationY = initAngle.y;
        float _plusRotationZ = initAngle.z;
        float _maxAngle = 60;

        float angleLimitX = 60f;
        float angleLimitY = 45f;
        float angleLimitZ = 10f;
        if(index != 0)
        {
            angleLimitX = 15;
            angleLimitY = 45;
        }

        // 捕まってるやつを見るか否かのABフラグ
        if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
        {
            if(GameDataManager.IsCatchSomething() && !_isBroken && !IsCatch() )
            {
                Vector3 lookPos = GameDataManager.GetLookAtPos();
                // targetAngle = initAngle;
                // looker.localEulerAngles = initAngle;

                // Vector3 eulerAngles = looker.eulerAngles;
                // looker.eulerAngles = new Vector3(0, 180, 0);

                looker.LookAt(lookPos);

                // looker.localEulerAngles += eulerAngles;

                // 
                // pos.y = looker.position.y;
                // looker.LookAt(looker.position);
                // looker.rotation = Quaternion.identity;
                // targetAngle = GameDataManager.GetLookAtPos() - looker.position;
                // looker.rotation = Quaternion.FromToRotation(Vector3.forward, targetAngle);

                // targetAngle = targetAngle.normalized;

                targetAngle = looker.localEulerAngles;
                
                // Debug.Log("pos:" + GameDataManager.GetLookAtPos());

                // 首の角度がやばくなりそうなら、近似の正常な角度に戻す
                if( angleLimitX < targetAngle.x && targetAngle.x < 180 ) 
                    targetAngle.x = angleLimitX;
                if( 180 < targetAngle.x && targetAngle.x < (360 - angleLimitX) ) 
                    targetAngle.x = (360 - angleLimitX);
                if( angleLimitY < targetAngle.y && targetAngle.y < 180 ) 
                    targetAngle.y = angleLimitY;
                if( 180 < targetAngle.y && targetAngle.y < (360 - angleLimitY)) 
                    targetAngle.y = (360 - angleLimitY);
                if( angleLimitZ < targetAngle.z && targetAngle.z < 180 ) 
                    targetAngle.z = angleLimitZ;
                if( 180 < targetAngle.z && targetAngle.z < (360 - angleLimitZ) ) 
                    targetAngle.z = (360 - angleLimitZ);

                // if(transform.parent.name == "HumanHub (3)")
                //     Debug.Log("targetAngle:" + looker.localEulerAngles + ", " + looker.eulerAngles);


                // 制限なしの回転を求め...
                // var rotation = Quaternion.LookRotation(GameDataManager.GetLookAtPos() - looker.position);
                // その回転角を_maxAngleまでに制限した回転を作り、それをrotationにセットする
                // looker.rotation = Quaternion.RotateTowards(Quaternion.identity, rotation, _maxAngle);
                
                // looker.Rotate(0, _plusRotationY, _plusRotationZ);//回転値をプラスして補間
            }
            else
            {
                targetAngle = initAngle;
                looker.localEulerAngles = targetAngle;
            }
        }

        looker.localEulerAngles = targetAngle;
    }
}
