using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.Events;

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
    private Vector3 _scale = default;
    private UnityEvent _onInitialize = null;
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

        _scale = this.transform.localScale;
        this.transform.localScale = Vector3.one;

        // int activeIndex = PlayerPrefs.GetInt("is_BananaMan",0);

        int activeIndex = 0;
        if(PlayerPrefs.GetInt("is_BananaMan") == 1)
        {
            if(PlayerPrefs.GetInt("Change_Background") == 0)
            {
                if(PlayerPrefs.GetInt("Change_Character") == 0)
                    activeIndex = 1;    // 今までのバナナマン
                else
                    activeIndex = 3;    // 調整モデル+今までの配色
            }
            else
            {
                if(PlayerPrefs.GetInt("Change_Character") == 0)
                    activeIndex = 2;    // 新しい配色今までのバナナマンモデル
                else
                    activeIndex = 4;    // 新配色＋調整モデル
            }
        }

        _activeHuman = Instantiate(_ListHuman[activeIndex]);
        _activeHuman.gameObject.SetActive(true);
        _activeHuman.transform.parent = this.transform;
        _activeHuman.transform.localPosition = Vector3.zero;
        _activeHuman.transform.localScale = _scale;
        _activeHuman.transform.localEulerAngles = Vector3.one;
        _activeHuman.SetChildLayer(_childLayer);
        if(_animeController != null)
            _activeHuman.SetAnimatorController(_animeController);
        else
            DOVirtual.DelayedCall(0.01f, ()=>{
                if(_activeHuman != null)
                    _activeHuman.DesableAnimation();
            });

        Transform ghost = _activeHuman.GetGhost();
        ghost.parent = this.transform;
        ghost.localScale = _scale;
    }
    public Human GetActiveHuman(){ return _activeHuman; }

    public void AddOnInitialize( UnityAction onInitialize)
    {
        if(_onInitialize == null)
            _onInitialize = new UnityEvent();
        _onInitialize.AddListener(onInitialize);
    }
    // ---------- Private関数 ----------
}
