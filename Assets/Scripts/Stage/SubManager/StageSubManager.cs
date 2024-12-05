using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class StageSubManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージ")] protected GameStage _stage;
    private bool _isInitialize = false;
    protected List<Tween> _tweenList = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);

        AwakeUnique();
    }
    private void Update()
    {
        UpdateUnique();
    }
    private void FixedUpdate()
    {
        FixedUpdateUnique();
    }
    private void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;
        
        _tweenList = new List<Tween>();
        InitializeUnique();
    }
    private void OnDisable() {
        KillAllTween();
        OnDisableUnique();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    protected virtual void AwakeUnique(){}
    protected virtual void InitializeUnique(){}
    protected virtual void UpdateUnique(){}
    protected virtual void FixedUpdateUnique(){}
    protected virtual void OnDisableUnique(){}
    protected void KillAllTween()
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
