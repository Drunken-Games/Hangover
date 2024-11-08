using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Shaker : MonoBehaviour
{
    [Header("Touch Settings")]
    [SerializeField] private float maxDistanceFromOrigin = 10f;
    [SerializeField] private float returnSpeed = 0.5f;
    [SerializeField] private Ease returnEase = Ease.OutElastic;
    
    [Header("Shake Settings")]
    private float someMovementThreshold = 10.0f;
    [SerializeField] private float shakeThreshold = 20.0f;
    [SerializeField] private float shakePower = 1.0f;
    [SerializeField] private float verticalMultiplier = 1.0f;
    [SerializeField] private Vector2 verticalRange = new Vector2(-5f, 5f);
    [SerializeField] private Ease shakeEase = Ease.OutQuad;
    // [SerializeField] private float accelerationSmoothness = 10f;

    [Header("Effect Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;
    // [SerializeField] private float soundFadeOutTime = 0.3f;
    // [SerializeField] private float soundVolume = 0.7f;
    // [SerializeField] private float soundFadeInTime = 0.1f;
    [SerializeField] private bool useVibration = true;
    [SerializeField] private float effectCooldown = 0.1f;
    [SerializeField] private float soundCheckInterval = 0.2f;
    [SerializeField] private float lastSoundTime = 0f;
    [SerializeField] private float soundInterval = 0.1f; // 소리 재생 간격
    // [SerializeField] private bool isSoundPlaying = false;

    [Header("Scene Change Settings")]
    [SerializeField] private float requiredShakeTime = 1.5f;
    [SerializeField] private float touchHoldTime = 2f;
    [SerializeField] private string nextSceneName = "GameScene";
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Sound Settings")]
    [SerializeField] private float soundFadeInDuration = 0.2f;
    [SerializeField] private float soundFadeOutDuration = 0.3f;
    private bool isPlayingSound = false;
    private Tween currentSoundFade;

    private Vector3 originalPosition;
    private Vector3 lastAcceleration;
    private float lastEffectTime;
    private float lastMovementTime;
    private bool isBeingTouched = false;
    private bool isMoving = false;
    private Camera mainCamera;
    private Vector2 lastTouchPosition;
    private float currentShakeTime = 0f;
    private float currentTouchTime = 0f;
    private Tween currentMoveTween;
    private bool isShaking = false;
    private SceneController sceneController;
    private Tween currentShakeTween;
    private Tween currentSoundTween;
    private bool isInitialized = false;
    private float lastShakeCheckTime;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (isInitialized) return;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Waiting for camera...");
            StartCoroutine(WaitForCamera());
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        SetupAudioSource();

        originalPosition = transform.position;
        lastAcceleration = Vector3.zero;
        lastEffectTime = Time.time;
        lastMovementTime = Time.time;
        lastShakeCheckTime = Time.time;
        
        sceneController = FindObjectOfType<SceneController>();
        if (sceneController == null)
        {
            Debug.LogWarning("SceneController not found! Adding one...");
            GameObject controllerObject = new GameObject("SceneController");
            sceneController = controllerObject.AddComponent<SceneController>();
        }

        Input.gyro.enabled = true;
        StartCoroutine(InitializeAccelerometer());

        isInitialized = true;
        Debug.Log("Shaker initialized successfully!");
    }

    private IEnumerator WaitForCamera()
    {
        while (Camera.main == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        mainCamera = Camera.main;
        InitializeComponents();
    }

    private IEnumerator InitializeAccelerometer()
    {
        float stabilizationTime = 0.5f;
        float elapsedTime = 0f;
        Vector3 accumulatedAcceleration = Vector3.zero;
        int sampleCount = 0;

        while (elapsedTime < stabilizationTime)
        {
            accumulatedAcceleration += Input.acceleration;
            sampleCount++;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lastAcceleration = accumulatedAcceleration / sampleCount;
        Debug.Log($"Accelerometer initialized with baseline: {lastAcceleration}");
    }

    private void SetupAudioSource()
    {
        audioSource.loop = true;
        audioSource.volume = 0;
        audioSource.playOnAwake = false;
        audioSource.clip = moveSound;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            InitializeComponents();
            return;
        }

        HandleMouseInput(); // 터치 입력 대신 마우스 입력 처리
        ProcessShakeInput();
        UpdateSoundState();

        if (showDebugInfo)
        {
            DebugLog();
        }
    }
    

    private void ProcessShakeInput()
    {
        if (!isInitialized || isBeingTouched) return;

        Vector3 currentAcceleration = Input.acceleration;
        float timeSinceLastCheck = Time.time - lastShakeCheckTime;

        Vector3 filteredAcceleration = Vector3.Lerp(lastAcceleration, currentAcceleration, 0.5f);
        Vector3 accelerationDelta = (filteredAcceleration - lastAcceleration) / timeSinceLastCheck;
        float shakeMagnitude = accelerationDelta.magnitude;

        Debug.Log($"Current Acc: {filteredAcceleration}, Delta: {accelerationDelta}, Magnitude: {shakeMagnitude}");

        if (shakeMagnitude > shakeThreshold)
        {
            if (!isShaking)
            {
                isShaking = true;
                currentShakeTime = 0f;
                Debug.Log($"Shake started! Magnitude: {shakeMagnitude:F3}");
            }

            currentShakeTime += timeSinceLastCheck;

            if (currentShakeTime >= requiredShakeTime)
            {
                Debug.Log("Required shake time reached!");
                LoadNextScene();
                return;
            }

            if (shakeMagnitude > someMovementThreshold) 
            {
                UpdateMovement();
                Vector3 shakeDirection = new Vector3(accelerationDelta.x, accelerationDelta.y * verticalMultiplier, 0).normalized;
                ApplyShake(shakeDirection);

                // 특정 임계값 이상일 때만 소리 재생
                if (shakeMagnitude > shakeThreshold && Time.time - lastSoundTime > soundInterval)
                {
                    PlaySoundWithFade();
                }

                if (useVibration && Time.time - lastEffectTime > effectCooldown)
                {
                    TriggerVibration();
                    lastEffectTime = Time.time;
                }
            }
        }
        else
        {
            if (isShaking)
            {
                currentShakeTime = Mathf.Max(0, currentShakeTime - (timeSinceLastCheck * 0.5f));
                if (currentShakeTime == 0)
                {
                    isShaking = false;
                    Debug.Log("Shake ended!");
                }
            }
        }

        lastAcceleration = filteredAcceleration;
        lastShakeCheckTime = Time.time;
    }



    private void DebugLog()
    {
        if (isShaking)
        {
            Debug.Log($"Shake Progress: {currentShakeTime:F2}/{requiredShakeTime:F2}");
        }
        if (isBeingTouched)
        {
            Debug.Log($"Touch Hold Progress: {currentTouchTime:F2}/{touchHoldTime:F2}");
        }
    }

    private void UpdateSoundState()
    {
        if (isMoving && Time.time - lastMovementTime > soundCheckInterval)
        {
            isMoving = false;
            StopSoundWithFade();
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isBeingTouched = true;
            currentTouchTime = 0f;
            lastTouchPosition = Input.mousePosition;

            StartMovement();
            SoundsManager.instance.PlaySFX("shaker");
        }

        if (isBeingTouched)
        {
            currentTouchTime += Time.deltaTime;

            if (currentTouchTime >= touchHoldTime)
            {
                LoadNextScene();
                return;
            }

            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 
                mainCamera.WorldToScreenPoint(transform.position).z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                
            transform.position = ClampPosition(worldPos);

            if (Vector2.Distance(lastTouchPosition, Input.mousePosition) > 5f)
            {
                UpdateMovement();
                lastTouchPosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isBeingTouched = false;
            currentTouchTime = 0f;
            ReturnToOriginalPosition();
        }
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        Vector3 offset = position - originalPosition;
        
        Vector2 horizontalOffset = new Vector2(offset.x, offset.z);
        if (horizontalOffset.magnitude > maxDistanceFromOrigin)
        {
            horizontalOffset = horizontalOffset.normalized * maxDistanceFromOrigin;
            offset.x = horizontalOffset.x;
            offset.z = horizontalOffset.y;
        }
        
        offset.y = Mathf.Clamp(offset.y, verticalRange.x, verticalRange.y);
        
        return originalPosition + offset;
    }

    private void StartMovement()
    {
        isMoving = true;
        lastMovementTime = Time.time;
        PlaySoundWithFade();
    }
    
    private void StopSound()
    {
        if (isPlayingSound)
        {
            StopSoundWithFade();
            isPlayingSound = false;
        }
    }
    
    private void StopSoundWithFade()
    {
        if (isPlayingSound)
        {
            SoundsManager.instance.StopSFXWithFade("shaker", soundFadeOutDuration);
            isPlayingSound = false;
        }
    }
    
      private void PlaySoundWithFade()
    {
        if (!isPlayingSound || Time.time - lastSoundTime >= soundInterval)
        {
            // 이전 페이드 트윈이 있다면 제거
            currentSoundFade?.Kill();

            // 소리가 이미 재생 중이 아니라면 새로 재생
            if (!isPlayingSound)
            {
                SoundsManager.instance.PlaySFXWithFade("shaker", soundFadeInDuration);
            }
            
            lastSoundTime = Time.time;
            isPlayingSound = true;
        }
    }
    
    private void ApplyShake(Vector3 direction)
    {
        KillTweens(false);

        float shakeDuration = 0.3f;
        float returnDuration = 0.8f;

        Sequence shakeSequence = DOTween.Sequence();
        
        Vector3 currentOffset = transform.position - originalPosition;
        Vector3 shakeOffset = direction * shakePower;
        Vector3 targetOffset = currentOffset + shakeOffset;
        Vector3 targetPos = ClampPosition(originalPosition + targetOffset);

        shakeSequence
            .Append(transform.DOMove(targetPos, shakeDuration).SetEase(shakeEase))
            .Append(transform.DOMove(originalPosition, returnDuration).SetEase(Ease.OutElastic));

        currentShakeTween = shakeSequence;
    }

    private void UpdateMovement()
    {
        if (!isMoving)
        {
            isMoving = true;
            lastMovementTime = Time.time;
            PlaySoundWithFade();
        }
        else
        {
            lastMovementTime = Time.time;
        }
    }

    private void LoadNextScene()
    {
        if (sceneController != null)
        {
            Debug.Log("Loading next scene...");
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
    

    private void TriggerVibration()
    {
        #if UNITY_IOS
        // HapticPatterns.GenerateHapticPattern(HapticPatterns.PresetType.MediumImpact);
        #elif UNITY_ANDROID
        Handheld.Vibrate();
        #endif
    }

    private void ReturnToOriginalPosition()
    {
        KillTweens(false);
        currentMoveTween = transform.DOMove(originalPosition, returnSpeed)
            .SetEase(returnEase);
    }

    private void KillTweens(bool killSound = true)
    {
        currentMoveTween?.Kill();
        currentShakeTween?.Kill();
        currentSoundFade?.Kill();
    
        if (killSound)
        {
            StopSoundWithFade();
        }
    }

    private void OnDisable()
    {
        StopSoundWithFade();
        isInitialized = false;
    }

    private void OnDestroy()
    {
        KillTweens(true);
    }
}