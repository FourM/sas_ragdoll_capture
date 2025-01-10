using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGoEndlessMode : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("インゲームUIマネージャー")] private InGameUIManager _inGameUIManager = default;
    [SerializeField, Tooltip("ボタン")] private CustomButton _button = default;
    [SerializeField, Tooltip("アイコン")] private GameObject _goEndlessIcon = default;
    [SerializeField, Tooltip("アイコン")] private GameObject _goMainIcon = default;
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
        ChangeIcon(GameDataManager.GameMode);
        _button.onClick.AddListener(()=>
        {
            if( GameDataManager.GameMode != GameMode.endlessBattle)
            {
                GameDataManager.InGameMainEvent.ChangeGameMode(GameMode.endlessBattle);
                ChangeIcon(GameMode.endlessBattle);
            }
            else
            {
                GameDataManager.InGameMainEvent.ChangeGameMode(GameMode.main);
                ChangeIcon(GameMode.main);
            }
        });
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void ChangeIcon(GameMode gameMode)
    {
        if(gameMode == GameMode.endlessBattle)
        {
            _goMainIcon.SetActive(true);
            _goEndlessIcon.SetActive(false);
        }
        if(gameMode == GameMode.main)
        {
            _goMainIcon.SetActive(false);
            _goEndlessIcon.SetActive(true);
        }
    }
}
