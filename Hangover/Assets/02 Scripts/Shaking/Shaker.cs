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
    [SerializeField] private float shakeThreshold = 0.2f;
    [SerializeField] private float shakePower = 2f;
    [SerializeField] private float verticalMultiplier = 1.5f;
    [SerializeField] private Vector2 verticalRange = new Vector2(-15f, 15f);
    [SerializeField] private Ease shakeEase = Ease.OutQuad;

    [Header("Effect Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private float soundFadeOutTime = 0.3f;
    [SerializeField] private float soundVolume = 0.7f;
    [SerializeField] private float soundFadeInTime = 0.1f;
    [SerializeField] private bool useVibration = true;
    [SerializeField] private float effectCooldown = 0.1f;
    [SerializeField] private float soundCheckInterval = 0.2f;

    [Header("Scene Change Settings")]
    [SerializeField] private float requiredShakeTime = 1.5f;
    [SerializeField] private float touchHoldTime = 2f;
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool showDebugInfo = true;

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

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        SetupAudioSource();

        originalPosition = transform.position;
        lastAcceleration = Input.acceleration;
        lastEffectTime = Time.time;
        lastMovementTime = Time.time;
        
        sceneController = FindObjectOfType<SceneController>();
        if (sceneController == null)
        {
            Debug.LogWarning("SceneController not found! Adding one...");
            GameObject controllerObject = new GameObject("SceneController");
            sceneController = controllerObject.AddComponent<SceneController>();
        }
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
        HandleTouchInput();
        CheckShakeInput();
        UpdateSoundState();

        if (showDebugInfo)
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
    }

    private void UpdateSoundState()
    {
        if (isMoving && Time.time - lastMovementTime > soundCheckInterval)
        {
            isMoving = false;
            FadeOutSound();
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isBeingTouched = true;
                    currentTouchTime = 0f;
                    KillTweens();
                    StartMovement();
                    lastTouchPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    currentTouchTime += Time.deltaTime;
                    
                    if (currentTouchTime >= touchHoldTime)
                    {
                        LoadNextScene();
                        return;
                    }

                    Vector3 touchPos = new Vector3(touch.position.x, touch.position.y, 
                        mainCamera.WorldToScreenPoint(transform.position).z);
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(touchPos);
                    
                    // 위치 제한 적용
                    transform.position = ClampPosition(worldPos);

                    if (Vector2.Distance(lastTouchPosition, touch.position) > 5f)
                    {
                        UpdateMovement();
                        lastTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                    isBeingTouched = false;
                    currentTouchTime = 0f;
                    ReturnToOriginalPosition();
                    break;
            }
        }
    }

    private void CheckShakeInput()
    {
        if (isBeingTouched) return;

        Vector3 acceleration = Input.acceleration;
        Vector3 accelerationDelta = acceleration - lastAcceleration;
        float shakeMagnitude = accelerationDelta.magnitude;

        if (shakeMagnitude > shakeThreshold)
        {
            if (!isShaking)
            {
                isShaking = true;
                currentShakeTime = 0f;
            }

            currentShakeTime += Time.deltaTime;
            
            if (currentShakeTime >= requiredShakeTime)
            {
                LoadNextScene();
                return;
            }

            UpdateMovement();
            
            Vector3 shakeDirection = new Vector3(
                accelerationDelta.x,
                accelerationDelta.y * verticalMultiplier,
                0
            ).normalized;

            ApplyShake(shakeDirection);

            if (useVibration && Time.time - lastEffectTime > effectCooldown)
            {
                TriggerVibration();
                lastEffectTime = Time.time;
            }
        }
        else
        {
            if (isShaking)
            {
                currentShakeTime = Mathf.Max(0, currentShakeTime - (Time.deltaTime * 0.5f));
                if (currentShakeTime == 0)
                {
                    isShaking = false;
                }
            }
        }

        lastAcceleration = acceleration;
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        Vector3 offset = position - originalPosition;
        
        // 수평 위치 제한
        Vector2 horizontalOffset = new Vector2(offset.x, offset.z);
        if (horizontalOffset.magnitude > maxDistanceFromOrigin)
        {
            horizontalOffset = horizontalOffset.normalized * maxDistanceFromOrigin;
            offset.x = horizontalOffset.x;
            offset.z = horizontalOffset.y;
        }
        
        // 수직 위치 제한
        offset.y = Mathf.Clamp(offset.y, verticalRange.x, verticalRange.y);
        
        return originalPosition + offset;
    }

    private void StartMovement()
    {
        isMoving = true;
        lastMovementTime = Time.time;
        
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        FadeInSound();
    }

    private void ApplyShake(Vector3 direction)
    {
        KillTweens(false);

        float shakeDuration = 0.3f;
        float returnDuration = 0.8f;

        Sequence shakeSequence = DOTween.Sequence();
        
        // 현재 위치에서 새로운 목표 위치를 계산
        Vector3 currentOffset = transform.position - originalPosition;
        Vector3 shakeOffset = direction * shakePower;
        
        // 새로운 목표 위치 계산 (현재 오프셋 + 흔들기 오프셋)
        Vector3 targetOffset = currentOffset + shakeOffset;
        
        // 위치 제한 적용
        Vector3 targetPos = ClampPosition(originalPosition + targetOffset);

        // 시퀀스 생성: 흔들기 -> 원위치
        shakeSequence
            .Append(transform.DOMove(targetPos, shakeDuration).SetEase(shakeEase))
            .Append(transform.DOMove(originalPosition, returnDuration).SetEase(Ease.OutElastic));

        currentShakeTween = shakeSequence;
    }

    private void UpdateMovement()
    {
        isMoving = true;
        lastMovementTime = Time.time;
        
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        if (audioSource.volume < soundVolume)
        {
            FadeInSound();
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

    private void FadeInSound()
    {
        currentSoundTween?.Kill();
        currentSoundTween = DOTween.To(() => audioSource.volume,
                                     x => audioSource.volume = x,
                                     soundVolume,
                                     soundFadeInTime);
    }

    private void FadeOutSound()
    {
        currentSoundTween?.Kill();
        currentSoundTween = DOTween.To(() => audioSource.volume,
                                     x => audioSource.volume = x,
                                     0f,
                                     soundFadeOutTime)
                                 .OnComplete(() => {
                                     if (!isMoving)
                                     {
                                         audioSource.Stop();
                                     }
                                 });
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

    private void KillTweens(bool includeSound = true)
    {
        currentMoveTween?.Kill();
        currentShakeTween?.Kill();
        if (includeSound)
        {
            currentSoundTween?.Kill();
            if (!isMoving)
            {
                audioSource.Stop();
                audioSource.volume = 0;
            }
        }
    }

    private void OnDestroy()
    {
        KillTweens(true);
    }
}