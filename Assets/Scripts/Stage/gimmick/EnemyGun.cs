using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵の銃
public class EnemyGun : CatchableObj
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("弾プレハブ")] private EnemyGunBurret _bulletPrefab = null;
    [SerializeField, Tooltip("発射位置")] private Transform _shotPos = null;
    [SerializeField, Tooltip("弾の寿命")] private float _bulletDuration = 5f;
    [SerializeField, Tooltip("弾速")] private float _shotSpd = 5f;
    [SerializeField, Tooltip("発射感覚")] private float _shotInterval = 0.7f;
    [SerializeField, Tooltip("弾数")] private int _shotNum = -1;
    [SerializeField, Tooltip("発射エフェクト")] ParticleSystem _effectShot = null;
    [SerializeField, Tooltip("破壊する衝撃の強さ係数")] private float _killShockStrengthFactor = 1f;
    private float _shotWait = 0f;
    private bool _isBurretInfinity = false;
    private bool _isShot = false;
    private Transform _burretParent = null;
    private Human _human = null;
    private Transform _target = null;
    private float _targetForward = 0f;
    private bool _haveHuman = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    protected override void StartUnique()
    {
        // 初期段数が0以下なら段数無限
        if(_shotNum <= 0)
            _isBurretInfinity = true;
    }

    protected override void UpdateUnique()
    {
        // 撃つ気がない
        if(!_isShot)
            return;
        // 弾切れ
        if( _shotNum <= 0 && !_isBurretInfinity )
            return;
        if( _human.IsBroken() )
            return;

        _shotWait -= Time.deltaTime;
        // 待機中
        if( 0 < _shotWait)
            return;

        // 発射あ！！
        EnemyGunBurret burret = Instantiate(_bulletPrefab);
        burret.Initialize();
        burret.SetUp(_shotPos.position, this.transform.forward * _shotSpd, _burretParent, this.transform.rotation, _bulletDuration, _target, _targetForward);
        _shotWait = _shotInterval;
        _effectShot?.Play();

        // 弾数消費
        if(!_isBurretInfinity)
            _shotNum--;
        // 発射待機
        _shotWait = _shotInterval;
    }

    // 銃身の衝突処理(メイスからのコピペ)
    private void OnCollisionEnter(Collision collision)
    {
        if(0 < GameDataManager.GetMutekiTime())
            return;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            return; 
        // Humanが持っているなら何もしない
        if(GetRigidbody().isKinematic == false)
            return;
        
        // 衝突相手が掴めるものか、さらにはHumanかの可否と情報を取得
        bool isHuman = false;
        CatchableObj catchableObj = GameDataManager.GetCatchableObj(collision.gameObject);
        CatchableObj parentCatchableObj = null;
        GameObject parentObj = null;
        HumanChild humanChild = null;

        if(catchableObj != null)
            humanChild = catchableObj.TryGetHumanChild();
        if(catchableObj != null)
            parentObj = catchableObj.GetParent();
        if(parentObj != null)
            parentCatchableObj = GameDataManager.GetCatchableObj(parentObj);
        isHuman = IsCollisionHuman(collision);

        // 破壊する衝撃の強さ
        float killShockStrength = GameDataManager.GetKillShockStrength() * _killShockStrengthFactor;

        if(catchableObj != null)
        {
            bool isBroken = catchableObj.IsBroken();
            // ぶっ壊す
            if(killShockStrength <= GetRigidbody().velocity.magnitude)
            {
                if(parentCatchableObj != null)
                    parentCatchableObj.OnBreak();
                
                if(catchableObj.GetRigidbody() != null)
                    catchableObj.GetRigidbody().velocity = GetRigidbody().velocity * 5f;
                // catchableObj.OnBreak();
                catchableObj.OnDamage(110);

                if(humanChild != null)
                    humanChild.SetImpactPos(collision.GetContact(0).point);
            }
            bool isKill = catchableObj.IsBroken();
        }
    }
    // ---------- Public関数 ----------
    public void ShotStart()
    {
        _isShot = true;
    }
    // 弾丸の親設定
    public void SetBurretParent(Transform burretParent)
    {
        _burretParent = burretParent;
    }

    public Vector3 GetShotPos(){ return _shotPos.position; }
    public float GetShotSpd(){ return _shotSpd; }
    public void SetHuman(Human human)
    { 
        _human = human; 
        _haveHuman = true;
        _human.AddOnCatch(()=>
        {
            _isShot = false;
            // 持ち主から切り離す
            this.transform.parent = _human.transform.parent.parent;
            Rigidbody rigidbody = GetRigidbody(); 
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            _haveHuman = false;
            HumanChild humanChild = null;
        });
    }
    public void SetTarget(Transform target, float targetForward = 0f)
    { 
        _target = target; 
        _targetForward = targetForward;
    }

    // ---------- Private関数 ----------
    // メイスを奪った時の処理
    protected override void OnCatchUnique()
    {
        if(_haveHuman)
        {
            _haveHuman = false;
            GameObject parent = this.transform.parent.gameObject;

            // 持ち主から切り離す
            this.transform.parent = _human.transform.parent.parent;
            Rigidbody rigidbody = GetRigidbody(); 
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;

            HumanChild humanChild = null;

            _human.Flinch();

            _isShot = false;
        }

        _rigidbody.excludeLayers = LayerMask.GetMask("");
    }

    protected override void OnReleaseUnique()
    {
        
    }
}
