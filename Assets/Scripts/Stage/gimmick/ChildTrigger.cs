using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChildTrigger : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    private UnityEvent<Collider> _onTriggerEnter = null;
    private UnityEvent<Collision> _onCollisionEnter = null;
    private UnityEvent<Collision> _onCollisionStay = null;
    private UnityEvent<Collision> _onCollisionExit = null;
    private UnityEvent<float> _onJointBreak = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void OnTriggerEnter(Collider other) {
        _onTriggerEnter?.Invoke(other);
    }
    private void OnCollisionEnter(Collision other) {
        _onCollisionEnter?.Invoke(other);
    }
    private void OnCollisionStay(Collision other) {
        _onCollisionStay?.Invoke(other);
    }
    private void OnCollisionExit(Collision other) {
        _onCollisionExit?.Invoke(other);
    }
    

    void OnJointBreak(float breakForce)
    {
        _onJointBreak?.Invoke(breakForce);
    }
    // ---------- Public関数 ----------
    public void AddCallbackOnTriggerEnter(UnityAction<Collider> onTriggerEnter)
    { 
        if(_onTriggerEnter == null)
            _onTriggerEnter = new UnityEvent<Collider>();
        _onTriggerEnter.AddListener(onTriggerEnter); 
    }
    public void AddCallbackOnCollisionEnter(UnityAction<Collision> onCollisionEnter)
    { 
        if(_onCollisionEnter == null)
            _onCollisionEnter = new UnityEvent<Collision>();
        _onCollisionEnter.AddListener(onCollisionEnter); 
    }
    public void AddCallbackOnCollisionStay(UnityAction<Collision> onCollisionStay)
    { 
        if(_onCollisionStay == null)
            _onCollisionStay = new UnityEvent<Collision>();
        _onCollisionStay.AddListener(onCollisionStay); 
    }
    public void AddCallbackOnCollisionExit(UnityAction<Collision> onCollisionExit)
    { 
        if(_onCollisionExit == null)
            _onCollisionExit = new UnityEvent<Collision>();
        _onCollisionExit.AddListener(onCollisionExit); 
    }
    public void AddCallbackOnJointBreak(UnityAction<float> onJointBreak)
    {
        if(_onJointBreak == null)
            _onJointBreak = new UnityEvent<float>();
        _onJointBreak.AddListener(onJointBreak); 
    }
    // ---------- Private関数 ----------
}
