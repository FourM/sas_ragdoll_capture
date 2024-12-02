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
    [SerializeField, Tooltip("捕まった後の参考constraints")] private Rigidbody _rafConstraints = null;
    private Vector3 _velocity = Vector3.zero;
    private bool _isFirstCatch = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void OnCollisionEnter(Collision collision)
    {
        if( this.gameObject.layer != LayerMask.NameToLayer("catchableNotKill") )
            GimmickOnCollisionHuman(collision, _velocity);
    }

    private void UpdateUnique()
    {
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
        }
    }
}
