using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PopupSkin : Popup
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    [SerializeField, Tooltip("スキンアイコンプレハブ")] private SkinIcon _skinIconPrefab = default;
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("背景の閉じるボタン")] private Button _closeButton = default;
    [SerializeField, Tooltip("スキンマネージャー")] private HandSkinManager _skinManager = default;
    [SerializeField, Tooltip("ボタンコンテナ")] private RectTransform _container = default;
    // [SerializeField, Tooltip("背景の閉じるボタン")] private Button _closeButton = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    protected override void InitializeUnique()
    {
        _closeButton.onClick.AddListener(HidePopup);

        List<HandSkinData> listHandSkinData = _skinManager.GetListHandSkinData();

        foreach(Transform child in _container.transform)
        {
            Destroy(child.gameObject);
        }
        for(int i = 0; i < listHandSkinData.Count; i++)
        {
            // Debug.Log("!!!:" + i);
            SkinIcon skinIcon = Instantiate(_skinIconPrefab);
            HandSkinData handSkinData = listHandSkinData[i];
            int index = i;
            skinIcon.SetUp(handSkinData, ()=>
            {
                _skinManager.UpdateHandSkin(index);
            });
            skinIcon.transform.parent = _container;
        }
    }
}
