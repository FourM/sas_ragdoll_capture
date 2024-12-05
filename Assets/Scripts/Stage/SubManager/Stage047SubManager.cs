using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// お馬さん
/// </summary>
public class Stage047SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("トリガー")] private float _forceZ = 0f;
    [SerializeField, Tooltip("トリガー")] private float _forceY = 0f;
    [SerializeField, Tooltip("トリガー")] private RigidbodyConstraints _gimmickOffHumanConst = default;
    [SerializeField, Tooltip("トリガー")] private List<Transform> _randomShakeList;
    [SerializeField, Tooltip("トリガー")] private float _shakePos;
    [SerializeField, Tooltip("トリガー")] private float _shakeTime;
    [SerializeField, Tooltip("トリガー")] private int _shakeNum;
    private List<Tween> _tweenList = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void OnDisable() {
        if(_tweenList != null)
        {
            for(int i = 0; i < _tweenList.Count; i++)
            {
                _tweenList[i].Kill();
            }
        }
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    protected override void InitializeUnique()
    {
        _tweenList = new List<Tween>();
        Tween tween = null;

        _childTrigger.AddCallbackOnTriggerStay((Collider other)=>{
            TryAddForce(other, _forceZ, _forceY);
        });
        
        for(int i = 0; i < _randomShakeList.Count; i++)
        {
            tween = _randomShakeList[i].DOPunchPosition(new Vector3(0, _shakePos, 0), _shakeTime, _shakeNum).SetLoops(-1, LoopType.Restart);
            _tweenList.Add(tween);
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
                if(human != null && !human.IsEnableAnimation())
                {
                    // 接地中、起き上がり動作の初期なら奥に行く
                    if(!human.IsFollowBaseLock())
                    {
                        human.PartsActiion((HumanChild humanChild)=>{
                            humanChild.GetRigidbody().constraints = RigidbodyConstraints.None;
                        });
                        rigidbody.AddForce(new Vector3(0, forceY, forceZ), ForceMode.VelocityChange);
                    }
                    else
                    {
                        human.PartsActiion((HumanChild humanChild)=>{
                            humanChild.GetRigidbody().constraints = _gimmickOffHumanConst;
                        });
                    }
                }
            }
            
            if(human == null)
                rigidbody.AddForce(new Vector3(0, forceY, forceZ), ForceMode.VelocityChange);
        }
    }
}
