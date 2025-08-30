using UnityEngine;
using UnityEngine.UI;

namespace CubeHopper
{
    public class ExtraLifeButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(FindObjectOfType<ExtraLifeRewarded>().ShowRewardedAd);
        }
    }
}
