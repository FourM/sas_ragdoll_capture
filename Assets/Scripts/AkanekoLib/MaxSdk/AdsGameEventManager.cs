using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 広告周りのゲーム側から呼ばれるイベントマネージャー
/// </summary>
public static class AdsGameEventManager
{
    private static UnityEvent<UnityAction, UnityAction> _onShowRewardAd = null;
    private static UnityEvent _onRewardLoaded = null;
    /// <summary>
    /// リワード広告再生イベント発火時のコールバック設定
    /// </summary>
    /// <param name="callback"></param>
    public static void AddOnShowRewardAd(UnityAction<UnityAction, UnityAction> callback)
    {
        if(_onShowRewardAd == null)
            _onShowRewardAd = new UnityEvent<UnityAction, UnityAction>();
        _onShowRewardAd.AddListener(callback);
    }
    /// <summary>
    /// リワード広告再生イベント発火
    /// </summary>
    /// <param name="onSuccess">成功時の処理</param>
    /// <param name="onfailure">失敗時の処理</param>
    public static void OnShowRewardAd(UnityAction onSuccess, UnityAction onFailure)
    {
        _onShowRewardAd?.Invoke(onSuccess, onFailure);
    }
    /// <summary>
    /// リワード広告を読み込めた時のコールバック追加
    /// </summary>
    /// <param name="callback"></param>
    public static void AddOnRewardLoaded(UnityAction callback)
    {
        if(_onRewardLoaded == null)
            _onRewardLoaded = new UnityEvent();
        _onRewardLoaded.AddListener(callback);
    }
    public static void OnRewardLoaded()
    {
        _onRewardLoaded?.Invoke();
    }
}
