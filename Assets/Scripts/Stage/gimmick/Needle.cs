using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// トゲなど、少しでも触れたものを壊すオブジェクト
/// </summary>
public class Needle : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("トリガーでも倒すか")] private bool _isTriggerKill = true;
    [SerializeField, Tooltip("rigidbody")] private Rigidbody _rigidbody = null;
    private Vector3 _beforevelocity = default;
    private Vector3 _beforePos = default;
    private Vector3 _moveSpeed = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // 触れた相手を倒す
    private void OnCollisionEnter(Collision collision)
    {
        TryBreakObject(collision.transform.gameObject, collision);
    }
    private void OnTriggerEnter(Collider collider)
    {
        if(!_isTriggerKill)
            return;
        if(collider != null && collider.transform != null && collider.transform.gameObject != null)
            TryBreakObject(collider.transform.gameObject, null);
    }
    private void Update()
    {
        if(_rigidbody != null)
        {
            _beforevelocity = _rigidbody.velocity;
            _moveSpeed = this.transform.position - _beforePos;
            _beforePos = this.transform.position;
        }
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void TryBreakObject(GameObject gameObject, Collision collision)
    {
        CatchableObj catchableObj = GameDataManager.GetCatchableObj(gameObject);
        if(catchableObj != null )
        {
            HumanChild humanChild = catchableObj.TryGetHumanChild();
            if(humanChild != null && collision != null)
            {
                humanChild.SetImpactPos(collision.GetContact(0).point);
            }

            if(_rigidbody != null)
            {
                Vector3 addVelocity = _rigidbody.velocity;
                if(addVelocity.magnitude < _beforevelocity.magnitude)
                    addVelocity = _beforevelocity;
                if(addVelocity.magnitude < _moveSpeed.magnitude)
                    addVelocity = _moveSpeed;

                if(addVelocity.magnitude < 10f)
                    addVelocity = addVelocity.normalized * 10f;
                addVelocity += new Vector3(0, 3, 0);
                if(catchableObj.GetRigidbody() != null)
                {
                    catchableObj.GetRigidbody().constraints = RigidbodyConstraints.None;
                    catchableObj.GetRigidbody().velocity = addVelocity;
                }

                Human human = catchableObj.TryGetParentHuman();
                if(human != null)
                {
                    human.GetRigidbody().constraints = RigidbodyConstraints.None;
                    human.GetRigidbody().velocity = addVelocity;
                }
            }
            catchableObj.OnBreak();
        }
    }
}
