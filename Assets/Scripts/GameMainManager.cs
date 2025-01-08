using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体のマネージャー
/// 　ステージ管理、
/// </summary>
public class GameMainManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("インゲームマネージャー")] private InGameManager _inGameManager = default;
    [SerializeField, Tooltip("広告マネージャー")] private AdsManager _adManager = default;
    [SerializeField, Tooltip("ABテストフラグ設定クラス")] private UserSegment _userSegment = default;
    private bool _isInitialize = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    public static GameMainManager instance = null;
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start() {
        Initialize();
    }
    // ---------- Public関数 ----------
    public void SceneReload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameReset();
    }
    // ---------- Private関数 ----------
    // ゲーム初期化
    private void Initialize() {
        if(_isInitialize) return;
        _isInitialize = true;

        // ユーザーのAbフラグ設定(初回起動時のみ処理される)
        _userSegment.Initialize();
        // ゲームロード
        SaveDataManager.LoadData();

        GameReset();
    }
    // インステ広告表示試行
    private void TryShowInterstitialAd()
    {
        if(30f <= TimeManager.instance.elapsedTime)
        {
            _adManager.ShowAd();
            TimeManager.instance.elapsedTime = 0;
        }
        else
        {
            // ステージスタートイベントの発火を試行
            GameDataManager.TryEventStageStart();
        }
    }
    private void ShowAd()
    {
        _adManager.ShowAd();
    }
    public void GameReset()
    {
        // インゲーム初期化
        _inGameManager.Initialize();
        _inGameManager.SetTryShowInterstitialAdAction(TryShowInterstitialAd);
    }
}
