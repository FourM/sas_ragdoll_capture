using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mace : CatchableObj
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    // [SerializeField, Tooltip("hoge")] private int hoge = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // メイスの衝突処理
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
        float killShockStrength = GameDataManager.GetKillShockStrength() * 1f / 7f;

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
                catchableObj.OnBreak();

                if(humanChild != null)
                    humanChild.SetImpactPos(collision.GetContact(0).point);
            }
            bool isKill = catchableObj.IsBroken();

            if(!isBroken && humanChild != null)
            {
                // FirebaseManager.instance.EventCrashed(GetRigidbody().velocity.magnitude, isKill);
            }
        }
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    // メイスを奪った時の処理
    protected override void OnCatchUnique()
    {
        GameObject parent = this.transform.parent.gameObject;

        // 持ち主から切り離す
        this.transform.parent = GameDataManager.GetStage().transform;
        Rigidbody rIgidbody = GetRigidbody(); 
        rIgidbody.isKinematic = false;
        rIgidbody.useGravity = true;
        rIgidbody.mass = 3;

        HumanChild humanChild = null;

        Human human = null;

        // 位置の補正
        CatchableObj obj = GameDataManager.GetCatchableObj(parent);
        if( obj != null )
            humanChild = obj.TryGetHumanChild();
        if( humanChild != null )
            human = humanChild.Gethuman();
        if( human != null)
        {
            Vector3 pos = this.transform.position;
            pos.z = human.transform.position.z;
            this.transform.DOMove(pos, 0.3f).SetEase(Ease.OutBack);
        }

        // 角度の補正
        Vector3 angle = this.transform.localEulerAngles;
        angle.x = 0f;
        angle.y = 0f;
        this.transform.DORotate(angle, 0.3f).SetEase(Ease.OutBack);
        DOVirtual.DelayedCall(0.305f, ()=>
        {
            rIgidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        });
    }

    protected override void OnReleaseUnique()
    {
        GetRigidbody().mass = 20f;
    }
}
