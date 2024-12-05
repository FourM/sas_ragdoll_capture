using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSkin : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("おててのアニメーション")] private Animator _handAnimator = default;
    [SerializeField, Tooltip("糸開始地点")] private Transform _webStartPos = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    public Animator GetHandAnimator(){ return _handAnimator; }
    public Transform GetWebStartPos(){ return _webStartPos; }
    // ---------- Private関数 ------------------------
}
