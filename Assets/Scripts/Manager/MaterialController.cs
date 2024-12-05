using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("hoge")] private Renderer _renderer = default;
    [SerializeField, Tooltip("hoge")] private MaterialType _materialType = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Start(){
        _renderer.material = MaterialManager.instance.ResetMaterial(_renderer.material, _materialType);
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
}
