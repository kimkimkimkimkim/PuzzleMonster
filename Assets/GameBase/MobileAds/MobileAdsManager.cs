using UnityEngine;
using GoogleMobileAds.Api;
using System;
using PM.Enum.UI;
using UniRx;

namespace GameBase
{
    public class MobileAdsManager : SingletonMonoBehaviour<MobileAdsManager>
    {
        [SerializeField] protected string BOTTOM_BANNER_AD_UNIT_ID_IOS;
        [SerializeField] protected string BOTTOM_BANNER_AD_UNIT_ID_ANDROID;
        [SerializeField] protected string CENTER_BANNER_AD_UNIT_ID_IOS;
        [SerializeField] protected string CENTER_BANNER_AD_UNIT_ID_ANDROID;
        [SerializeField] protected string INTERSTITIAL_AD_UNIT_ID_IOS;
        [SerializeField] protected string INTERSTITIAL_AD_UNIT_ID_ANDROID;
        [SerializeField] protected string REWARD_AD_UNIT_ID_IOS;
        [SerializeField] protected string REWARD_AD_UNIT_ID_ANDROID;

        private const string BANNER_TEST_AD_UNIT_ID_IOS = "ca-app-pub-3940256099942544/2934735716";
        private const string BANNER_TEST_AD_UNIT_ID_ANDROID = "ca-app-pub-3940256099942544/6300978111";
        private const string INTERSTITIAL_TEST_AD_UNIT_ID_IOS = "ca-app-pub-3940256099942544/4411468910";
        private const string INTERSTITIAL_TEST_AD_UNIT_ID_ANDROID = "ca-app-pub-3940256099942544/1033173712";
        private const string REWARD_TEST_AD_UNIT_ID_IOS = "ca-app-pub-3940256099942544/1712485313";
        private const string REWARD_TEST_AD_UNIT_ID_ANDROID = "ca-app-pub-3940256099942544/5224354917";

        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardedAd rewardedAd;

        private Action rewardedCallBackAction; // 報酬受け取り時に実行する処理

        private void Start()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(initStatus => { });

            // 広告取得
            //RequestBanner();
            //RequestInterstitial();
            RequestRewarded();
        }

        #region Banner

        public void RequestBanner()
        {
            var adUnitId = "";
            if (Application.platform == RuntimePlatform.Android)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? BANNER_TEST_AD_UNIT_ID_ANDROID : BOTTOM_BANNER_AD_UNIT_ID_ANDROID;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? BANNER_TEST_AD_UNIT_ID_IOS : BOTTOM_BANNER_AD_UNIT_ID_IOS;
            }
            else
            {
                adUnitId = "unexpected_platform";
            }

            // Create a 320x50 banner at the top of the screen.
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

            // Raised when an ad is loaded into the banner view.
            bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : "
                    + error);
            };
            // Raised when the ad is estimated to have earned money.
            bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };

            // Create an empty ad request.
            AdRequest request = new AdRequest();

            // Load the banner with the request.
            bannerView.LoadAd(request);
        }

        public void DestroyBanner()
        {
            bannerView.Destroy();
        }

        public void RequestCenterBanner()
        {
            var adUnitId = "";
            if (Application.platform == RuntimePlatform.Android)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? BANNER_TEST_AD_UNIT_ID_ANDROID : CENTER_BANNER_AD_UNIT_ID_ANDROID;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? BANNER_TEST_AD_UNIT_ID_IOS : CENTER_BANNER_AD_UNIT_ID_IOS;
            }
            else
            {
                adUnitId = "unexpected_platform";
            }

            // Create a 320x50 banner at the top of the screen.
            bannerView = new BannerView(adUnitId, AdSize.MediumRectangle, 0, 300);

            // Raised when an ad is loaded into the banner view.
            bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : "
                    + error);
            };
            // Raised when the ad is estimated to have earned money.
            bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };

            // Create an empty ad request.
            AdRequest request = new AdRequest();

            // Load the banner with the request.
            bannerView.LoadAd(request);
        }

        public void DestroyCenterBanner()
        {
            bannerView.Destroy();
        }

        #endregion Banner

        #region Interstitial

        public bool TryShowInterstitial()
        {
            if (interstitial != null && interstitial.CanShowAd())
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
            LoadInterstitial();

            // Raised when the ad is estimated to have earned money.
            interstitial.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            interstitial.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            interstitial.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            interstitial.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            interstitial.OnAdFullScreenContentClosed += () =>
            {
                DestroyInterstitial();
                RequestInterstitial();
                Debug.Log("Interstitial ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            interstitial.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        private void LoadInterstitial()
        {
            var adUnitId = "";
            if (Application.platform == RuntimePlatform.Android)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? INTERSTITIAL_TEST_AD_UNIT_ID_ANDROID : INTERSTITIAL_AD_UNIT_ID_ANDROID;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? INTERSTITIAL_TEST_AD_UNIT_ID_IOS : INTERSTITIAL_AD_UNIT_ID_IOS;
            }
            else
            {
                adUnitId = "unexpected_platform";
            }

            // Clean up the old ad before loading a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
                interstitial = null;
            }

            Debug.Log("Loading the interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");

            // send the request to load the ad.
            InterstitialAd.Load(
                adUnitId,
                adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());

                    interstitial = ad;
                }
            );
        }

        public void DestroyInterstitial()
        {
            interstitial.Destroy();
        }

        #endregion Interstitial

        #region Rewarded

        public bool IsRewardAdLoaded()
        {
            return rewardedAd != null && rewardedAd.CanShowAd();
        }

        public IObservable<Unit> ShowRewardObservable()
        {
            const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                return Observable.Create<Unit>(observer =>
                {
                    rewardedAd.Show((Reward reward) =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                        Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                    });
                    return Disposable.Empty;
                });
            }
            else
            {
                return CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.YesOnly,
                    title = "エラー",
                    content = "広告の読み込みに失敗しました\n時間を開けて再度お試しください",
                })
                    .SelectMany(_ =>
                    {
                        return Observable.Create<Unit>(observer =>
                        {
                            observer.OnCompleted();
                            return Disposable.Empty;
                        });
                    })
                    .AsUnitObservable();
            }
        }

        private void RequestRewarded()
        {
            var adUnitId = "";
            if (Application.platform == RuntimePlatform.Android)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? REWARD_TEST_AD_UNIT_ID_ANDROID : REWARD_AD_UNIT_ID_ANDROID;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                adUnitId = ApplicationSettingsManager.Instance.isTestAdMode ? REWARD_TEST_AD_UNIT_ID_IOS : REWARD_AD_UNIT_ID_IOS;
            }
            else
            {
                adUnitId = "unexpected_platform";
            }

            // Clean up the old ad before loading a new one.
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            Debug.Log("Loading the rewarded ad.");
            // create our request used to load the ad.
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");

            // send the request to load the ad.
            RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                rewardedAd = ad;

                // Raised when the ad is estimated to have earned money.
                rewardedAd.OnAdPaid += (AdValue adValue) =>
                {
                    Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                        adValue.Value,
                        adValue.CurrencyCode));
                };
                // Raised when an impression is recorded for an ad.
                rewardedAd.OnAdImpressionRecorded += () =>
                {
                    Debug.Log("Rewarded ad recorded an impression.");
                };
                // Raised when a click is recorded for an ad.
                rewardedAd.OnAdClicked += () =>
                {
                    Debug.Log("Rewarded ad was clicked.");
                };
                // Raised when an ad opened full screen content.
                rewardedAd.OnAdFullScreenContentOpened += () =>
                {
                    Debug.Log("Rewarded ad full screen content opened.");
                };
                // Raised when the ad closed full screen content.
                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    RequestRewarded();
                    Debug.Log("Rewarded ad full screen content closed.");
                };
                // Raised when the ad failed to open full screen content.
                rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    RequestRewarded();
                    Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);
                    CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "エラー",
                        content = "広告の読み込みに失敗しました\n時間を開けて再度お試しください",
                    }).Subscribe();
                };
            });
        }

        #endregion Rewarded
    }
}