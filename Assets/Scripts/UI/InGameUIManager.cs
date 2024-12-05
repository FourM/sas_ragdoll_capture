using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class InGameUIManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("キャンバス")] private Canvas _canvas = default;
    [SerializeField, Tooltip("ステージマネージャー")] private Button _buttonUndo = default;
    [SerializeField, Tooltip("照準")] private UIReticle _uiReticle = default;
    private bool _isInitialize = false;
    private UnityEvent _onInitialize = null;
    private UnityEvent _onHideUI = null;
    private UnityEvent _onShowUI = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    public void Initialize(){
        if(_isInitialize) return;
            _isInitialize = true;

        _uiReticle.Initialize();
        UpdateReticleActive();

        _onInitialize?.Invoke();
    }

    public void SetOnClickButtonUndo( UnityAction onClick )
    {
        _buttonUndo.onClick.AddListener(onClick);
    }

    public void ShowInGameUI(){
        if(GameDataManager.DebugIsShowUi())
        {   
            ShowButtonUndo();
        }   
        _onShowUI?.Invoke();
    }
    public void HideInGameUI(){
        HideButtonUndo();
        _onHideUI?.Invoke();
    }

    public void SetReticlePos( Vector2 mousePos )
    {
        _uiReticle.SetPos(mousePos);
    }
    public void OnClick( Vector2 mousePos )
    {
        _uiReticle.OnClick(mousePos);
    }
    public void OnMouseUp()
    {
        _uiReticle.OnMouseUp();
    }
    public void SetIsCatch(bool isCatch)
    {
        _uiReticle.SetIsCatch(isCatch);
    }
    public void UpdateReticleActive()
    {
        int aimOn = PlayerPrefs.GetInt("Aim_ON");
        if(aimOn == 0)
            _uiReticle.gameObject.SetActive(false);
        else if(aimOn == 1)
            _uiReticle.gameObject.SetActive(true);
        else
            Debug.LogError("Aim_ON が0と1以外です！:" + aimOn);
    }
    // 初期化時イベント設定
    public void AddOnInitialize( UnityAction onInitialize)
    {
        if(_onInitialize == null)
            _onInitialize = new UnityEvent();
        _onInitialize.AddListener(onInitialize);
    }
    
    public void AddOnShowUI( UnityAction onShow)
    {
        if(_onShowUI == null)
            _onShowUI = new UnityEvent();
        _onShowUI.AddListener(onShow);
    }
    public void AddOnHideUI( UnityAction onHide)
    {
        if(_onHideUI == null)
            _onHideUI = new UnityEvent();
        _onHideUI.AddListener(onHide);
    }
    // ---------- Private関数 ----------
    private void ShowButtonUndo()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_buttonUndo.transform.DOScale(Vector3.one, 0.07f).SetEase(Ease.OutBack));
        sequence.AppendCallback(()=>
        {
            _buttonUndo.enabled = true;
        });
    }
    private void HideButtonUndo()
    {
        _buttonUndo.enabled = false;
        // _buttonUndo.transform.localScale = Vector3.zero;
        _buttonUndo.transform.DOScale(Vector3.zero, 0.07f).SetEase(Ease.InBack);
    }
}
