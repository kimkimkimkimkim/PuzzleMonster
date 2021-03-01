using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using GoogleMobileAds.Api;
using System;

namespace GameBase {
    public class MobileAdsManager : SingletonMonoBehaviour<MobileAdsManager>
    {
        /*
        [SerializeField] protected bool _isTestAdUnitId = true;
        [SerializeField] protected string BOTTOM_BANNER_AD_UNIT_ID_IOS;
        [SerializeField] protected string BOTTOM_BANNER_AD_UNIT_ID_ANDROID;
        [SerializeField] protected string CENTER_BANNER_AD_UNIT_ID_IOS;
        [SerializeField] protected string CENTER_BANNER_AD_UNIT_ID_ANDROID;
        [SerializeField] protected string INTERSTITIAL_AD_UNIT_ID_IOS;
        [SerializeField] protected string INTERSTITIAL_AD_UNIT_ID_ANDROID;
        [SerializeField] protected string REWARD_AD_UNIT_ID_IOS;
        [SerializeField] protected string REWARD_AD_UNIT_ID_ANDROID;

        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardedAd rewardedAd;

        private Action rewardedCallBackAction; // 報酬受け取り時に実行する処理

        void Start()
        {
            #if UNITY_ANDROID
                string appId = "ca-app-pub-7228877379141040~7917671822";
            #elif UNITY_IPHONE
                string appId = "ca-app-pub-7228877379141040~5855627022";
            #else
                string appId = "unexpected_platform";
            #endif

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(initStatus => { });

            // 広告取得
            RequestBanner();
            RequestInterstitial();
            RequestRewarded();
        }

        #region Banner
        public void RequestBanner()
        {
            #if UNITY_ANDROID
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/6300978111" : BOTTOM_BANNER_AD_UNIT_ID_ANDROID;
            #elif UNITY_IPHONE
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/2934735716" : BOTTOM_BANNER_AD_UNIT_ID_IOS;
            #else
                string adUnitId = "unexpected_platform";
            #endif

            // Create a 320x50 banner at the top of the screen.
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

            // Called when an ad request has successfully loaded.
            bannerView.OnAdLoaded += HandleOnBannerAdLoaded;
            // Called when an ad request failed to load.
            bannerView.OnAdFailedToLoad += HandleOnBannerAdFailedToLoad;
            // Called when an ad is clicked.
            bannerView.OnAdOpening += HandleOnBannerAdOpened;
            // Called when the user returned from the app after an ad click.
            bannerView.OnAdClosed += HandleOnBannerAdClosed;
            // Called when the ad click caused the user to leave the application.
            bannerView.OnAdLeavingApplication += HandleOnBannerAdLeavingApplication;

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();

            // Load the banner with the request.
            bannerView.LoadAd(request);
        }

        public void HandleOnBannerAdLoaded(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLoaded event received");
        }

        public void HandleOnBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                                + args.Message);
        }

        public void HandleOnBannerAdOpened(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdOpened event received");
        }

        public void HandleOnBannerAdClosed(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdClosed event received");
        }

        public void HandleOnBannerAdLeavingApplication(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLeavingApplication event received");
        }

        public void DestroyBanner()
        {
            bannerView.Destroy();
        }

        public void RequestCenterBanner()
        {
            #if UNITY_ANDROID
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/6300978111" : CENTER_BANNER_AD_UNIT_ID_ANDROID;
            #elif UNITY_IPHONE
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/2934735716" : CENTER_BANNER_AD_UNIT_ID_IOS;
            #else
                string adUnitId = "unexpected_platform";
            #endif

            // Create a 320x50 banner at the top of the screen.
            bannerView = new BannerView(adUnitId, AdSize.MediumRectangle, 0, 300);

            // Called when an ad request has successfully loaded.
            bannerView.OnAdLoaded += HandleOnCenterBannerAdLoaded;
            // Called when an ad request failed to load.
            bannerView.OnAdFailedToLoad += HandleOnCenterBannerAdFailedToLoad;
            // Called when an ad is clicked.
            bannerView.OnAdOpening += HandleOnCenterBannerAdOpened;
            // Called when the user returned from the app after an ad click.
            bannerView.OnAdClosed += HandleOnCenterBannerAdClosed;
            // Called when the ad click caused the user to leave the application.
            bannerView.OnAdLeavingApplication += HandleOnCenterBannerAdLeavingApplication;

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();

            // Load the banner with the request.
            bannerView.LoadAd(request);
        }

        public void HandleOnCenterBannerAdLoaded(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLoaded event received");
        }

        public void HandleOnCenterBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                                + args.Message);
        }

        public void HandleOnCenterBannerAdOpened(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdOpened event received");
        }

        public void HandleOnCenterBannerAdClosed(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdClosed event received");
        }

        public void HandleOnCenterBannerAdLeavingApplication(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLeavingApplication event received");
        }

        public void DestroyCenterBanner()
        {
            bannerView.Destroy();
        }
        #endregion

        #region Interstitial
        public bool TryShowInterstitial()
        {
            if (interstitial.IsLoaded())
            {
                interstitial.Show();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RequestInterstitial()
        {
            #if UNITY_ANDROID
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/1033173712" : INTERSTITIAL_AD_UNIT_ID_ANDROID;
            #elif UNITY_IPHONE
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/4411468910" : INTERSTITIAL_AD_UNIT_ID_IOS;
            #else
                string adUnitId = "unexpected_platform";
            #endif

            // Initialize an InterstitialAd.
            interstitial = new InterstitialAd(adUnitId);

            // Called when an ad request has successfully loaded.
            this.interstitial.OnAdLoaded += HandleOnInterstitialAdLoaded;
            // Called when an ad request failed to load.
            this.interstitial.OnAdFailedToLoad += HandleOnInterstitialAdFailedToLoad;
            // Called when an ad is shown.
            this.interstitial.OnAdOpening += HandleOnInterstitialAdOpened;
            // Called when the ad is closed.
            this.interstitial.OnAdClosed += HandleOnInterstitialAdClosed;
            // Called when the ad click caused the user to leave the application.
            this.interstitial.OnAdLeavingApplication += HandleOnInterstitialAdLeavingApplication;

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();

            // Load the interstitial with the request.
            interstitial.LoadAd(request);
        }

        public void HandleOnInterstitialAdLoaded(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLoaded event received");
        }

        public void HandleOnInterstitialAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                                + args.Message);
        }

        public void HandleOnInterstitialAdOpened(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdOpened event received");
        }

        public void HandleOnInterstitialAdClosed(object sender, EventArgs args)
        {
            DestroyInterstitial();
            RequestInterstitial();
            MonoBehaviour.print("HandleAdClosed event received");
        }

        public void HandleOnInterstitialAdLeavingApplication(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLeavingApplication event received");
        }

        public void DestroyInterstitial()
        {
            interstitial.Destroy();
        }
        #endregion

        #region Rewarded
        public bool IsRewardAdLoaded()
        {
            return this.rewardedAd.IsLoaded();
        }

        public bool TryShowRewarded(Action action)
        {
            if (this.rewardedAd.IsLoaded())
            {
                rewardedCallBackAction = action;
                this.rewardedAd.Show();
                return true;
            }
            else
            { 
                return false;
            }
        }

        private void RequestRewarded()
        {
            #if UNITY_ANDROID
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/5224354917" : REWARD_AD_UNIT_ID_ANDROID;
            #elif UNITY_IPHONE
                string adUnitId = _isTestAdUnitId ? "ca-app-pub-3940256099942544/1712485313" : REWARD_AD_UNIT_ID_IOS;
            #else
                string adUnitId = "unexpected_platform";
            #endif

            this.rewardedAd = new RewardedAd(adUnitId);

            // Called when an ad request has successfully loaded.
            this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
            // Called when an ad request failed to load.
            this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
            // Called when an ad is shown.
            this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
            // Called when an ad request failed to show.
            this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
            // Called when the user should be rewarded for interacting with the ad.
            this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
            // Called when the ad is closed.
            this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();

            // Load the rewarded ad with the request.
            this.rewardedAd.LoadAd(request);
        }

        public void HandleRewardedAdLoaded(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleRewardedAdLoaded event received");
        }

        public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
        {
            MonoBehaviour.print(
                "HandleRewardedAdFailedToLoad event received with message: "
                                 + args.Message);
        }

        public void HandleRewardedAdOpening(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleRewardedAdOpening event received");
        }

        public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
        {
            MonoBehaviour.print(
                "HandleRewardedAdFailedToShow event received with message: "
                                 + args.Message);
        }

        public void HandleRewardedAdClosed(object sender, EventArgs args)
        {
            // 次の広告を読み込んでおく
            RequestRewarded();
            MonoBehaviour.print("HandleRewardedAdClosed event received");
        }

        public void HandleUserEarnedReward(object sender, Reward args)
        {
            string type = args.Type;
            double amount = args.Amount;
            MonoBehaviour.print(
                "HandleRewardedAdRewarded event received for "
                            + amount.ToString() + " " + type);

            //報酬受け取り時の処理
            if(rewardedCallBackAction != null) rewardedCallBackAction();
        }

        #endregion
        */      
    }
}