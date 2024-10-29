using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage009SubManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージ")] private GameStage _stage;
    [SerializeField, Tooltip("ステージ")] private ChildTrigger _seeSaw;
    
    private bool _isInitialize = false;
    private bool _isShot = false;
    Human _human = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);
    }
    private void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;

        _human = _stage.GetHuman(0);

        // _seeSaw.AddCallbackOnCollisionStay(AddForce);
        // _seeSaw.AddCallbackOnCollisionExit(AddForce);
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void AddForce(Collision other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Human1"))
        {
            Rigidbody waistRigid = _human.GetParts(HumanParts.waist).GetRigidbody();
            Vector3 velocity = waistRigid.velocity;

            if(0f < velocity.y )
            {
                velocity.y *= 5f;
                waistRigid.AddForce(velocity, ForceMode.Acceleration);
            }
        }
    }
}
