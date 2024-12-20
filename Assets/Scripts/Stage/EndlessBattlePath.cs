using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// ここを通過したらプレイヤーはどう動くか
public enum EnterPlayerState
{
    none,       // 今の状態を続ける
    battle,     // 応戦状態に入る
    move        // 移動する
}
// ここを通過したときプレイヤーは何を見るか
public enum EnterLook
{
    none,       // 今の状態を続ける
    front,      // 前方を見る
    next,       // 次を見る
    look,       //　指定した場所を見る
}
// ここを通過後、クリアしたらorクリアしてたらプレイヤーは何を見るか
public enum ClearLook
{
    none,       // 今の状態を続ける
    front,      // 前方を見る
    next,       // 次を見る
    look,       //　指定した場所を見る
}

public class EndlessBattlePath : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger = null;
    [SerializeField, Tooltip("見るとこ")] private Transform _lookPos = null;
    [SerializeField, Tooltip("ここを通過したらプレイヤーはどう動くか")] private EnterPlayerState _enterPlayerState = EnterPlayerState.move;
    [SerializeField, Tooltip("ここを通過したときプレイヤーは何を見るか")] private EnterLook _enterLook = EnterLook.none;
    [SerializeField, Tooltip("ここを通過後、クリアしたらorクリアしてたらプレイヤーは何を見るか")] private ClearLook _clearLook = ClearLook.next;
    private bool _isPath = false;

    public EnterPlayerState EnterPlayerState{ get{ return _enterPlayerState; } }
    public EnterLook EnterLook{ get{ return _enterLook; } }
    public ClearLook ClearLook{ get{ return _clearLook; } }
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    public void Initialize()
    {

    }
    public void AddCallbackOnTriggerEnter(UnityAction<Collider> onTriggerEnter)
    {
        _childTrigger.AddCallbackOnTriggerEnter(( Collider collider )=>
        {
            if(!_isPath)
            {
                onTriggerEnter(collider);
                _isPath = true;
            }
        });
    }
    public Transform LookPos(){ return _lookPos; }
    // ---------- Private関数 ------------------------
}
