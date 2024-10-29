using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class HumanHub : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    [SerializeField, Tooltip("Humanリスト")] private List<Human> _ListHuman;
    [SerializeField, Tooltip("レイヤー")] private int _childLayer = 20;
    [SerializeField, Tooltip("アニメーションコントローラー")] private RuntimeAnimatorController _animeController = null;
    // ---------- プロパティ ----------
    private Human _activeHuman = null;
    private bool _isInitialize = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    public void Initialize(int layer = 0)
    { 
        if(_isInitialize )
            return;
        _isInitialize = true;

        _childLayer = 20 + layer;

        int activeIndex = PlayerPrefs.GetInt("is_BananaMan",0);
        _activeHuman = Instantiate(_ListHuman[activeIndex]);
        _activeHuman.gameObject.SetActive(true);
        _activeHuman.transform.parent = this.transform;
        _activeHuman.transform.localPosition = Vector3.zero;
        _activeHuman.transform.localScale = Vector3.one;
        _activeHuman.transform.localEulerAngles = Vector3.one;
        _activeHuman.SetChildLayer(_childLayer);
        if(_animeController != null)
            _activeHuman.SetAnimatorController(_animeController);
        else
            DOVirtual.DelayedCall(0.01f, ()=>{
                if(_activeHuman != null)
                    _activeHuman.DesableAnimation();
            });
    }
    public Human GetActiveHuman(){ return _activeHuman; }
    // ---------- Private関数 ----------
}
