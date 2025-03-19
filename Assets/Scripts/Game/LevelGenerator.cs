using System.Collections.Generic;
using UnityEngine;
using CubeHopper.CameraModule;
using CubeHopper.Platform;
using UnityEngine.Pool;
using UnityEditor;

namespace CubeHopper.Game
{
    public class LevelGenerator : MonoBehaviour
    {
        private const float PLATFORM_HALFSIZE = 0.8f;
        [SerializeField] private List<SimplePlatform> _platforms;
        [SerializeField] private SimplePlatform _previousPlatform, _currentPlatform, _nextPlatform;
        [Space]
        [Header("Player Reference")]
        [SerializeField] private Player _player;
        private List<int> _prefixSum = new List<int>();
        private int _sumOfWeights = 0;
        private int _length = 0;
        private int _currentIndex = 0;
        private int _previousIndex = 0;
        private Dictionary<SimplePlatform, ObjectPool<SimplePlatform>> _pools;

        private float placableX;
        private void OnEnable()
        {
            Player.OnLand += CreateNextPlatform;
            Player.OnRelease += DestroyPlatform;
            PlayerDataManager.OnDifficultyChange += UpdateWeights;
            ExtraLifeRewarded.OnRewardGiven += GiveSecondAttempt;
        }

        private void OnDisable()
        {
            Player.OnLand -= CreateNextPlatform;
            Player.OnRelease -= DestroyPlatform;
            PlayerDataManager.OnDifficultyChange -= UpdateWeights;
            ExtraLifeRewarded.OnRewardGiven -= GiveSecondAttempt;
        }

        private void Awake()
        {
            _player = FindObjectOfType<Player>();
            _length = _platforms.Count;
            placableX = CameraScaler.X_SIZE / 2 - PLATFORM_HALFSIZE;
            for (int i = 0; i < _length; i++)
            {
                _sumOfWeights += _platforms[i].Weight;
                _prefixSum.Add(0);
                for (int j = 0; j <= i; j++)
                {
                    _prefixSum[i] += _platforms[j].Weight;
                }
            }
            _pools = new Dictionary<SimplePlatform, ObjectPool<SimplePlatform>>();
            foreach (var platform in _platforms)
            {
                _pools[platform] = new ObjectPool<SimplePlatform>(
                    () => Instantiate(platform),
                    p => ActionOnGet(p),
                    p => ActionOnRelease(p),
                    p => Destroy(p.gameObject),
                    false, 3, 3
                );
            }
        }
      
        private void ActionOnGet(SimplePlatform p)
        {
            p.gameObject.SetActive(true);
            p.SetPlatformState(true);
            p.SetCollider(true);
        }
        private void ActionOnRelease(SimplePlatform p)
        {
            p.gameObject.SetActive(false);
            p.SetPlatformState(false);
        }
        private void UpdateWeights()
        {
            for (int i = 0; i < _length; i++)
            {
                int change = _platforms[i].Rate;
                _sumOfWeights += change;
                for (int j = i; j < _length; j++)
                {
                    _prefixSum[j] += change;
                }
            }
        }
        private void CreateNextPlatform(Vector2 playerPos)
        {
            _previousIndex = _currentIndex;
            _currentIndex = GetNextIndex();
            var p = _platforms[_currentIndex];
            _nextPlatform = _pools[p].Get();
            _nextPlatform.transform.position = GetSpawnPosition(playerPos, p);
        }
        private void DestroyPlatform()
        {
            if (_previousPlatform != null)
            {
                _pools[_platforms[_previousIndex]].Release(_previousPlatform);
            }
            _previousPlatform = _currentPlatform;

            if (_currentPlatform != null)
            {
                _currentPlatform.SetPlatformState(false);
                _currentPlatform.gameObject.SetActive(false);
            }

            _currentPlatform = _nextPlatform;

            _nextPlatform = null;

        }

        public Vector2 GetSpawnPosition(Vector2 playerPosition, SimplePlatform p)
        {
            float randomY = Random.Range(p.YOffset, p.YOffset + p.YMaxOffset);
            float randomX;

            if (playerPosition.x - PLATFORM_HALFSIZE < -placableX)
                randomX = Random.Range(playerPosition.x + PLATFORM_HALFSIZE * 2, placableX);
            else if (playerPosition.x + PLATFORM_HALFSIZE > placableX)
                randomX = Random.Range(-placableX, playerPosition.x - PLATFORM_HALFSIZE * 2);
            else
                randomX = Random.Range(-placableX, placableX);

            return new Vector2(randomX, playerPosition.y + randomY);
        }

        private void GiveSecondAttempt()
        {
         
            _nextPlatform = _currentPlatform;
            _currentPlatform = _previousPlatform;
            _previousPlatform = null;
            _currentPlatform.gameObject.SetActive(true);
            _player.transform.position = new Vector3
            (
                _currentPlatform.transform.position.x,
                _currentPlatform.transform.position.y + 1,
                _currentPlatform.transform.position.z
            );
            
            _player.Resurrect();
        }
        private int GetNextIndex()
        {
            int randomPick = Random.Range(0, _sumOfWeights);
            for (int i = 0; i < _prefixSum.Count; i++)
            {
                if (randomPick < _prefixSum[i]) return i;
            }
            return 0;
        }
    }
}
