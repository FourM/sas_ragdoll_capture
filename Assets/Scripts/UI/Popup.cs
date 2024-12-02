using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class Popup : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("インゲームUIマネージャー")] private InGameUIManager _inGameUIManager = default;
    [SerializeField, Tooltip("背景の閉じるボタン")] private Button _closeBackButton = default;
    protected List<Tween> _tweenList = default;
    private bool _isInitialize = false;
    private Tween _showHideTween = default;
    private bool _isShowPopup = false;
    private bool _isHidePopup = false;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake(){
        _inGameUIManager.AddOnInitialize(Initialize);
    }
    private void Start()
    {
        if(!_isInitialize)
            Initialize();
    }
    private void Initialize()
    {
        _showHideTween = null;
        _closeBackButton.onClick.AddListener(HidePopup);
        InitializeUnique();
        this.transform.localScale = Vector3.zero;
        _isInitialize = true;
    }

    // private void Update(){

    // }
    private void OnDisable() {
        if(_tweenList != null)
        {
            for(int i = 0; i < _tweenList.Count; i++)
            {
                _tweenList[i].Kill();
            }
        }
        KillShowHideTween();
        if(_isShowPopup)
            InGameManager.instance.SetIsShowUI(false);
    }
    // ---------- Public関数 -------------------------
    public void ShowPopup()
    {
        this.gameObject.SetActive(true);
        if(!_isInitialize)
            Initialize();

        ShowPopupUnique();
        KillShowHideTween();

        if(_isHidePopup && _isShowPopup)
            InGameManager.instance.SetIsShowUI(false);

        _showHideTween = this.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).OnComplete(()=>
        {
            ShowPopupCompleteUnique();
        });
        InGameManager.instance.SetIsShowUI(true);
        _isShowPopup = true;
        _isHidePopup = false;
    }
    public void HidePopup()
    {
        KillShowHideTween();
        HidePopupUnique();
        _isHidePopup = true;
        _showHideTween = this.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(()=>
        {
            InGameManager.instance.SetIsShowUI(false);
            _isShowPopup = false;
            HidePopupCompleteUnique();
        });
    }
    public bool IsShowPopup(){ return _isShowPopup; }
    // ---------- Private関数 ------------------------
    protected virtual void InitializeUnique(){  }
    protected virtual void UpdateUnique(){  }
    protected virtual void ShowPopupUnique(){  }
    protected virtual void HidePopupUnique(){  }
    protected virtual void ShowPopupCompleteUnique(){  }
    protected virtual void HidePopupCompleteUnique(){  }
    // 捕まった時の継承先の独自処理
    // protected virtual void OnCatchUnique(){  }
    // // 離された時の継承先の独自処理
    // protected virtual void OnReleaseUnique(){  }
    // protected virtual void OnBreakUnique(){  }
    // protected virtual void OnDisableUnique(){  }
    private void KillShowHideTween()
    {
        if(_showHideTween != null)
        {
            _showHideTween.Kill();
            _showHideTween = null;
        }
    }
}
