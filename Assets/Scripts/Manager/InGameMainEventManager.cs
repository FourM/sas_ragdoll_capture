using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 孫クラスがインゲームマネージャーの処理を呼び出すための窓口(インターフェース)
/// ゲームマネージャーがイベントを設定する。孫クラスがイベントを呼び出す
/// 直接呼び出すより孫の独立性を高められる
/// </summary>
public interface InGameMainEventManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    public void OnEnemyAttackStart()
    {
        Debug.Log("敵の攻撃");
    }
    public void OnEnemyAttackCansel()
    {

    }
    public void OnEnemyAttackHit()
    {

    }
    // ---------- Private関数 ------------------------
}
