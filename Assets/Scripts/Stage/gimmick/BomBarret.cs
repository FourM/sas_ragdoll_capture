using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// 爆発するドラム缶
/// </summary>
public class BomBarret : CatchableObj
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("爆発エフェクト")] private ParticleSystem _effectExplosion = default;
    [SerializeField, Tooltip("爆発攻撃判定")] private ChildTrigger _triggerExplotion = default;
    private Tween _bomTween = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    // ドラム缶本体の衝突処理
    private void OnCollisionEnter(Collision collision)
    {
        if(0 < GameDataManager.GetMutekiTime())
            return;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            return; 
        if(IsBroken())
            return;

        float collisionSpeed = GetBeforeVelocityMagnitude();
        // 衝突相手が掴めるものか、さらにはHumanかの可否と情報を取得
        bool isHuman = false;
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(collision.gameObject);
        HumanChild humanChild = null;
        if(collitionChatchableObj != null)
        {
            humanChild = collitionChatchableObj.TryGetHumanChild();
            collisionSpeed += collitionChatchableObj.GetBeforeVelocityMagnitude();
        }
        isHuman = IsCollisionHuman(collision);

        // 爆発する衝撃の強さ
        float killShockStrength = GameDataManager.GetKillShockStrength() * 2f / 7f;
        // Humanに当たった時は爆発する衝撃のラインをゆるくする
        // if(isHuman)
        //     killShockStrength = killShockStrength * 2f / 7f;
        
        // 致死衝撃を受けたか否か
        if( killShockStrength <= collisionSpeed)
        {
            // Debug.Log("killShockStrength:" + killShockStrength + ", " + collisionSpeed);
            // 致死衝撃を受けた処理
            OnBreak();
            if(isHuman)
                collitionChatchableObj.OnBreak();
        }
    }

    // 爆風部分のトリガー処理
    private void ExplosionTrigger(Collider other)
    {
        // 壁は無視
        if(other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            return;

        // 衝突相手が掴めるものか、さらにはHumanかの可否と情報を取得
        bool isHuman = false;
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(other.gameObject);

        // 爆風を受けたもの全部壊す
        if(collitionChatchableObj != null && !collitionChatchableObj.IsBroken())
            collitionChatchableObj.OnBreak();
        // 爆風を受けたものを吹き飛ばす
        Vector3 velocity = Vector3.zero;
        velocity = other.transform.position - this.transform.position;
        velocity = velocity.normalized * 10f;
        if(other.attachedRigidbody != null)
            other.attachedRigidbody.velocity = velocity;
    }

    private void OnDisable()
    {
        if(_bomTween != null)
            _bomTween.Kill();
    }
    // ---------- Private関数 ----------
    protected override void StartUnique(){
        // 爆風のトリガー処理
        _triggerExplotion.AddCallbackOnTriggerEnter(ExplosionTrigger);
        _triggerExplotion.gameObject.SetActive(false);
    }
    protected override void OnBreakUnique()
    {
        _effectExplosion.transform.parent = this.transform.parent;
        _triggerExplotion.gameObject.SetActive(true);
        _effectExplosion.Play();
        // GetRigidbody().isKinematic = true;
        _bomTween = DOVirtual.DelayedCall(0.2f, ()=>{ this.gameObject.SetActive(false); });
        
        _onDoReleaseCallback?.Invoke();
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
}
