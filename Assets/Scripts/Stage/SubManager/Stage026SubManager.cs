using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 火炎放射
/// </summary>
public class Stage026SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _killTriggerOver;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _killTriggerUnder;
    [SerializeField, Tooltip("トリガー")] private float _forceX = 0f;
    [SerializeField, Tooltip("トリガー")] private float _forceY = 0f;
    [SerializeField, Tooltip("煙エフェクト")] private List<ParticleSystem> _effectSmokeList;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    protected override void InitializeUnique()
    {
        // _killTriggerOver.AddCallbackOnTriggerEnter(TryPlaySmoke);
        // _killTriggerUnder.AddCallbackOnTriggerEnter(TryPlaySmoke);
        _killTriggerOver.AddCallbackOnTriggerStay((Collider other)=>{
            TryAddForce(other, _forceX, _forceY);
        });
        _killTriggerUnder.AddCallbackOnTriggerStay((Collider other)=>{
            TryAddForce(other, -_forceX, _forceY);
        });
    }
    private void TryAddForce(Collider other, float forceX, float forceY)
    {
        HumanChild humanChild = null;
        Human human = null;
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(other.gameObject);
        
        if(collitionChatchableObj != null)
            humanChild = collitionChatchableObj.TryGetHumanChild();
        if(humanChild != null)
        {
            human = humanChild.Gethuman();

            humanChild.GetRigidbody().AddForce(new Vector3(forceX, forceY, 0), ForceMode.Acceleration);
            human.GetRigidbody().AddForce(new Vector3(forceX, forceY, 0), ForceMode.Acceleration);
        }
        else
        {
             // 禁忌
            Rigidbody rigidbody = other.GetComponent<Rigidbody>();
            if(rigidbody != null)
            {
                rigidbody.AddForce(new Vector3(forceX, forceY, 0), ForceMode.Acceleration);
            }
        }
    }
    private void TryPlaySmoke(Collider other)
    {
        Human human = null;
        HumanChild humanChild = null;
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(other.gameObject);
        
        if(collitionChatchableObj != null)
            humanChild = collitionChatchableObj.TryGetHumanChild();
        if(humanChild != null)
            human = humanChild.Gethuman();
        if( human == null )
            return;
        if( human.IsDead() )
            return;

        human.OnRelease();

        human.OnBreak();

        int index = _stage.GetHumanNum();

        ParticleSystem _effectSmoke = _effectSmokeList[index];

        Vector3 pos = other.transform.position;
        pos.y = _effectSmoke.transform.position.y;
        _effectSmoke.transform.position = pos;
        _effectSmoke.Play();
    }
}
