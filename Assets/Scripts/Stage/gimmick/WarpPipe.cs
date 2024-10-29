using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPipe : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("子のコライダー")] private ChildTrigger _childCollider = default;
    [SerializeField, Tooltip("瞬間移動先")] private Transform _warpToPos = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start(){
        _childCollider.AddCallbackOnCollisionEnter(TryWarp);
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void TryWarp(Collision collision)
    {   
        // 衝突相手が掴めるものか、さらにはHumanかの可否と情報を取得
        Human human = null;
        HumanChild humanChild = null;
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(collision.gameObject);
        
        if(collitionChatchableObj != null)
            humanChild = collitionChatchableObj.TryGetHumanChild();
        if(humanChild != null)
            human = humanChild.Gethuman();
        if( human == null )
            return;
        
        human.SetPos(_warpToPos.position);
        human.OnRelease();
    }

}
