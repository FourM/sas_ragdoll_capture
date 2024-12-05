using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メイス持ったやつらが走ってくる
/// </summary>
public class Stage027SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("トリガー")] private List<ChildTrigger> _childTriggers;
    [SerializeField, Tooltip("トリガー")] private float _forceZ = 0f;
    [SerializeField, Tooltip("トリガー")] private float _forceY = 0f;
    [SerializeField, Tooltip("トリガー")] private RigidbodyConstraints _gimmickOffHumanConst = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    protected override void InitializeUnique()
    {
        _childTrigger.AddCallbackOnTriggerStay((Collider other)=>{
            TryAddForce(other, _forceZ, _forceY);
        });

        // メイス
        for(int i = 0; i < _childTriggers.Count; i++)
        {
            ChildTrigger trigger = _childTriggers[i];

            // 床に当たったら奥に飛んでくようになる
            trigger.AddCallbackOnCollisionStay((Collision collision)=>{
                CatchableObj chatchableObj = GameDataManager.GetCatchableObj(trigger.gameObject);
                if(chatchableObj == null)   
                    return;
                if(collision.gameObject.layer == LayerMask.NameToLayer("Floor") && !chatchableObj.IsCatch())
                {
                    trigger.GetRigidBody().constraints = RigidbodyConstraints.None;
                }
            });
        }
    }
    private void TryAddForce(Collider other, float forceZ, float forceY)
    {
        CatchableObj chatchableObj = GameDataManager.GetCatchableObj(other.gameObject);
        Human human = null;

        Rigidbody rigidbody = null;

        if( chatchableObj != null )
            rigidbody = chatchableObj.GetRigidbody();
        else
            rigidbody = other.GetComponent<Rigidbody>();

        if(rigidbody != null && (chatchableObj == null || !chatchableObj.IsCatch()))
        {
            if(other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                rigidbody.constraints = RigidbodyConstraints.None;

            // Humanなら、接地中、ぐてっとなってる〜起き上がりまでの間、奥に流される
            if(chatchableObj != null)
            {       
                human = chatchableObj.TryGetParentHuman();
                // if(other.gameObject.layer == LayerMask.NameToLayer("Human2"))
                // {
                //     Debug.Log("人だよ:" + human + ", " +  human.IsEnableAnimation());
                // }
                if(human != null && !human.IsEnableAnimation())
                {
                    // if(other.gameObject.layer == LayerMask.NameToLayer("Human2"))
                    // {
                    //     Debug.Log("アニメーションしてない人だよ");
                    // }
                    // 接地中、起き上がり動作の初期なら奥に行く
                    // if(!human.IsFollowBaseLock() && human.IsGround())
                    if(!human.IsFollowBaseLock())
                    {
                        human.PartsActiion((HumanChild humanChild)=>{
                            humanChild.GetRigidbody().constraints = RigidbodyConstraints.None;
                        });
                        // if(other.gameObject.layer == LayerMask.NameToLayer("Human3"))
                        //     Debug.Log("敵：奥に行く:" + human.IsFollowBaseLock() + ", " + human.IsGround());
                        rigidbody.AddForce(new Vector3(0, forceY, forceZ), ForceMode.VelocityChange);
                    }
                    else
                    {
                        human.PartsActiion((HumanChild humanChild)=>{
                            humanChild.GetRigidbody().constraints = _gimmickOffHumanConst;
                        });
                        // if(other.gameObject.layer == LayerMask.NameToLayer("Human3"))
                        //     Debug.Log("敵：奥に行かない:" + human.IsFollowBaseLock() + ", " + human.IsGround());
                    }
                }
            }
            
            if(human == null)
                rigidbody.AddForce(new Vector3(0, forceY, forceZ), ForceMode.VelocityChange);
        }
    }
}
