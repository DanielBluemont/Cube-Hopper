using UnityEngine;
using UnityEngine.Events;
using GoogleMobileAds.Api;

public class AdInitializer : MonoBehaviour
{

    public static AdInitializer Instance { get; private set; }

    [SerializeField] private UnityEvent OnAdsInitialized;
    public bool IsInitialized { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(initStatus =>
        {
            IsInitialized = true;
            print("LOADED");
            OnAdsInitialized?.Invoke();
        });
    }
}
