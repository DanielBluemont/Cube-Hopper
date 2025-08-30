using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Env;
using UnityEngine.UI;

namespace CubeHopper
{
    public class ExtraLifeRewarded : MonoBehaviour
    {
        #if UNITY_ANDROID
        private string _adUnit = Keys.EXTRA_LIFE_AD_UNIT;
        #else
        private string _adUnitId = "unused";
        #endif
        private RewardedAd rewardedAd;
        public static Action OnAdStarted;
        public static Action OnRewardGiven;

       
        public void LoadRewardedAd()
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            var adRequest = new AdRequest();

            RewardedAd.Load(_adUnit, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                rewardedAd = ad;
                RegisterEventHandlers(rewardedAd);
            });
        }
        public void ShowRewardedAd()
        {
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                OnAdStarted?.Invoke();
                rewardedAd.Show((Reward reward) => { OnRewardGiven?.Invoke(); });
            }
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                LoadRewardedAd();
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
                LoadRewardedAd();
               
            };
        }

    }
}
