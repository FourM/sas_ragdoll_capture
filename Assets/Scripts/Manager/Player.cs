using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("hoge")] private CinemachineDollyCart _movePath = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Start(){
        
    }

    private void Update(){

    }
    // ---------- Public関数 ------------------------- 
    public CinemachineDollyCart GetMovePath(){ return _movePath; }
    // ---------- Private関数 ------------------------
}
