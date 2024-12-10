using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using AdjustSdk;

// 広告マネージャー。
public class AdsManager : MonoBehaviour
{
    string adUnitId;
    int retryAttempt;
    public int buffer;
    public MaxSdkBase.AdInfo _adInfo = null;
    private UnityEvent _onLoaded = default;
    [SerializeField] private StageBanner _stageBanner = default;

    // Start is called before the first frame update
    void Start()
    { 
        adUnitId   = "5c90ca19dea88f82";

        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
           
        InitializeInterstitialAds();
        _stageBanner.InitializeBannerAds();


        };
        MaxSdk.SetSdkKey("EpIDwy0bhJT7B76E65tdJt8Wkp20-IrR2Oc9sbxuS-6BseH7R3bQzSfFTN1u0Jvxh88rOvyh2rPH0WX81eO7Km");
        //MaxSdk.SetTestDeviceAdvertisingIdentifiers(new string[] { "87FBF16D-0FCB-4CF4-AB0C-C1625A66F250" });
        MaxSdk.SetUserId("USER_ID");
        MaxSdk.InitializeSdk();
        
    }

    // Update is called once per frame
    // void FixedUpdate()
    // {
    // //  　動作テスト用
    // //    buffer++;
    // //    if(buffer == 1000)
    // //         ShowAd();
    // }
       public void InitializeInterstitialAds()
    {
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

        // Load the first interstitial
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(adUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'

        // Reset retry attempt
        retryAttempt = 0;
        _adInfo = adInfo;
        _onLoaded?.Invoke();
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

        retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

        Invoke("LoadInterstitial", (float)retryDelay);
        LoadInterstitial();

    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
       //LoadInterstitial();

    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        LoadInterstitial();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad revenue paid. Use this callback to track user revenue.

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

        TrackAdRevenue(adInfo);
    }
    private void TrackAdRevenue(MaxSdkBase.AdInfo adInfo)
    {
        AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue("applovin_max_sdk");

        adjustAdRevenue.SetRevenue(adInfo.Revenue, "USD");
        adjustAdRevenue.AdRevenueNetwork = adInfo.NetworkName;
        adjustAdRevenue.AdRevenueUnit = adInfo.AdUnitIdentifier;
        adjustAdRevenue.AdRevenuePlacement = adInfo.Placement;

        Adjust.TrackAdRevenue(adjustAdRevenue);

        // AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);

        // adjustAdRevenue.setRevenue(adInfo.Revenue, "USD");
        // adjustAdRevenue.setAdRevenueNetwork(adInfo.NetworkName);
        // adjustAdRevenue.setAdRevenueUnit(adInfo.AdUnitIdentifier);
        // adjustAdRevenue.setAdRevenuePlacement(adInfo.Placement);

        // AdjustSdk.Adjust.trackAdRevenue(adjustAdRevenue);
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad.
        LoadInterstitial();
        // ステージスタートイベントの発火を試行
        GameDataManager.TryEventStageStart();
    }
    public void ShowAd()
    {
        if (MaxSdk.IsInterstitialReady(adUnitId))
        {
            // try
            // {
            //     double revenue = 0;
            //     if(_adInfo != null)
            //     {
            //         revenue = _adInfo.Revenue * 1000;
            //         FirebaseManager.instance.EventWatchInste(true, revenue);
            //     }
            //     else
            //         FirebaseManager.instance.EventWatchInste(true, -1);
            // }
            // catch
            // {
            //     FirebaseManager.instance.EventWatchInste(true, -1);
            // }
            FirebaseManager.instance.EventWatchInste(true);
            MaxSdk.ShowInterstitial(adUnitId);
        }
        else
        {
            // FirebaseManager.instance.EventWatchInste(false, 0);
            FirebaseManager.instance.EventWatchInste(false);
            // ステージスタートイベントの発火を試行
            GameDataManager.TryEventStageStart();
        }
    }

    
    
    public void AddOnLoadedCallback(UnityAction onLoaded)
    {
        if(_onLoaded == null)
            _onLoaded = new UnityEvent();
        _onLoaded.AddListener(onLoaded);
    }
    public double GetAdRevenue()
    {
        try
        {
            if(_adInfo != null)
                return _adInfo.Revenue * 1000;
            return -1;
        }
        catch
        {
            return -1;
        }
    }
}