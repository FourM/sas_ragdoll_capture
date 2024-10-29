using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class HandSkinManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("マテリアル")] private Material _handMaterial = default;
    [SerializeField, Tooltip("ポップアップ閉じる")] private List<Texture> _handMaterialTextures = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start(){
        UpdateHandMaterial();
    }
    // ---------- Public関数 ----------
    public void UpdateHandMaterial()
    {
        int index = PlayerPrefs.GetInt("is_SpiderSkin", 1);
        _handMaterial.SetTexture("_MainTex", _handMaterialTextures[index]);
    }   
    // ---------- Private関数 ----------
}
