using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SkinIcon : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("ボタン")] private CustomButton _button = default;
    [SerializeField, Tooltip("スキン名")] private TextMeshProUGUI _skinName = default;
    [SerializeField, Tooltip("画像")] private Image _image = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    public void SetUp(HandSkinData skinData, UnityAction onClick )
    {
        _skinName.text = skinData.name;
        _image.sprite = skinData.image;
        _button.onClick.AddListener(onClick);
    }
    // ---------- Private関数 ------------------------
}
