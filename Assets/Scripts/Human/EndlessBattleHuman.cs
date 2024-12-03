using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessBattleHuman : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("Hub")] private HumanHub _humanHub = default;
    [SerializeField, Tooltip("Hub")] private ChildTrigger _childTrigger = default;
    private Human _activeHuman = null;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake() {
        _humanHub.AddOnInitialize(Initialize);
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void Initialize()
    {
        _activeHuman = _humanHub.GetActiveHuman();
        _childTrigger.transform.parent = _activeHuman.transform;
        // _childTrigger.AddCallbackOnTriggerEnter(()=>{
        //         // Debug.Log("あーあ");
        //         // if(!_activeHuman.IsCatch() && !_activeHuman.IsDead() && _activeHuman.IsGround() && _activeHuman.IsEnableAnimation())
        //         //     Debug.Log("攻撃！");
        // });      
    }
}
