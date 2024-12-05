using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// トゲの壁？が迫ってくる
/// </summary>
public class Stage038SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("トリガー")] private float _forceZ = 0f;
    [SerializeField, Tooltip("トリガー")] private float _forceY = 0f;
    [SerializeField, Tooltip("トリガー")] private RigidbodyConstraints _gimmickOffHumanConst = default;
    [SerializeField, Tooltip("トリガー")] private Transform _randomShake;
    [SerializeField, Tooltip("トリガー")] private Transform _randomShake2;
    [SerializeField, Tooltip("トリガー")] private float _shakePos;
    [SerializeField, Tooltip("トリガー")] private float _shakePos2;
    [SerializeField, Tooltip("トリガー")] private float _shakeTime;
    [SerializeField, Tooltip("トリガー")] private float _shakeTime2;
    [SerializeField, Tooltip("トリガー")] private int _shakeNum;
    [SerializeField, Tooltip("トリガー")] private int _shakeNum2;
    private Tween _shakeTween = null;
    private Tween _shakeTween2 = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    void OnDisable()
    {
        if(_shakeTween != null)
            _shakeTween.Kill();
        if(_shakeTween2 != null)
            _shakeTween2.Kill();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    protected override void InitializeUnique()
    {
        _childTrigger.AddCallbackOnTriggerStay((Collider other)=>{
            TryAddForce(other, _forceZ, _forceY);
        });

        _shakeTween = _randomShake.DOShakePosition(_shakeTime, _shakePos, _shakeNum, 1, false, false).SetLoops(-1, LoopType.Restart);
        _shakeTween2 = _randomShake2.DOShakePosition(_shakeTime2, _shakePos2, _shakeNum2, 1, false, false).SetLoops(-1, LoopType.Restart);
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
