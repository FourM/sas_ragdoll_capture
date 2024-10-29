using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ヒューマノイドの破壊可能パーツの挙動
/// 破壊前はSkinnedMeshRenderer、破壊後はMeshRendererを使用して表示する
/// </summary>
public class BreakableHumanoidParts : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("親パーツ")] private BreakableHumanoidParts _parentParts = default;
    [SerializeField, Tooltip("スキンメッシュレンダラー")] private SkinnedMeshRenderer _skinnedMeshRenderer = null;
    [SerializeField, Tooltip("メッシュフィルター")] private MeshFilter _meshFilter = null;
    [SerializeField, Tooltip("メッシュレンダラー")] private MeshRenderer _meshRenderer = null;
    [SerializeField, Tooltip("メッシュコライダー")] private Collider _collider = null;
    [SerializeField, Tooltip("リジッドボディ")] private Rigidbody _rigidbody = null;
    private UnityEvent _onBreakCallback = default;
    private bool _isBreak = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start(){

        if(_skinnedMeshRenderer == null)
            _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if(_skinnedMeshRenderer == null)
        {
            Debug.Log("エラー：SkinnedMeshRendererがアタッチされていません！:" + this.name);
            return;
        }
        // これをゲームマネージャーに登録
        // GameDataManager.AddBreakableHumanoidPartsDic(this.gameObject, this);

        // アタッチされてないコンポーネントを取得する。取得できないコンポーネントは動的に作成する
        if(_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();
        if(_meshFilter == null)
            _meshFilter = this.gameObject.AddComponent<MeshFilter>();
        
        if(_meshRenderer == null)
            _meshRenderer = GetComponent<MeshRenderer>();
        if(_meshRenderer == null)
            _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();


        // コライダーが登録されていないなら、メッシュコライダーをつける
        if(_collider == null)
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if(meshCollider == null)
                meshCollider = this.gameObject.AddComponent<MeshCollider>();
            if(meshCollider != null)
            {
                meshCollider.convex = true;
                _collider = meshCollider;
            }
        }
        
        if(_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
        if(_rigidbody == null)
            _rigidbody = this.gameObject.AddComponent<Rigidbody>();

        
        // SkinnedMeshRendererからメッシュとマテリアルの情報取得
        if(_meshFilter.sharedMesh == null)
            _meshFilter.sharedMesh = _skinnedMeshRenderer.sharedMesh;
        _meshRenderer.materials = _skinnedMeshRenderer.materials;

        // 破壊後用の設定を無効化しておく
        _collider.enabled = false;
        _rigidbody.isKinematic = true;
        // _meshRenderer.enabled = false;

        _skinnedMeshRenderer.enabled = false;
        _meshRenderer.enabled = true;

        if(_parentParts != null && _parentParts != this)
        {
            // 親が壊れたらこれも壊れる
            _parentParts.AddOnBreakCallback(()=>{
                ParentBreak();
            });
        }
    }

    private void Update(){
        if(_skinnedMeshRenderer == null)
            return;
    }
    // ---------- Public関数 ----------
    public void AddOnBreakCallback( UnityAction callback )
    {
        if( _onBreakCallback == null )
            _onBreakCallback = new UnityEvent();
        _onBreakCallback.AddListener(callback);
    }

    // 親が壊れた
    public void ParentBreak()
    {
        if(_isBreak)
            return;
        this.transform.parent = _parentParts.transform;
        
        CommonBreak();
    }
    // これが壊れた
    public void Break( Vector3 velocity )
    {
        if(_isBreak)
            return;
        if(GameDataManager.GetStage() != null)
            this.transform.parent = GameDataManager.GetStage().transform;
        CommonBreak();
        // 物理演算を有効化
        _collider.enabled = true;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.velocity = velocity;
    }
    // 壊れた時の共通処理
    public void CommonBreak()
    {
        _isBreak = true;
        _onBreakCallback?.Invoke();
        _skinnedMeshRenderer.enabled = false;
        _meshRenderer.enabled = true;
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    public void SetMaterial( Material material )
    {
        _meshRenderer.material = material;
    }
    // ---------- Private関数 ----------
}
