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
    [SerializeField, Tooltip("hoge")] private CinemachineDollyCart _cinemachineDollyCart = default;
    [SerializeField, Tooltip("hoge")] private float _baseSpeed = 0f;
    private Vector3 _initPos = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake()
    {
        _initPos = this.transform.position;
        _cinemachineDollyCart.m_Speed = 0f;
    }
    private void Start(){
        
    }

    private void Update(){

    }
    // ---------- Public関数 ------------------------- 
    public CinemachineDollyCart GetMovePath(){ return _cinemachineDollyCart; }
    public void StopPathMove(){ _cinemachineDollyCart.m_Speed = 0f; }
    public void ContinuePathMove()
    { 
        _cinemachineDollyCart.m_Speed = _baseSpeed; 
    }
    public void InitPos(){ this.transform.position = _initPos; }
    public Vector3 GetInitPos(){ return _initPos; }
    // ---------- Private関数 ------------------------
}
