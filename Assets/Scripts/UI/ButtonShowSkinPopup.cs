using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class ButtonShowSkinPopup : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("インゲームUIマネージャー")] private InGameUIManager _inGameUIManager = default;
    [SerializeField, Tooltip("ボタン")] private CustomButton _button = default;
    [SerializeField, Tooltip("ポップアップ")] private PopupSkin _popupSkin = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake(){
        _inGameUIManager.AddOnInitialize(Initialize);
        _inGameUIManager.AddOnShowUI(()=>{ this.gameObject.SetActive(true); });
        _inGameUIManager.AddOnHideUI(()=>{ this.gameObject.SetActive(false); });
    }
    private void Initialize()
    {
        _button.onClick.AddListener(()=>
        {
            if(_popupSkin.IsShowPopup())
                _popupSkin.HidePopup();
            else
                _popupSkin.ShowPopup();
        });
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
}
