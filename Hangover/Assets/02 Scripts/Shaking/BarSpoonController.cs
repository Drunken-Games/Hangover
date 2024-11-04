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
    [SerializeField] private float shakeThreshold = 0.15f;
    [SerializeField] private float shakePower = 1f;
    [SerializeField] private Ease shakeEase = Ease.OutQuad;
    [SerializeField] private float shakeDetectionInterval = 0.05f;
    
    [Header("효과 설정")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stirSound;
    [SerializeField] private float soundVolume = 0.7f;
    [SerializeField] private bool useVibration = true;
    [SerializeField] private float effectCooldown = 0.1f;

    [Header("씬 전환 설정")]
    [SerializeField] private float requiredStirTime = 1.5f;     
    [SerializeField] private float touchHoldTime = 2f;          
    [SerializeField] private string nextSceneName = "GameScene";
    [SerializeField] private bool showDebugInfo = true;         

    private Vector3 originalPosition;
    private Vector3 lastAcceleration;
    private float lastEffectTime;
    private float lastShakeCheckTime;
    private Camera mainCamera;
    private Vector2 lastTouchPosition;
    private Tween currentShakeTween;
    private bool isDragging = false;
    private float currentStirTime = 0f;     
    private float currentTouchTime = 0f;    
    private bool isStirring = false;        
    private SceneController sceneController;
    
    private Vector3[] accelerationBuffer = new Vector3[10];
    private int bufferIndex = 0;
    private int validMovementCount = 0;
    private const int REQUIRED_MOVEMENT_COUNT = 3;
    private const float MOVEMENT_RESET_TIME = 0.5f;
    private Vector3 prevFilteredAcceleration;

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
        lastShakeCheckTime = Time.time;
        
        for (int i = 0; i < accelerationBuffer.Length; i++)
        {
            accelerationBuffer[i] = Input.acceleration;
        }
        prevFilteredAcceleration = Input.acceleration;
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
            CheckStirMovement();
        }

        if (showDebugInfo)
        {
            if (isStirring)
            {
                Debug.Log($"저어지는 진행도: {currentStirTime:F2}/{requiredStirTime:F2}, 움직임 카운트: {validMovementCount}/{REQUIRED_MOVEMENT_COUNT}");
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
                        currentTouchTime += Time.deltaTime;
                        if (currentTouchTime >= touchHoldTime)
                        {
                            LoadNextScene();
                            return;
                        }

                        float deltaX = touchWorldPos.x - lastTouchPosition.x;
                        if (Mathf.Abs(deltaX) > 0.1f)
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

    private void CheckStirMovement()
    {
        if (Time.time - lastShakeCheckTime < shakeDetectionInterval) return;

        accelerationBuffer[bufferIndex] = Input.acceleration;
        bufferIndex = (bufferIndex + 1) % accelerationBuffer.Length;

        Vector3 filteredAcceleration = Vector3.zero;
        foreach (Vector3 acc in accelerationBuffer)
        {
            filteredAcceleration += acc;
        }
        filteredAcceleration /= accelerationBuffer.Length;

        Vector2 horizontalMovement = new Vector2(
            filteredAcceleration.x - prevFilteredAcceleration.x,
            filteredAcceleration.z - prevFilteredAcceleration.z
        );

        float movementMagnitude = horizontalMovement.magnitude;

        if (showDebugInfo)
        {
            // Debug.Log($"Movement: {movementMagnitude:F3}, X: {horizontalMovement.x:F3}, Z: {horizontalMovement.z:F3}");
        }

        if (movementMagnitude > shakeThreshold)
        {
            if (Time.time - lastEffectTime > MOVEMENT_RESET_TIME)
            {
                validMovementCount = 0;
            }

            validMovementCount++;
            
            float moveDirection = Mathf.Sign(horizontalMovement.x);
            ApplyShake(moveDirection);
            
            if (useVibration && Time.time - lastEffectTime > effectCooldown)
            {
                TriggerVibration();
                PlayStirEffect();
            }
            
            if (validMovementCount >= REQUIRED_MOVEMENT_COUNT)
            {
                UpdateStirProgress();
            }
            
            lastEffectTime = Time.time;
        }

        prevFilteredAcceleration = filteredAcceleration;
        lastShakeCheckTime = Time.time;
    }

    private void UpdateStirProgress()
    {
        isStirring = true;
        currentStirTime += Time.deltaTime * 1.5f;
        
        if (showDebugInfo)
        {
            Debug.Log($"UpdateStirProgress - Current Time: {currentStirTime:F2}");
        }
        
        if (currentStirTime >= requiredStirTime)
        {
            LoadNextScene();
        }
    }

    private void ResetStirProgress()
    {
        if (isStirring && Time.time - lastEffectTime > MOVEMENT_RESET_TIME)
        {
            currentStirTime = Mathf.Max(0, currentStirTime - (Time.deltaTime * 0.5f));
            if (currentStirTime <= 0)
            {
                isStirring = false;
                validMovementCount = 0;
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