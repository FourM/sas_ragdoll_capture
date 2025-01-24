using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// ここを通過したらプレイヤーはどう動くか
public enum EnterPlayerState
{
    none,       // 今の状態を続ける
    battle,     // 応戦状態に入る
    battleStop, // 応戦状態に入る(足を止める)
    move,       // 移動する
    dash        // 移動する(早い)
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
    [SerializeField, Tooltip("クリア判定に用いる敵 空ならそのセグメントの敵全てが対象になる")] private List<HumanHub> _refHumanList = default;
    [SerializeField, Tooltip("クリア判定に用いるオブジェクト")] private List<CatchableObj> _refCatchableObjList = default;
    [SerializeField, Tooltip("クリアしたら走るか")] private bool _isClearDash = false;
    private bool _isPath = false;
    private UnityEvent _onPass = null;

    public EnterPlayerState EnterPlayerState{ get{ return _enterPlayerState; } }
    public EnterLook EnterLook{ get{ return _enterLook; } }
    public ClearLook ClearLook{ get{ return _clearLook; } }
    public bool IsClearDash{ get{ return _isClearDash; } }
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

                _onPass?.Invoke();
            }
        });
    }
    public Transform GetLookPos(){ return _lookPos; }
    // パス別のクリア判定をするか
    public bool IsRefPathClear(){ return 0 < _refCatchableObjList.Count || 0 < _refHumanList.Count; }
    public bool IsPathClear()
    { 
        for(int i = 0; i < _refCatchableObjList.Count; i++)
        {
            if(_refCatchableObjList[i] != null && !_refCatchableObjList[i].IsBroken())
                return false;
        }
        for(int i = 0; i < _refHumanList.Count; i++)
        {
            Human human = _refHumanList[i].GetActiveHuman();
            if( human != null && !human.IsBroken() )
                return false;
        }
        return true;
    }
    // プレイヤーが通過した時の処理
    public void AddOnPassCallback(UnityAction onPass)
    {
        if(_onPass == null)
            _onPass = new UnityEvent();
        _onPass.AddListener(onPass);
    }
    // ---------- Private関数 ------------------------
}
