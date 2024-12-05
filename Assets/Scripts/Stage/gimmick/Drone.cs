using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drone : CatchableObj
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("捕まった後の参考constraints")] private Rigidbody _rafConstraints = null;
    [SerializeField, Tooltip("移動りょうX")] private float _movePosX = 0f;
    [SerializeField, Tooltip("移動時間")] private float _duration = 0f;
    [SerializeField, Tooltip("イージング")] private Ease _ease = default;
    private List<Tween> _tweenList = default;
    private Vector3 _velocity = Vector3.zero;
    private bool _isFirstCatch = false;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void OnCollisionEnter(Collision collision)
    {
        GimmickOnCollisionHuman(collision, _velocity);
    }
    protected override void StartUnique(){
        _tweenList = new List<Tween>();
        Tween tween = this.transform.DOMoveX(_movePosX, _duration).SetEase(_ease).SetLoops(-1, LoopType.Yoyo);
        _tweenList.Add(tween);
    }
    protected override void UpdateUnique()
    {
        if( 5f <= GetRigidbody().velocity.magnitude )
        {
            _velocity = GetRigidbody().velocity;
            _velocity = _velocity.normalized * 10f;
        }
        else
            _velocity = GetRigidbody().velocity;
    }
    protected override void OnDisableUnique()
    {
        KillAllTween();
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    // 掴んだら、Z移動以外は動き回せるようになる
    protected override void OnCatchUnique()
    { 
        if(!_isFirstCatch)
        {
            _isFirstCatch = true;

            Rigidbody rIgidbody = GetRigidbody();
            if(_rafConstraints == null)
                rIgidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            else
                rIgidbody.constraints = _rafConstraints.constraints;
            KillAllTween();
        }
    }
    private void KillAllTween()
    {
        if(_tweenList != null)
        {
            for(int i = _tweenList.Count - 1; 0 <= i; i--)
            {
                _tweenList[i].Kill();
                _tweenList.RemoveAt(i);
            }
        }
    }
}
