using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CatchableObj : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    private const float FAST_SWIPED_TIME = 0.2f;
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("これが捕まったら代わりに捕まえさせる部位。nullならこれ自身が捕まる")] protected CatchableObj _alternate = null;
    [SerializeField, Tooltip("これが捕まったら代わりにぐるぐる巻きする部位。nullならこれ自身が捕まる")] protected Transform _catchWebParent = null;
    [SerializeField, Tooltip("リジッドボディ")] protected Rigidbody _rigidbody = null;
    [SerializeField, Tooltip("ぐるぐる巻き糸のスケール。nullならデフォルトサイズ")] protected Vector3 _webScale = default;
    [SerializeField, Tooltip("ぐるぐる巻き糸のローカル位置。nullなら0")] protected Vector3 _webPosition = default;
    [SerializeField, Tooltip("ぐるぐる巻き糸のローカル回転。nullなら0")] protected Vector3 _webRotate = default;
    private bool _isCatch;
    protected bool _isBroken = false;
    private GameObject _parent;
    private UnityEvent _onBreakCallback = default;
    // 条件を満たしたらこれを手放すコールバック
    protected UnityAction _onDoReleaseCallback = null;
    private bool _isInitialize = false;
    private UnityEvent _onInitialize = null;
    private Vector3 _beforevelocity = default;
    private Vector3 _beforevelocity2 = default;
    private float _fastSwipedTime = 0f;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start(){
        Initialize();
    }
    private void Update(){
        _beforevelocity = _rigidbody.velocity;
        _beforevelocity2 = _beforevelocity;

        _fastSwipedTime -= Time.deltaTime;
        if(_fastSwipedTime <= 0)
            _fastSwipedTime = 0;
        UpdateUnique();
    }
    
    public void Initialize(){
        if(_isInitialize)
            return;
        _isInitialize = true;

        GameDataManager.AddCatchableObjDic(this.gameObject, this);

        if(_rigidbody == null)
        {
            Debug.Log("_rigidbody is null:" + this.gameObject.name);
            _rigidbody = this.gameObject.GetComponent<Rigidbody>();
        }

        if(_catchWebParent == null)
            _catchWebParent = this.transform;
        if(_webScale == Vector3.zero)
            _webScale = Vector3.one;

        StartUnique();

        if(_onInitialize != null)
            _onInitialize?.Invoke();
    }
    // ---------- Public関数 ----------
    // 捕まった時の共通処理
    public void OnCatch()
    { 
        // if(_isCatch)
        //     return;
        _isCatch = true; 
        OnCatchUnique();
    }
    // 離された時の共通処理
    public void OnRelease()
    { 
        if(!_isCatch)
            return;
        _isCatch = false; 
        _onDoReleaseCallback?.Invoke();
        OnReleaseUnique();
    }

    public CatchableObj TryGetAlternate(){ return _alternate; }
    public Transform GetCatchWebParent(){ return _catchWebParent; }
    public Vector3 GetWebScale(){ return _webScale; }
    public Vector3 GetWebPosition(){ return _webPosition; }
    public Vector3 GetWebRotate(){ return _webRotate; }
    public Rigidbody GetRigidbody(){ return _rigidbody; }
    public Rigidbody TryGetAlternateRigidbody()
    { 
        if(_alternate != null)
            return _alternate.GetRigidbody();
        return null; 
    }
    public bool IsCatch(){ return _isCatch; }
    public GameObject GetParent(){ return _parent; }
    // 壊された時の継承先の独自処理
    public void OnBreak()
    {
        OnBreakUnique();
        _onBreakCallback?.Invoke();
        _isBroken = true;
    }
    public void AddOnBreakCallback(UnityAction onBreak)
    {
        if(_onBreakCallback == null)
            _onBreakCallback = new UnityEvent();
        _onBreakCallback.AddListener(onBreak);
    }
    public void SetOnDoReleaseCallback( UnityAction onDoReleaseCallback )
    {
        _onDoReleaseCallback = onDoReleaseCallback;
    }
    public bool IsBroken(){ return _isBroken; }
    public GameObject GetGameObject(){ return this.gameObject; }
    // これが特定の子クラスならそれを返す
    public HumanChild TryGetHumanChild()
    {
        if(typeof(HumanChild) == this.GetType())
            return (HumanChild)this;
        return null;
    }
    public Human TryGetParentHuman()
    {
        HumanChild humanChild = TryGetHumanChild();
        if(humanChild == null)
            return null;
        return humanChild.Gethuman();
    }
    public void AddOnInitialize( UnityAction onInitialize)
    {
        if(_onInitialize == null)
            _onInitialize = new UnityEvent();
        _onInitialize.AddListener(onInitialize);
    }
    public float GetBeforeVelocityMagnitude()
    {
        return _beforevelocity.magnitude;
    }
    public Vector3 GetBeforeVelocity()
    {
        return _beforevelocity;
    }
    public void FastSwipe(){ _fastSwipedTime = FAST_SWIPED_TIME; }
    public bool IsFastSwiped()
    { 
        if( 0 < _fastSwipedTime )
            return true;
        return false;
    }
    // ---------- Private関数 ----------
    // Startの、継承先の独自処理
    protected virtual void StartUnique(){  }
    protected virtual void UpdateUnique(){  }
    // 捕まった時の継承先の独自処理
    protected virtual void OnCatchUnique(){  }
    // 離された時の継承先の独自処理
    protected virtual void OnReleaseUnique(){  }
    protected virtual void OnBreakUnique(){  }
    protected void SetParent( GameObject parent ){ _parent = parent; }
    //　衝突相手が(他の)Humanかチェック
    protected bool IsCollisionHuman( Collision collision )
    {
        // 自分と相手が同じレイヤーならNOと返す
        if(this.gameObject.layer == collision.gameObject.layer)
            return false;

        // 衝突相手がHumanなら、そのフラグを立てる
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human1"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human2"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human3"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human4"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human5"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human6"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human7"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human8"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human9"))
            return true;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Human10"))
            return true;

        return false;
    }
}
