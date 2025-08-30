using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using CubeHopper.CameraModule;
using System.Collections.Generic;
using CubeHopper.UI;
using CubeHopper.Audio;
using CubeHopper.Platform;


namespace CubeHopper.Game
{
    public class Player : MonoBehaviour
    {
        private const int PLATFORM_LAYER = 6;
        private const int OBSTACLE_LAYER = 7;
        private const int DEATH_LAYER = 9;
        private const int COIN_LAYER = 11;
        private const int BORDER_LAYER = 13;

        private const float BOUNCE_REDUCTION = 0.75f;
        private Vector2 HALF_PLAYER_SIZE = new Vector2(0.5f, 0.5f);
        private int REFLECTION_LAYER = (1 << OBSTACLE_LAYER) | (1 << BORDER_LAYER);
        [SerializeField] private Transform point1, point2;
        [Header("Touch Drag settings")]
        [SerializeField][Range(1, 10)] private float _power = 5f;
        [SerializeField][Range(1, 10)] private float _maxDrag = 5f;
        [Space]
        [Header("Time rendering")]
        [SerializeField][Range(1, 5)] private float _duration = 1f;
        [SerializeField][Range(0, 1)] private float _timeStep = 0.1f;
        [Space]
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _flying;
        [SerializeField] private Sprite _idle;
        [Space]
        [SerializeField] private ParticleSystem _deathParticles;
        [SerializeField] private ParticleSystem _coinParticles;
        [Space]
        [SerializeField] private CameraFollow _cameraFollow;
        [Space]
        [Header("Sound")]
        [SerializeField] private AudioClip _coinSound;
        [SerializeField] private AudioClip _launchSound;
        [SerializeField] private AudioClip _losingSound;
        [SerializeField] private AudioClip _landingSound;
        [Space]
        [Header("Colors")]
        [SerializeField] private Gradient _coinPickUpColor;
        [SerializeField] private Gradient _deathColor;
        [Space]
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private GameObject _dot;
        [SerializeField] private List<GameObject> _dots = new List<GameObject>();


        private Rigidbody2D _rigidBody;
        private Camera _cam;

        private Vector2 _startPos;

        private int _steps;
        private bool isOnGround = true;
        [SerializeField] private bool isOnGround = true;

        public static Action<Vector2> OnLand;
        public static Action OnRelease;
        public static Action<int> OnScore;
        public static Action<int> OnCoinTrigger;

        private float _previous_height = 0;
        private float _diff = 0;
        private bool canDrag = true;
        private bool isResurrected = false;
        public static Action OnDeath;
        private void OnEnable()
        {
            CameraFollow.OnCameraStop += AdjustStarPos;
        }
        private void OnDisable()
        {
            CameraFollow.OnCameraStop -= AdjustStarPos;
        }
        private void AdjustStarPos(bool isStopped)
        {
            canDrag = isStopped;

        }
        private void Awake()
        {
            Time.timeScale = 1;
            _rigidBody = GetComponent<Rigidbody2D>();
            HALF_PLAYER_SIZE = GetComponent<BoxCollider2D>().size / 2;
            _steps = (int)Mathf.Round(_duration / _timeStep);


            _cam = Camera.main;

            for (int i = 0; i < _steps; i++)
            {
                _dots.Add(Instantiate(_dot));
                _dots[i].SetActive(false);
            }
        }
        [SerializeField] bool isDragging = false;
        public Vector2 velocity;
        private void Update()
        {
            velocity = _rigidBody.velocity;
            if (UItools.IsOnUI() || Settings.isPaused) return;

            if (!canDrag && Input.GetMouseButtonDown(0))
            {
                _rigidBody.velocity = Vector2.zero;
                _rigidBody.AddForce(new Vector2(0, -(_power) * 3), ForceMode2D.Impulse);
            }

            if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && !isDragging))
            {
                DragStart();
            }
            
            if (!canDrag || !isOnGround)
            {
                SetDotsActive(false);
                return;
            }
         
            if (Input.GetMouseButton(0))
                PlotTrajectory();
            else if (Input.GetMouseButtonUp(0))
                DragRelease();

        }


        private bool AngleCheck(Vector2 launchDirection)
        {
            float angle = -Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
            if (angle >= 0 && angle <= 180) return true;
            else return false;
        }




        private void OnTriggerEnter2D(Collider2D collision)
        {
            int layer = collision.gameObject.layer;


            if (layer == DEATH_LAYER)
            {
                Die();
            }
            if (layer == COIN_LAYER)
            {
                collision.gameObject.SetActive(false);
                AudioManager.Instance.PlayAudio(_coinSound);
                //Handheld.Vibrate();
                Instantiate(_coinParticles, transform.position, Quaternion.identity);
                OnCoinTrigger?.Invoke(UnityEngine.Random.Range(3, 10));
            }
            if (layer == PLATFORM_LAYER)
            {
                RaycastHit2D hit1 = Physics2D.Raycast(point1.position, -Vector2.up, 2f, 1 << PLATFORM_LAYER);
                RaycastHit2D hit2 = Physics2D.Raycast(point2.position, -Vector2.up, 2f, 1 << PLATFORM_LAYER);
                if (hit1 || hit2)
                {
                    AudioManager.Instance.PlayAudio(_landingSound);
                    //Handheld.Vibrate();
                    _rigidBody.velocity = Vector2.zero;
                    var p = collision.transform.parent.parent.GetComponent<SimplePlatform>();
                    p.DeactivateAtributes();
                    p.SetPlatformState(false);

                    isOnGround = true;
                    OnLand?.Invoke(transform.position);
                    _spriteRenderer.sprite = _idle;
                    _diff = transform.position.y - _previous_height;
                    _startPos.y += _diff;
                    OnScore?.Invoke(1);
                }

            }
        }


      /*  private void OnTriggerExit2D(Collider2D collision)
        {
            int layer = collision.gameObject.layer;
            if (layer == PLATFORM_LAYER)
            {
                isOnGround = false;
            }
        }*/

        public void Resurrect()
        {
            isResurrected = true;
            canDrag = true;
            isDragging = false;
            gameObject.SetActive(true);
        }
        private void Die()
        {
            AudioManager.Instance.PlayAudio(_losingSound);
            //Instantiate(_deathParticles, transform.position, Quaternion.identity);
            _spriteRenderer.sprite = _idle;
            isOnGround = true;
            gameObject.SetActive(false);
            if (isResurrected || FindAnyObjectByType<PlayerDataManager>().Score <= 3)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                OnDeath?.Invoke();
            }
        }
        private void DragStart()
        {
            _startPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        private void DragRelease()
        {
            _previous_height = transform.position.y;
            Vector2 releasePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = _startPos - releasePos;
            if (IsDragCancelled(releasePos) || AngleCheck(direction))
            {
                SetDotsActive(false);
                return;
            }
            Vector2 forceDirection = Vector3.ClampMagnitude(direction, _maxDrag) * _power;
            _spriteRenderer.sprite = _flying;
            _rigidBody.velocity = Vector2.zero;
            _rigidBody.AddForce(forceDirection, ForceMode2D.Impulse);
            AudioManager.Instance.PlayAudio(_launchSound);
            OnRelease?.Invoke();
            isOnGround = false;
            canDrag = false;
            isDragging = false;
            SetDotsActive(false);
        }
        private void PlotTrajectory()
        {
            if (!isOnGround) return;
            Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            if (IsDragCancelled(mousePos) || !isOnGround)
            {
                SetDotsActive(false);
                return;
            }

            Vector2 direction = _startPos - mousePos;
            Vector2 launchPos = transform.position - (Vector3.up * HALF_PLAYER_SIZE.y);
            Vector2 force = Vector2.ClampMagnitude(direction, _maxDrag) * _power;

            float time = 0f;
            int cancelIndex = _steps - 1;

            for (int i = 0; i < _steps; i++)
            {
                time += _timeStep;
                float scale = Mathf.Lerp(0.25f, 0.1f, (float)i / _steps);
                Vector2 displacement = CalculateDisplacement(force, time);
                Vector2 newPos = launchPos + displacement;

                RaycastHit2D hit = Physics2D.BoxCast(newPos, HALF_PLAYER_SIZE, 0, displacement.normalized, 0.5f, REFLECTION_LAYER);
                if (hit)
                {
                    if (hit.transform.gameObject.layer == BORDER_LAYER)
                    {
                        Vector2 closestPoint = hit.collider.ClosestPoint(newPos);

                        launchPos = (closestPoint - Vector2.up) + (hit.normal * scale / 2);

                        Vector2 reducedForce = force - displacement;
                        Vector2 normalComponent = Vector2.Dot(reducedForce, hit.normal) * hit.normal;
                        Vector2 tangentComponent = reducedForce - normalComponent;
                        force = tangentComponent - normalComponent * BOUNCE_REDUCTION;

                        time = _timeStep;
                        displacement = CalculateDisplacement(force, time);
                        newPos = launchPos + displacement;
                    }
                    else
                    {
                        cancelIndex = i;
                        break;
                    }
                }


                _dots[i].transform.position = newPos;
                _dots[i].transform.localScale = new Vector2(scale, scale);
            }

            float closeness = Vector2.Distance(mousePos, _startPos) / _maxDrag;

            for (int i = 0; i < cancelIndex; i++)
            {
                _dots[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, closeness);
                _dots[i].SetActive(true);
            }
            for (int i = cancelIndex; i < _steps; i++)
            {
                _dots[i].SetActive(false);
            }
        }

        private void SetDotsActive(bool isActive)
        {
            foreach (GameObject dot in _dots)
            {
                dot.SetActive(isActive);
            }
        }
        private Vector2 CalculateDisplacement(Vector2 forceDirection, float time)
        {
            return forceDirection * time + Vector2.up * (Physics2D.gravity.y * _rigidBody.gravityScale * Mathf.Pow(time, 2)) / 2;
        }

        private bool IsDragCancelled(Vector2 mousePos)
        {
            return Vector2.Distance(mousePos, _startPos) < _maxDrag / 3;
        }

        public void SetSkin(Sprite idleSprite, Sprite flySprite, Gradient traceColor)
        {
            _idle = idleSprite;
            _flying = flySprite;
            _spriteRenderer.sprite = _idle;
            _trailRenderer.colorGradient = traceColor;
        }
    }
}
