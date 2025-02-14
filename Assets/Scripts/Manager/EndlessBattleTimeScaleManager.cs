using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// エンドレスバトルのスローモーションマネージャー
public static class EndlessBattleTimeScaleManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    private static List<IAttacker> _listIAttacker = null;
    public static List<IAttacker> HashIAttacker{ get{ return _listIAttacker; } }
    public static bool IsSlow{ get{
        // 攻撃が当たりそうな要素が一つもいないならFalse
        if(_listIAttacker == null )
            return false;
        
        // 自衛処理：参照しているインターフェースが消えていたらそれを弾く
        _listIAttacker.RemoveAll(i => i == null);

        if(_listIAttacker.Count <= 0 )
            return false;

        return true;
    } }
    // private static bool IsSlowLock = false;
    private static bool _beforeIsSlow = false;
    public static event UnityAction OnSlow;
    public static event UnityAction OnResume;
    public static event UnityAction OnAttackCansel;
    // private static UnityEvent _onSlow = null;
    // public static event UnityAction OnSlow
    // {   add{
    //         if (_onSlow == null)
    //             _onSlow = new UnityEvent();
    //         _onSlow.AddListener(value);
    //     }
    //     remove{ _onSlow?.RemoveListener(value); }
    // }
    // private static UnityEvent _onResume = null;
    // public static event UnityAction OnResume
    // {   add{
    //         if (_onResume == null)
    //             _onResume = new UnityEvent();
    //         _onResume.AddListener(value);
    //     }
    //     remove{ _onResume?.RemoveListener(value); }
    // }
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    public static void Initialize()
    {
        Reset();
    }

    public static void Reset()
    {
        if(_listIAttacker != null)
        {
            HashRemoveAll();
        }
        _listIAttacker = new List<IAttacker>();
        Time.timeScale = 1f;
    }

    // もうすぐ攻撃を当ててくる敵を登録
    public static void RegistIAttacker( IAttacker iAttacker )
    {
        if(_listIAttacker == null)
            _listIAttacker = new List<IAttacker>();
        if(_listIAttacker.Contains(iAttacker))
            return;
        _listIAttacker.Add(iAttacker);
        UpdateSlowMotion();
    }
    // もうすぐ攻撃を当ててくる敵の登録から解除
    public static void UnRegistIAttacker( IAttacker iAttacker )
    {
        if(_listIAttacker == null || !_listIAttacker.Contains(iAttacker))
            return;
        _listIAttacker.Remove(iAttacker);
        OnAttackCansel?.Invoke();
        UpdateSlowMotion();
    }

    private static void UpdateSlowMotion()
    {
        // もうすぐ攻撃を当ててくる敵がいないならスローモーションを解除
        if(!IsSlow)
        {
            Time.timeScale = 1f;
            if(_beforeIsSlow != IsSlow)
            {
                _beforeIsSlow = IsSlow;
                OnResume?.Invoke();
            }
        }
        // もうすぐ攻撃を当ててくる敵がいるならスローモーションにする
        else
        {
            Time.timeScale = 0.333f;
            if(_beforeIsSlow != IsSlow)
            {
                _beforeIsSlow = IsSlow;
                OnSlow?.Invoke();
            }
        }
    }

    private static void HashRemoveAll()
    {
        _listIAttacker.Clear();
        // List<IAttacker> toRemove = new List<IAttacker>();

        // foreach (var item in _listIAttacker)
        // {
        //     toRemove.Add(item);
        // }

        // foreach (var item in toRemove)
        // {
        //     _listIAttacker.Remove(item);
        // }
    }

    //　スロー時にプレイヤーが見る対象
    public static Transform GetPrimaryLookAtTarget()
    {
        for(int i = 0; i < _listIAttacker.Count; i++)
        {
            Transform lookTarget = _listIAttacker[i].AttackerBaseClass.LookTransform;
            if(lookTarget != null)
                return lookTarget;
        }
        return null;
    }

    // 敵の攻撃が１つでももうすぐ当たりそうならスローモーションにする
    // 捕まってる敵はスローモーションの影響を受けない
    // プレイヤーの移動はスローモーションの影響を受ける
    // プレイヤーが物を掴んで振り回す処理はスローモーションの影響を受けない
    // もうすぐ当たりそうな敵の攻撃がなくなったらスローモーションを解除する

    // もうすぐ当たりそうな敵の攻撃とは？
    // 　近接攻撃：あとn秒で殴られる
    //  　　　
    // 　弾丸　　：あとn秒で当たる　→　どう判定する？弾丸の距離と、プレイヤーと弾丸の相対速度から算出する

    // もうすぐ当たりそうな敵の攻撃がなくなったかの判定は？
    // 　1.敵の攻撃がもうすぐ当たりそうならその敵や弾丸からトリガーする
    //      その敵をこのマネージャーに保存し、スローモーション判定する
    //   2.もうすぐ当たりそうな状態を解除されたらその敵や弾丸からトリガーする
    //       その敵をこのマネージャーの保存から破棄し、スローモーション判定する
    //   3. スローモーション判定
    //       もうすぐ攻撃が当たりそうな敵が一体でもいるならスローモーションにする
    //       一体もいないならスローモーションを解除する
}
