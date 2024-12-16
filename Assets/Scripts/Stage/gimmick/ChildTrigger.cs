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
    [SerializeField, Tooltip("RigidBody")] private Rigidbody _rigidBody;
    private UnityEvent<Collider> _onTriggerEnter = null;
    private UnityEvent<Collider> _onTriggerStay = null;
    private UnityEvent<Collision> _onCollisionEnter = null;
    private UnityEvent<Collision> _onCollisionStay = null;
    private UnityEvent<Collision> _onCollisionExit = null;
    private UnityEvent<float> _onJointBreak = null;
    private UnityEvent _onBecameVisible = null;
    private UnityEvent _onBecameInvisible = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void OnTriggerEnter(Collider other) {
        _onTriggerEnter?.Invoke(other);
    }
    private void OnTriggerStay(Collider other) {
        _onTriggerStay?.Invoke(other);
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
    private void OnBecameVisible()
    { 
        _onBecameVisible?.Invoke();
    }
    private void OnBecameInvisible()
    { 
        _onBecameInvisible?.Invoke();
    }
    

    void OnJointBreak(float breakForce)
    {
        _onJointBreak?.Invoke(breakForce);
    }
    // ---------- Public関数 ----------
    public Rigidbody GetRigidBody()
    { 
        if(_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody>();
        return _rigidBody;
    }
    public void AddCallbackOnTriggerEnter(UnityAction<Collider> onTriggerEnter)
    { 
        if(_onTriggerEnter == null)
            _onTriggerEnter = new UnityEvent<Collider>();
        _onTriggerEnter.AddListener(onTriggerEnter); 
    }
    public void AddCallbackOnTriggerStay(UnityAction<Collider> onTriggerStay)
    { 
        if(_onTriggerStay == null)
            _onTriggerStay = new UnityEvent<Collider>();
        _onTriggerStay.AddListener(onTriggerStay); 
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
    public void AddOnBecameVisible(UnityAction onBecameVisible)
    {
        if(_onBecameVisible == null)
            _onBecameVisible = new UnityEvent();
        _onBecameVisible.AddListener(onBecameVisible); 
        // Debug.Log("見えたぁメメタァ");
    }
    public void AddOnBecameInVisible(UnityAction onBecameInVisible)
    {
        if(_onBecameInvisible == null)
            _onBecameInvisible = new UnityEvent();
        _onBecameInvisible.AddListener(onBecameInVisible); 
        // Debug.Log("みえんごなった");
    }
    // ---------- Private関数 ----------
}
