using Env;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CubeHopper
{
    public class CoinRewarded : MonoBehaviour
    {
        #if UNITY_ANDROID
        private string _adUnit = Keys.COIN_AD_UNIT;
        #else
        private string _adUnitId = "unused";
        #endif

        [SerializeField] private CanvasGroup _prompt;
        private RewardedAd rewardedAd;

        public static Action<int> OnMoneyRewardGiven;
        
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
               
            });
        }

        
        public void ShowRewardedAd()
        {
            LoadRewardedAd();
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) => {
                    OnMoneyRewardGiven?.Invoke((int)reward.Amount);
                });
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

                _prompt.gameObject.SetActive(true);
                LeanTween.value(_prompt.gameObject, (x) => { _prompt.alpha = x; }, 0, 1, 0.6f).setEaseOutQuad();
                LeanTween.value(_prompt.gameObject, (x) => { _prompt.alpha = x; }, 1, 0, 0.6f).setEaseOutQuad().setDelay(1.2f).setOnComplete(() => {
                    _prompt.gameObject.SetActive(false);
                });
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
                LoadRewardedAd();
            };
        }


    }
}
