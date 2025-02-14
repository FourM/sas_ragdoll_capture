using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using UnityEngine.Events;
using AppLovinMax.Internal;

/// <summary>
/// ゲーム全体のマネージャー
/// 　ステージ管理、
/// </summary>
/// 
// [DefaultExecutionOrder(-1)]
public class GameMainManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("インゲームマネージャー")] private InGameManager _inGameManager = default;
    // [SerializeField, Tooltip("広告マネージャー")] private InterstitialAdManager _adManager = default;
    // [SerializeField, Tooltip("リワード広告マネージャー")] private RewardedAdManager _rewardAdManager = default;
    [SerializeField, Tooltip("ABテストフラグ設定クラス")] private UserSegment _userSegment = default;
    private bool _isInitialize = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    public static GameMainManager instance { get; private set; }
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        GlobalExceptionHandler.Init();
    }
    private void Start() {
        Initialize();
    }
    // ---------- Public関数 ----------
    public void SceneReload()
    {
        MaxEventExecutor.InitializeIfNeeded();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameReset();
    }
    // ---------- Private関数 ----------
    // ゲーム初期化
    private void Initialize() {
        if(_isInitialize) return;
        _isInitialize = true;

        // FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        //     if (task.Result == DependencyStatus.Available)
        //     {
        //         FirebaseApp app = FirebaseApp.DefaultInstance;
        //         Debug.Log("Firebase Initialized Successfully!");
        //     }
        //     else
        //     {
        //         Debug.LogError("Firebase initialization failed: " + task.Result);
        //     }
        // });

        // ユーザーのAbフラグ設定(初回起動時のみ処理される)
        _userSegment.Initialize();
        // ゲームロード
        SaveDataManager.LoadData();
        // 広告イベント設定
        AdsGameEventManager.AddOnShowRewardAd(OnShowRewardAd);

        RewardedAdManager.instance.AddOnLoadedCallback(AdsGameEventManager.OnRewardLoaded);

        EndlessBattleTimeScaleManager.Initialize();

        GameReset();
    }
    // インステ広告表示試行
    private void TryShowInterstitialAd()
    {
        if(30f <= TimeManager.instance.elapsedTime)
        {
            // _adManager.ShowAd();
            InterstitialAdManager.instance.ShowAd();
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
        // 広告表示関連のコード
        // _adManager.ShowAd();
        InterstitialAdManager.instance.ShowAd();
    }
    public void GameReset()
    {
        // インゲーム初期化
        _inGameManager.Initialize();
        _inGameManager.SetTryShowInterstitialAdAction(TryShowInterstitialAd);

        // _rewardAdManager
    }

    /// <summary>
    /// リワード広告表示
    /// </summary>
    /// <param name="onSuccess">成功時の処理</param>
    /// <param name="onfailure">失敗時の処理</param>
    private void OnShowRewardAd(UnityAction onSuccess, UnityAction onfailure)
    {
        RewardedAdManager.instance.ShowReward(onSuccess, onfailure);
    }

}
