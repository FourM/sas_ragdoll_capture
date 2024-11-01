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
    private bool _isInitialize = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    public void Initialize(){
        if(_isInitialize) return;
            _isInitialize = true;
    }

    public void SetOnClickButtonUndo( UnityAction onClick )
    {
        _buttonUndo.onClick.AddListener(onClick);
    }

    public void ShowInGameUI(){
        ShowButtonUndo();
    }
    public void HideInGameUI(){
        HideButtonUndo();
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
