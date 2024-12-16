using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
    [SerializeField, Tooltip("ゲーム開始時真っ暗画面")] private GameObject _blackOut = default;
    private bool _isInitialize = false;
    private UnityEvent _onFetchComplete = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start() {
        // await FetchDataAsync();
        _blackOut.SetActive(true);
        Initialize();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    // ゲーム初期化
    private async void Initialize() {
    // private void Initialize() {
        if(_isInitialize) return;
        _isInitialize = true;

        // リモートコンフィグのデータ取得(非同期)
        // FetchDataAsyncStart();
        await FetchDataAsync();
        // リアルタイム Remote Configによるデータ取得。デバイス上でないと動いてくれないらしい。
        FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener += ConfigUpdateListenerEventHandler;

        // ユーザーのAbフラグ設定(初回起動時のみ処理される)
        _userSegment.Initialize();
        // ゲームロード
        SaveDataManager.LoadData();
        // インゲーム初期化
        _inGameManager.Initialize();
        _inGameManager.SetTryShowInterstitialAdAction(TryShowInterstitialAd);
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
    // Firebaseリモートコンフィグのフェッチが完了した時のイベント設定
    public void AddOnFetchComplete( UnityAction onInitialize)
    {
        if(_onFetchComplete == null)
            _onFetchComplete = new UnityEvent();
        _onFetchComplete.AddListener(onInitialize);
    }
    // Firebaseリモートコンフィグのフェッチが完了した時のイベント実行
    private void OnFetchComplete()
    {
        PlayerPrefs.SetInt("isInitFetch", 1);
        _onFetchComplete?.Invoke();
        // _blackOut.SetActive(false);
    }

    public void ShowGame()
    {
        _blackOut.SetActive(false);
    }

    private async void FetchDataAsyncStart() { await FetchDataAsync(); }

    // リモートコンフィグのフェッチ
    public Task FetchDataAsync() {
        // フェッチリクエスト。引数はリクエスト時間間隔。開発時は即時反映させるため、０がいい。
#if UNITY_EDITOR
        Debug.Log("リモートコンフィグのデータ取得中");
        // 本番ではデフォルト値の12時間にするか、リアルタイムリモートコンフィグを使うべき
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
#else
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync();
#endif
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    // リモートコンフィグのフェッチが終わったか、正常に終了したかの判定。
    private void FetchComplete(Task fetchTask) {
        // フェッチが終わらなかった
        if (!fetchTask.IsCompleted) {
            // Debug.LogError("Retrieval hasn't finished.");
            return;
        }

        // フェッチ失敗
        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;
        if(info.LastFetchStatus != LastFetchStatus.Success) {
            // Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
            return;
        }

        // Fetch successful. Parameter values must be activated to use.
        // フェッチ成功。「remoteConfig.ActivateAsync」をすることでフェッチした値が使用可能になる
        remoteConfig.ActivateAsync().ContinueWithOnMainThread(
            task => {
#if UNITY_EDITOR
                Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");
#endif
                OnFetchComplete();
            }
        );
    }

    // リモートコンフィグ更新時に処理される関数（サンプル）。エディタ上ではOnConfigUpdateListenerが動いてくれないらしい。動作確認するには実機で確認する必要があるらしい
    private void ConfigUpdateListenerEventHandler(object sender, ConfigUpdateEventArgs args) {
        if (args.Error != RemoteConfigError.None) {
            // Debug.LogError($"[ConfigUpdateListenerEventHandler] Error occurred while listening: {args.Error}");
            return;
        }

        FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(
        task => {
            OnFetchComplete();
        });
	}
}
