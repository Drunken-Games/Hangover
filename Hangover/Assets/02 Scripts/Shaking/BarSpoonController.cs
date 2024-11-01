using UnityEngine;
using DG.Tweening;

public class BarSpoonController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float maxXPosition = 2f;  
    [SerializeField] private float minXPosition = -2f; 
    
    [Header("터치 설정")]
    [SerializeField] private float touchSensitivity = 1f;
    
    [Header("흔들기 설정")]
    [SerializeField] private float shakeThreshold = 0.2f;
    [SerializeField] private float shakePower = 1f;
    [SerializeField] private Ease shakeEase = Ease.OutQuad;
    
    [Header("효과 설정")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stirSound;
    [SerializeField] private float soundVolume = 0.7f;
    [SerializeField] private bool useVibration = true;
    [SerializeField] private float effectCooldown = 0.1f;

    [Header("씬 전환 설정")]
    [SerializeField] private float requiredStirTime = 1.5f;     // 필요한 저어야 하는 시간
    [SerializeField] private float touchHoldTime = 2f;          // 터치 홀드 시간
    [SerializeField] private string nextSceneName = "GameScene";
    [SerializeField] private bool showDebugInfo = true;         // 디버그 정보 표시

    private Vector3 originalPosition;
    private Vector3 lastAcceleration;
    private float lastEffectTime;
    private Camera mainCamera;
    private Vector2 lastTouchPosition;
    private Tween currentShakeTween;
    private bool isDragging = false;
    private float currentStirTime = 0f;     // 현재 저은 시간
    private float currentTouchTime = 0f;    // 현재 터치 홀드 시간
    private bool isStirring = false;        // 저어지고 있는지 여부
    private SceneController sceneController;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다!");
            return;
        }

        SetupAudioSource();
        InitializeSceneController();
        
        originalPosition = transform.position;
        lastAcceleration = Input.acceleration;
        lastEffectTime = Time.time;
    }

    private void InitializeSceneController()
    {
        sceneController = FindObjectOfType<SceneController>();
        if (sceneController == null)
        {
            Debug.LogWarning("SceneController를 찾을 수 없습니다! 새로 추가합니다...");
            GameObject controllerObject = new GameObject("SceneController");
            sceneController = controllerObject.AddComponent<SceneController>();
        }
    }

    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.clip = stirSound;
        audioSource.loop = false;
        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        HandleTouchInput();
        if (!isDragging)
        {
            CheckShakeInput();
        }

        // 디버그 정보 표시
        if (showDebugInfo)
        {
            if (isStirring)
            {
                Debug.Log($"저어지는 진행도: {currentStirTime:F2}/{requiredStirTime:F2}");
            }
            if (isDragging)
            {
                Debug.Log($"터치 홀드 진행도: {currentTouchTime:F2}/{touchHoldTime:F2}");
            }
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, -mainCamera.transform.position.z));

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    float touchDistance = Mathf.Abs(touchWorldPos.x - transform.position.x);
                    if (touchDistance < 1f)
                    {
                        isDragging = true;
                        currentTouchTime = 0f;
                        lastTouchPosition = touchWorldPos;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        // 터치 홀드 시간 체크
                        currentTouchTime += Time.deltaTime;
                        if (currentTouchTime >= touchHoldTime)
                        {
                            LoadNextScene();
                            return;
                        }

                        float deltaX = touchWorldPos.x - lastTouchPosition.x;
                        if (Mathf.Abs(deltaX) > 0.1f) // 최소 이동 거리 체크
                        {
                            MoveSpoon(deltaX * touchSensitivity);
                            lastTouchPosition = touchWorldPos;
                            PlayStirEffect();
                            UpdateStirProgress();
                        }
                    }
                    break;

                case TouchPhase.Stationary:
                    if (isDragging)
                    {
                        currentTouchTime += Time.deltaTime;
                        if (currentTouchTime >= touchHoldTime)
                        {
                            LoadNextScene();
                            return;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    currentTouchTime = 0f;
                    break;
            }
        }
        else
        {
            ResetStirProgress();
        }
    }

    private void CheckShakeInput()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 accelerationDelta = acceleration - lastAcceleration;
        float shakeMagnitude = accelerationDelta.magnitude;

        if (shakeMagnitude > shakeThreshold)
        {
            float shakeDirection = Mathf.Sign(accelerationDelta.x);
            ApplyShake(shakeDirection);
            
            if (useVibration && Time.time - lastEffectTime > effectCooldown)
            {
                TriggerVibration();
                PlayStirEffect();
                lastEffectTime = Time.time;
            }

            UpdateStirProgress();
        }
        else
        {
            ResetStirProgress();
        }

        lastAcceleration = acceleration;
    }

    private void UpdateStirProgress()
    {
        isStirring = true;
        currentStirTime += Time.deltaTime;
        
        if (currentStirTime >= requiredStirTime)
        {
            LoadNextScene();
        }
    }

    private void ResetStirProgress()
    {
        if (isStirring)
        {
            currentStirTime = Mathf.Max(0, currentStirTime - (Time.deltaTime * 0.5f));
            if (currentStirTime == 0)
            {
                isStirring = false;
            }
        }
    }

    private void LoadNextScene()
    {
        if (sceneController != null)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                sceneController.LoadSceneByName(nextSceneName);
            }
            else
            {
                sceneController.LoadNextScene();
            }
        }
    }

    private void MoveSpoon(float deltaX)
    {
        Vector3 newPosition = transform.position;
        newPosition.x += deltaX;
        newPosition.x = Mathf.Clamp(newPosition.x, minXPosition, maxXPosition);
        transform.position = newPosition;
    }

    private void ApplyShake(float direction)
    {
        currentShakeTween?.Kill();

        float shakeDuration = 0.3f;
        Vector3 targetPos = transform.position + new Vector3(direction * shakePower, 0, 0);
        targetPos.x = Mathf.Clamp(targetPos.x, minXPosition, maxXPosition);

        currentShakeTween = transform.DOMove(targetPos, shakeDuration)
            .SetEase(shakeEase);
    }

    private void PlayStirEffect()
    {
        if (audioSource != null && stirSound != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void TriggerVibration()
    {
        #if UNITY_IOS
        // iOS 햅틱 구현
        #elif UNITY_ANDROID
        Handheld.Vibrate();
        #endif
    }

    private void OnDestroy()
    {
        currentShakeTween?.Kill();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 1f, 1f));
    }
}