using UnityEngine;
using UnityEngine.UI;

namespace CubeHopper
{
    public class CoinRewardedButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(FindObjectOfType<CoinRewarded>().ShowRewardedAd);
        }
    }
}
