using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronBall : CatchableObj
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("捕まった時の質量")] private float _catchMass = 1f;
    [SerializeField, Tooltip("離された時の質量")] private float _releaseMass = 1000f;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    private Vector3 _velocity = Vector3.zero;
    // ---------- Unity組込関数 ----------
    private void OnCollisionEnter(Collision collision)
    {
        if(GetRigidbody() == null)
            return;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            return; 

        CatchableObj catchableObj = GameDataManager.GetCatchableObj(collision.transform.gameObject);
        GameObject parentObj = null;
        CatchableObj parentCatchableObj = null;

        if(catchableObj != null)
            parentObj = catchableObj.GetParent();
        if(parentObj != null)
            parentCatchableObj = GameDataManager.GetCatchableObj(parentObj);

        float killShockStrength = GameDataManager.GetKillShockStrength();
        // killShockStrength = killShockStrength * 5f / 7f;

        HumanChild humanChild = null;

        if(catchableObj != null)
        {
            humanChild = catchableObj.TryGetHumanChild();

            bool isBroken = catchableObj.IsBroken();
            float collisionSpeed = GetBeforeVelocityMagnitude();
            // ぶっ壊す
            if(killShockStrength <= collisionSpeed)
            {
                if(parentCatchableObj != null)
                    parentCatchableObj.OnBreak();
                
                if(catchableObj.GetRigidbody() != null)
                    catchableObj.GetRigidbody().velocity = GetBeforeVelocity().normalized * 10f;
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
    protected override void OnCatchUnique()
    { 
        if(_catchMass < 0)
            return;
        Rigidbody rigidbody = GetRigidbody();
        rigidbody.mass = _catchMass;
    }
    protected override void OnReleaseUnique()
    { 
        if(_releaseMass < 0)
            return;
        Rigidbody rigidbody = GetRigidbody();
        rigidbody.mass = _releaseMass;
    }
}
