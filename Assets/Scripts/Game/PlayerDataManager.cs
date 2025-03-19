using CubeHopper.Platform;
using CubeHopper.SavingData;
using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

namespace CubeHopper.Game
{
    public class PlayerDataManager : MonoBehaviour
    {
        private const int MAX_DIFFICULTY = 12;
        private const string FILE_PATH = "/data.json";

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private TextMeshProUGUI _addMoneyText;
        [SerializeField] private TextMeshProUGUI _addScoreText;
        public static Action OnDifficultyChange;
        private int _threshold = 10;
        private int _level = 0;
        private int maxScore = 0;

        private IDataService _dataService = new JsonDataService();   
        private PlayerStats stats;
        private bool isEncrypted = true;
        public int Money => stats.Money;
        public int Score => stats.Score;

        private Vector3 _addMoneyStartPos, _addScoreStartPos;

        public void GiveReward(int amount)
        {
            _addMoneyText.gameObject.SetActive(true);
            _addMoneyText.text = "+" + amount.ToString();
            stats.Money += amount;
            _addMoneyText.transform.LeanMoveLocalY(_moneyText.transform.localPosition.y-1, 1f).setIgnoreTimeScale(true).setEaseOutQuart().setOnComplete(() => {
                _addMoneyText.gameObject.SetActive(false);
                _addMoneyText.transform.localPosition = _addMoneyStartPos;
                _moneyText.text = stats.Money.ToString();
            });
            if (!_dataService.SaveData(FILE_PATH, stats, isEncrypted))
            {
                Debug.LogError("could not save money");
            };
        }

        public void ExecutePurchase(int cost)
        {
            stats.Money -= cost;
            _moneyText.text = stats.Money.ToString();
            if (!_dataService.SaveData(FILE_PATH, stats, isEncrypted))
            {
                Debug.LogError("could not save money");
            }
        }

        private void OnEnable()
        {
            Player.OnScore += AddScore;
            Player.OnCoinTrigger += GiveReward;
            DailyReward.OnRewardClaimed += GiveReward;
            CoinRewarded.OnMoneyRewardGiven += GiveReward;
        }
        private void OnDisable()
        {
            Player.OnScore -= AddScore;
            Player.OnCoinTrigger -= GiveReward;
            DailyReward.OnRewardClaimed -= GiveReward;
            CoinRewarded.OnMoneyRewardGiven -= GiveReward;
        }
        private void Awake()
        {
            _addMoneyStartPos = _addMoneyText.transform.localPosition;
            _addScoreStartPos = _addScoreText.transform.localPosition;
            LoadPlayerData();
            maxScore = stats.Score;
            _moneyText.text = stats.Money.ToString();
            _scoreText.text = stats.Score.ToString();
        }
       
        public void ResetScore()
        {
            _scoreText.text = "0";
            stats.Score = 0;
        }
 

        private void AddScore(int value)
        {
            _addScoreText.gameObject.SetActive(true);
            _addScoreText.text = "+" + value.ToString();
            stats.Score+= value;
            _addScoreText.transform.LeanMoveLocalY(_scoreText.transform.localPosition.y, 1f).setIgnoreTimeScale(true).setEaseOutQuart().setOnComplete(() => {
                _addScoreText.gameObject.SetActive(false);
                _addScoreText.transform.localPosition = _addScoreStartPos;
                _scoreText.text = stats.Score.ToString();
            });
            if (stats.Score > maxScore)
            {
                maxScore = stats.Score;
            }
            SavePlayerData();
            if (stats.Score >= _threshold && _level != MAX_DIFFICULTY)
            {
                IncreaseDifficulty();
            }

        }
        //sometimes i'm a little bit slow :(
        private void IncreaseDifficulty()
        {
            _threshold += _threshold;
            _level++;
            OnDifficultyChange?.Invoke();
        }
        private void SavePlayerData()
        {
            PlayerStats dataToSave = stats.Score < maxScore ? new PlayerStats(maxScore, stats.Money): stats;
            
            if (!_dataService.SaveData(FILE_PATH, dataToSave, isEncrypted))
            {
                Debug.LogError("EEEee");
            }
        }

        private void LoadPlayerData()
        {
            if (_dataService.CheckPathExistence(FILE_PATH))
            {
                stats = _dataService.LoadData<PlayerStats>(FILE_PATH, isEncrypted);
            }
            else 
            {
                stats = new PlayerStats(0, 0);
                SavePlayerData();
            }
        }

    }
}

struct PlayerStats
{
    public int Score;
    public int Money;
    public PlayerStats(int score, int money)
    {
        Score = score;
        Money = money;
    }
}
