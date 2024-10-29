using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 足場
public class scaffold : CatchableObj
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("足場の上にいるHuman")] private GameStage _stage = default;
    [SerializeField, Tooltip("コライダー")] private Collider _collider = default;
    [SerializeField, Tooltip("捕まった後の参考constraints")] private Rigidbody _rafConstraints = null;
    private Human _human = null;
    private bool _isSetHuman = false;
    private Vector3 _velocity = Vector3.zero;
    private bool _isFirstCatch = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
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
        killShockStrength /= 2f;
        if(catchableObj != null )
        {
            bool isBroken = catchableObj.IsBroken();

            if(catchableObj.GetRigidbody() != null)
                catchableObj.GetRigidbody().velocity = _velocity;

            if( killShockStrength <= GetRigidbody().velocity.magnitude )
            {
                catchableObj.OnBreak();
                if(parentCatchableObj != null)
                    parentCatchableObj.OnBreak();
            }
            bool isKill = catchableObj.IsBroken();

            if(!isBroken &&
            ( collision.gameObject.layer == LayerMask.NameToLayer("Human1") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Human2") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human3") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human4") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human5") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human6") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human7") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human8") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human9") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Human10")
            ))
                FirebaseManager.instance.EventCrashed(GetRigidbody().velocity.magnitude, isKill);
        }
    }

    private void Update()
    {
        if(_stage != null && _stage.GetHuman(0) != null && _human == null && !_isSetHuman)
        {
            _human = _stage.GetHuman(0);
            _isSetHuman = true;
        }

        if( 5f <= GetRigidbody().velocity.magnitude )
        {
            _velocity = GetRigidbody().velocity;
            _velocity = _velocity.normalized * 10f;
        }
        else
            _velocity = GetRigidbody().velocity;
        
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    // 掴んだら、Z移動以外は動き回せるようになる
    protected override void OnCatchUnique()
    { 
        if(!_isFirstCatch)
        {
            _isFirstCatch = true;

            Rigidbody rIgidbody = GetRigidbody();
            if(_rafConstraints == null)
                rIgidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            else
                rIgidbody.constraints = _rafConstraints.constraints;
            
            if(_human != null)
                _human.DesableAnimation();
        }
    }
}
