using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[DefaultExecutionOrder(-5)]
public class HandSkinManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("インゲームマネージャー")] private GameMainManager _gameMainManager = default;
    [SerializeField, Tooltip("インゲームマネージャー")] private InGameManager _inGameManager = default;
    [SerializeField, Tooltip("おてて")] private Hand _hand;
    [SerializeField, Tooltip("インゲームマネージャー")] private ObiPathSmoother _webSmoother = default;
    [SerializeField, Tooltip("おててのデータリスト")] private List<HandSkinData> _listHandSkinData;
    [SerializeField, Tooltip("おてて")] private int _debugHandId = -1;
    [SerializeField, Tooltip("おてて")] private Transform _webLineStartPos;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        _inGameManager.AddOnInitialize(Initialize);
    }
    private void Initialize(){
        UpdateHandSkin();
        // firebaseリモートコンフィグのフェッチが終わったら再度スキン更新
        _gameMainManager.AddOnFetchComplete(()=>
        {
            UpdateHandSkin();
            _gameMainManager.ShowGame();
        });
    }
    // ---------- Public関数 ----------
    public void UpdateHandSkin(int handSkinId = -1)
    {
        if(handSkinId < 0)
        {
            // セーブデータを参考にする。
            handSkinId = PlayerPrefs.GetInt("HandSkin", -1);

            // セーブデータが「スキン未設定」なら、デフォルトスキンを設定する
            if(handSkinId < 0)
            {
                // 初リモートコンフィグ取得が終わってないならビジネスマンをデフォルトにする
                if(PlayerPrefs.GetInt("isInitFetch", 0) == 0)
                {
                    handSkinId = 1; // ビジネスマンをデフォルトにする
                }
                else
                {
                    bool isBusinessManDefaut = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("IsBussinessManSkin").BooleanValue;
                    // Debug.Log("ビジネスマンスキン？:" + isBusinessManDefaut);
                    if(!isBusinessManDefaut)
                        handSkinId = 0;
                    else
                        handSkinId = 1; // ビジネスマンをデフォルトにする
                }
            }
        }
        else
        {
            PlayerPrefs.SetInt("HandSkin", handSkinId);
        }
        // if(0 < _debugHandId )
        //     handSkinId = _debugHandId;
        HandSkinData handSkinData = _listHandSkinData[handSkinId];

        _webLineStartPos.parent = this.transform;

        HandSkin leftHand = _hand.SetLeftHandSkin(handSkinData.leftHandPrefab);
        _hand.SetRightHandSkin(handSkinData.rightHandPrefab);

        _inGameManager.UpdateWebRopeMaterial(handSkinData.webMaterial);
            
        _webLineStartPos.parent = leftHand.GetWebStartPos();

        _webSmoother.twist = handSkinData.webTwist;
    }   
    public List<HandSkinData> GetListHandSkinData(){ return _listHandSkinData; }
    // ---------- Private関数 ----------
}
