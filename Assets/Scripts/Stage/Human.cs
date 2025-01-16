using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Cinemachine;
using DG.Tweening;

public enum HumanParts
{
    none,
    head,
    handL,
    handR,
    footL,
    footR,
    body,
    waist,
    forearmL,
    forearmR
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
    [SerializeField, Tooltip("Ragdoll根本の位置の補助オブジェクト")] private Transform _basePosChild;
    // private bool _isBroken = false;
    private UnityEvent _onCatchCallback = default;
    private UnityEvent _onReleaseCallback = default;
    private UnityEvent _onDesableAnimationCallback = default;
    private UnityEvent _onChangePartsMassCallback = default;
    private UnityEvent _onFlinchCallback = default; // 怯んだ時のコールバック
    private UnityEvent _onFlinchEndCallback = default; // 怯み終了時のコールバック
    private UnityEvent<UnityAction<HumanChild>> _onPartsActiion = default;
    private Dictionary<HumanParts, HumanChild> _humanPartsDictionary = null;
    private UnityEvent<float> _onDamage = default;
    private List<Vector3> _lookerInitAngle = default;
    private bool _isGround = true;
    private Dictionary<GameObject, float> _stayObjectDic = null;
    private float _toughness = 1f;  // 死にやすさ。デフォルトは１
    private float _fallTime = 0;
    private bool _isOtherCatch = false;
    private bool _isVisible = true; // カメラに写っているか
    private Vector3 _baseInitPos = default;
    public bool IsVisible{ get{ return _isVisible; } }
    private int _maxHp = 10;
    public int MaxHP{ get{ return _maxHp; } }
    private int _currentHp = 10;
    public int HP{ get{ return _currentHp; } }
    private Vector3 _impactPos;
    private Transform _LookPlayer = null;
    private int _mutekiTime = 0;
    private Shield _haveShield = null;
    private bool _isCanGuard = true;
    private RuntimeAnimatorController _animFlinch = null;   // 怯みアニメーション
    private UnityEvent _actionChangeWaitCallBack = null;    // 状態変更待機
    private bool _isCanChaneAction = true;                  // 状態移行できるか
    private int _actionChangePrim = 0;                  // 状態移行の優先度
    private bool _isFallable = true;                    //　床がないと感じたら落ちるか
    private bool _isFloorDead = true;                   // 床ダメで死ぬか(事故死で勝手に死ぬロック)
    private bool _initIsFloorDead = true;               // _isFloorDeadの初期値。trueなら平時でも床で事故死する恐れがある。

    public bool IsFallable{ get{ return _isFallable; } set{ _isFallable = value; } }
    public bool IsFloorDead{ get{ return _isFloorDead; } set{ _isFloorDead = value; } }
    public bool InitIsFloorDead{ get{ return _initIsFloorDead; } set{ _isFloorDead = value; _isFloorDead = value; } }
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
            // 見えた
            _visibleEventTrigger.AddOnBecameVisible(()=>
            {
                _isVisible = true;
            });
            // 見えなくなった
            _visibleEventTrigger.AddOnBecameInVisible(()=>
            {
                _isVisible = false;
            });
        }
        _baseInitPos = _basePos.localPosition;

        // ある程度起き上がった時のコールバック設定
        _humanPartsFollow.AddOnStand(()=>
        {
            IsFloorDead = InitIsFloorDead;
        });
    }
    protected override void UpdateUnique()
    {
        for(int i = 0; i < _lookers.Count; i++)
        {
            if(!_isBroken)
            {
                // 生きてたらそちらを向く
                LookAtTarget(_lookers[i], _lookerInitAngle[i], i);
            }
            else
            {
                // 死んでたら首だけそのまま、目はデフォルトに戻す
                if( 0 < i )
                {
                    _lookers[i].localEulerAngles = _lookerInitAngle[i];
                }
            }

            _basePosChild.position = _basePos.position;
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

        if(_LookPlayer != null)
        {
            Vector3 LookPos = _LookPlayer.position;
            LookPos.y = _basePos.position.y;
            _basePos.LookAt(LookPos);
        }

        // 待機アクションを実行するか　怯んでいる時やガードしている時などは次のアクションが待機状態になる
        if(_isCanChaneAction)
        {
            _actionChangeWaitCallBack?.Invoke();
            _actionChangeWaitCallBack?.RemoveAllListeners();
        }
    }

    protected override void FixedUpdateUnique() {
        _mutekiTime--;
        if( _mutekiTime < 0 )
            _mutekiTime = 0;
    }

    protected override void OnCatchUnique()
    { 
        DesableAnimation();
        _onCatchCallback?.Invoke();
        _LookPlayer = null;

        // ぐてっとさせる
        SetIsPartsFollow(false);
        ChangePartsMass();

        // 落下を有効化
        IsFallable = true;
        // 床で死ぬのを有効化
        IsFloorDead = true;
    }

    protected override void OnReleaseUnique()
    { 
        ChangePartsMass();
        _onReleaseCallback?.Invoke();
    }

    protected override void OnDamageUnique(int damage)
    {
        if(_isBroken)
            return;

        if(0 < _mutekiTime )
            return;
        _mutekiTime = 30;

        _currentHp -= damage;
        if( !_isBroken )
        {
            EffectManager.instance.PlayEffect(_impactPos, effectType.impact);
        }

        _onDamage?.Invoke(damage);
        if(_currentHp <= 0 || GameDataManager.GameMode == GameMode.main)
            OnBreak();

        DesableAnimation();
        // ぐてっとさせる
        SetIsPartsFollow(false);
        _LookPlayer = null;
    }

    protected override void OnBreakUnique()
    {
        // Debug.Log("OnBreakだお");
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
            GameDataManager.OnHumanDie(this);
        }
            
        _isBroken = true;
        _LookPlayer = null;

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
    
    // アニメーション無効化
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

    // 怯む
    public void Flinch()
    {
        // 今のアニメーションを記憶
        RuntimeAnimatorController beforeAnimator = GetCurrentAnimatorController();
        SetAnimatorController(_animFlinch);
        _isCanChaneAction = false; 
        // ガードアニメーション終了後の処理
        DOVirtual.DelayedCall(1.5f, ()=>
        {
            // 直前のアニメーションに戻す
            SetAnimatorController(beforeAnimator);
            _isCanChaneAction = true;
        }).SetLink(this.gameObject).OnComplete(()=>{_onFlinchEndCallback?.Invoke();});
        _onFlinchCallback?.Invoke();
    }

    // 次のアクション待機
    public void AddActionChangeWaitCallBack( UnityAction callback )
    {
        if(_actionChangeWaitCallBack == null)
            _actionChangeWaitCallBack = new UnityEvent();
        _actionChangeWaitCallBack.AddListener(callback);
    }
    // 次のアクション待機を削除
    public void RemoveActionChangeWaitCallBack( UnityAction callback )
    {
        _actionChangeWaitCallBack?.RemoveListener(callback);
    }
    

    public void SetIsCanChaneAction(bool isCanChaneAction){ _isCanChaneAction = isCanChaneAction; }

    // アニメーションの再有効化
    public void EnableAnimation() {

        // 直前までアニメーションが無効化されてたら位置補正
        if(!_animator.enabled)
        {
            // _basePosChild.parent = this.transform.parent.parent;

            Vector3 setpos = _basePosChild.position;

            setpos.y -= _basePos.localPosition.z * _basePos.parent.localScale.z;
            this.transform.position = setpos;

            _animator.runtimeAnimatorController = _ghostAnimator.runtimeAnimatorController;
        }

        _animator.enabled = true;

        Rigidbody rigidBody = GetRigidbody();
        if(rigidBody != null)
        {
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
        }
        
        if(_collider != null)
            _collider.enabled = true;
    }
    public bool IsEnableAnimation(){ return _animator.enabled; }
    public void SetChildLayer(int layerMask){ _childLayer = layerMask; } 
    public int GetChildLayer(){ return _childLayer; } 
    // public Rigidbody GetRigidbody(){ return _rigidBody; }
    public void SetAnimatorController(RuntimeAnimatorController _animeController, bool isNeko = true){
        if(_animator.enabled)
            _animator.runtimeAnimatorController = _animeController;
        if(_ghostAnimator != null)
            _ghostAnimator.runtimeAnimatorController = _animeController;
    }
    public RuntimeAnimatorController GetCurrentAnimatorController(){ return _ghostAnimator.runtimeAnimatorController; }
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

    // 怯みアニメーション設定
    public void SetFlinchAnim( RuntimeAnimatorController animFlinch )
    {
        _animFlinch = animFlinch;
    }
    // 怯んだ時のコールバック設定
    public void AddCallbackOnFlinch( UnityAction callback )
    {
        if(_onFlinchCallback == null)
            _onFlinchCallback = new UnityEvent();
        _onFlinchCallback.AddListener(callback);
    }
    public void AddCallbackOnFlinchEnd( UnityAction callback )
    {
        if(_onFlinchEndCallback == null)
            _onFlinchEndCallback = new UnityEvent();
        _onFlinchEndCallback.AddListener(callback);
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

    public void AddOnDamage( UnityAction<float> onDamage )
    {
        if(_onDamage == null)
            _onDamage = new UnityEvent<float>();
        _onDamage.AddListener(onDamage);
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
    public void SetImpactPos(Vector3 pos ){ _impactPos = pos; }

    public void ActiveLookPlayer( Transform player ){ _LookPlayer = player; }
    public void InitMaxHp(int maxHP)
    { 
        _maxHp = maxHP; 
        _currentHp = _maxHp;
    }

    // シールド設定
    public void SetHaveShield(Shield haveShield){ _haveShield = haveShield;} 
    // ガードの可否（シールドを持ってたら）
    public void SetIsCanGuard(bool isCanGuard){ _isCanGuard = isCanGuard; }
    // 代わりに捕まえさせる物を返す。nullならデフォルト値を返すけど、それもnull
    protected override CatchableObj GetAlternateUnique()
    {
        CatchableObj ret = null; 
        // シールドを構えられるならシールドを返す
        if(_haveShield != null && _isCanGuard)
            ret = _haveShield;
        return ret;
    }
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
