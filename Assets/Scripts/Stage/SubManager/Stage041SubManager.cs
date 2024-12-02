using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 風車に捕まってる
/// </summary>
public class Stage041SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("羽")] private Transform _fan = default;
    [SerializeField, Tooltip("SpringJoint")] private List<SpringJoint> _ListSpringjoint;
    [SerializeField, Tooltip("風車の回転時間")] private float _duration = default;
    [SerializeField, Tooltip("SpringJointのトリガー")] private List<ChildTrigger> _jointBreakTriggerList;
    private bool _isCatch = true;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    protected override void InitializeUnique()
    {
        Tween tween = _fan.DOLocalRotate(new Vector3(0, 0, 360f), _duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);  
       _tweenList.Add(tween);


        for(int i = 0; i < _ListSpringjoint.Count; i++)
        {
            SpringJoint springJoint = _ListSpringjoint[i];
            ChildTrigger jointBreakTrigger = _jointBreakTriggerList[i];

            Human human = _stage.GetHuman(i);
            springJoint.connectedBody = human.GetParts(HumanParts.handL).GetRigidbody();

            // _fan.parent = human.GetParts(HumanParts.handL).transform;
            human.GetParts(HumanParts.handL).OnCatch();
            human.GetParts(HumanParts.handL).SetIsFixMass(true);
            // // 紐がちぎれたら、手の質量固定化を解除する
            jointBreakTrigger.AddCallbackOnJointBreak((float breakForce)=>{
                human.GetParts(HumanParts.handL).SetIsFixMass(false);
                human.ChangePartsMass();
                _isCatch = false;
                human.SetIsOtherCatch(false);
            });
            human.SetIsOtherCatch(true);
            human.AddCallbackOnCatch(()=>
            {
                if(springJoint != null)
                {
                    springJoint.breakForce = 0;
                    springJoint = null;
                }            
            });

            human.AddCallbackOnRelease(()=>{
                if(_isCatch)
                {
                    human.GetParts(HumanParts.handL).OnCatch();
                }
            });
        }
    }
}
