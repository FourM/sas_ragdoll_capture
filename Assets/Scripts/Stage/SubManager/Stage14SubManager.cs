using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// ロープで宙ぶらりんステージ
/// </summary>
public class Stage14SubManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージ")] private GameStage _stage;
    [SerializeField, Tooltip("おててL")] private Transform _handLChild;
    [SerializeField, Tooltip("左手SpringJoint")] private SpringJoint _springjointL;
    [SerializeField, Tooltip("SpringJointのトリガー")] private ChildTrigger _jointLBreakTrigger;
    private bool _isInitialize = false;
    private bool _isCatch = true;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);
    }

    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;

        Human human = _stage.GetHuman(0);

        _springjointL.connectedBody = human.GetParts(HumanParts.handL).GetRigidbody();
        // _springjointR.connectedBody = human.GetParts(HumanParts.handR).GetRigidbody();

        _handLChild.parent = human.GetParts(HumanParts.handL).transform;
        human.GetParts(HumanParts.handL).OnCatch();
        human.GetParts(HumanParts.handL).SetIsFixMass(true);
        // 紐がちぎれたら、手の質量固定化を解除する
        _jointLBreakTrigger.AddCallbackOnJointBreak((float breakForce)=>{
            human.GetParts(HumanParts.handL).SetIsFixMass(false);
            human.ChangePartsMass();
            _isCatch = false;
            human.SetIsOtherCatch(false);
        });

        human.SetIsOtherCatch(true);

        human.AddCallbackOnRelease(()=>{
            if(_isCatch)
            {
                human.GetParts(HumanParts.handL).OnCatch();
            }
        });


        _handLChild.localPosition = Vector3.zero;

        _handLChild.localEulerAngles = Vector3.zero;

        _handLChild.localScale = Vector3.zero;
    }
}
