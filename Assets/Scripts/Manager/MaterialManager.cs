using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MaterialType
{
    mainColor,
    subColor1,
    subColor2,
    subColor3,
    subColor4,
    subColor5,
    subColor6,
    subColor7,
    subColor8,
    subColor9,
    subColor10,
}

public class MaterialManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("インゲームマネージャー")] private InGameManager _inGameManager = default;
    [SerializeField, Tooltip("マテリアルリスト")] private List<SerializeList<Material>> _materialList;
    [SerializeField, Tooltip("ランダムマテリアルリスト")] private List<SerializeList<Material>> _materialList2;
    [SerializeField, Tooltip("ランダムマテリアルリスト")] private bool _isNewColor = true;
    [SerializeField, Tooltip("カメラ")] private Camera _camera = default;
    [SerializeField, Tooltip("カメラ")] private Light _mainLight = default;
    private bool _isInitialize = false;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    public static MaterialManager instance = null;
    // ---------- Unity組込関数 -----------------------
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
        _inGameManager.AddOnInitializeMaterialManager(Initialize);
    }
    public void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;
    }
    // ---------- Public関数 -------------------------
    public Material ResetMaterial(Material material, MaterialType materialType)
    {
        Material retMaterial = material;

        // 従来のマテリアルを返す
        if(PlayerPrefs.GetInt("Change_Background") == 0)
            retMaterial = _materialList[0].list[(int)materialType];

        return retMaterial;
    }
    public void SetBackGroundColor(Color32 color)
    {
        if(PlayerPrefs.GetInt("Change_Background") == 1)
            _camera.backgroundColor = color;
        else
            _camera.backgroundColor = _materialList[0].list[0].color;    // 従来のマテリアルを返す
    }
    public void SetEnableMainLight(bool isEnable) {
        if(PlayerPrefs.GetInt("Change_Background") == 1)
            _mainLight.gameObject.SetActive(isEnable);
        else
            _mainLight.gameObject.SetActive(true);
    }
    // ---------- Private関数 ------------------------
}
