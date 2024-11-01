using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DebugStageSelectButton : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ボタン")] private Button _button = default;
    [SerializeField, Tooltip("テキスト")] private TextMeshProUGUI _stageNoText = default;
    [SerializeField, Tooltip("テキスト")] private TextMeshProUGUI _text = default;
    private int _stage = 0;
    private string _stageId = "";
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    public void SetOuButton(UnityAction onButton){ _button.onClick.AddListener(onButton); }
    public void SetStageNo(int stage)
    { 
        _stage = stage;
        _stageNoText.text = (_stage + 1).ToString("D3");
    }
    public void SetTextColor(Color32 color){ _text.color = color; }
    public void SetStageId(string stageId)
    { 
        _stageId = stageId;
        _text.text = _stageId;
    }
    public int GetStageNo(){ return _stage; }
    // ---------- Private関数 ----------
}
