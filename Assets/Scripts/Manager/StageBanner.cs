using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Firebase.Analytics;
using AdjustSdk;


public class StageBanner : MonoBehaviour
{
    public string bannerAdUnitId;
    public int count;
    public int check;
    public int sbkcheck;
    public MaxSdkBase.AdInfo _adInfo = null;
    private UnityEvent _onLoaded = default;
    public void Start()
    {
#if UNITY_ANDROID
         bannerAdUnitId  = "231fa7a1b73515a9";
         check=1;
#elif UNITY_IOS
         bannerAdUnitId = "91a0381dd83e7a2b";
         check=1;
#else
        bannerAdUnitId  = "unexpected_platform";
#endif

        //  InitializeBannerAds();
         MaxSdk.ShowBanner(bannerAdUnitId);
        check= PlayerPrefs.GetInt("banner");
    }
    public void FixedUpdate()
    {
       





    }
    public void InitializeBannerAds()
    {

#if UNITY_ANDROID
         bannerAdUnitId  = "231fa7a1b73515a9";
#elif UNITY_IOS
        bannerAdUnitId = "9616ebb8764e92aa";
#else
        bannerAdUnitId  = "unexpected_platform";
#endif

        // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
        // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
        MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

    // Set background or background color for banners to be fully functional
    MaxSdk.SetBannerBackgroundColor(bannerAdUnitId,new Color32(0,0,0,0));

    MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
    MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
    MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
    MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
    MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
    MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
        check = 1;
}

private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
    _adInfo = adInfo;  
    showads();
    _onLoaded?.Invoke();
}

private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {

    }

private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

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


    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }



    public void showads()
    {
        // FirebaseManager.instance.EventWatchBanner(true, revenue);
        // MaxSdk.ShowBanner(bannerAdUnitId);
        // FirebaseManager.instance.EventWatchBanner(true, GetAdRevenue());
        FirebaseManager.instance.EventWatchBanner(true);
        MaxSdk.ShowBanner(bannerAdUnitId);
    }

    public void DebugHideAds()
    {
        MaxSdk.HideBanner(bannerAdUnitId);
    }

  
    public void checkads()
    {
        InitializeBannerAds();
        showads();
        check = 0;
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