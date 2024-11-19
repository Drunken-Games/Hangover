using UnityEngine;
using DG.Tweening;
using TMPro;

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
    [SerializeField] private TextMeshPro textMeshProObject;
    [SerializeField] private bool useVibration = true;
    [SerializeField] private float effectCooldown = 0.1f;
    [SerializeField] private float soundFadeInDuration = 0.2f;
    [SerializeField] private float soundFadeOutDuration = 0.3f;
    [SerializeField] private float sceneTransitionSoundFadeTime = 0.5f;
    [SerializeField] private float soundFadeStartOffset = 0.2f;

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
    private bool isPlayingSound = false;
    private bool isFadingOut = false;
    
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

    private void Update()
    {
        HandleInput();
        UpdateTextTransparency();
        if (!isDragging)
        {
            CheckStirMovement();
        }

        // 씬 전환 전 페이드아웃 체크
        if (isStirring && !isFadingOut)
        {
            float remainingTime = requiredStirTime - currentStirTime;
            if (remainingTime <= sceneTransitionSoundFadeTime + soundFadeStartOffset)
            {
                StartTransitionFade();
            }
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

    private void StartTransitionFade()
    {
        if (isPlayingSound && !isFadingOut)
        {
            isFadingOut = true;
            if (SoundsManager.instance != null)
            {
                SoundsManager.instance.StopSFXWithFade("stirring", sceneTransitionSoundFadeTime);
            }
            isPlayingSound = false;
        }
    }

    private void HandleInput()
    {
        // 마우스 클릭이 감지되면 입력을 처리합니다.
        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼 클릭 감지
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

            // 드래그 시작
            if (!isDragging)
            {
                float touchDistance = Mathf.Abs(mouseWorldPos.x - transform.position.x);
                if (touchDistance < 1f) // 객체와의 거리 체크
                {
                    isDragging = true; // 드래깅 상태로 변경
                    currentTouchTime = 0f;
                    lastTouchPosition = mouseWorldPos; // 마지막 위치 저장
                }
            }
            else // 드래그 중
            {
                currentTouchTime += Time.deltaTime; // 드래그 시간 증가
                if (currentTouchTime >= touchHoldTime)
                {
                    LoadNextScene(); // 드래그 시간이 충분하면 다음 씬 로드
                    return;
                }

                float deltaX = mouseWorldPos.x - lastTouchPosition.x; // 마우스 이동 거리 계산
                if (Mathf.Abs(deltaX) > 0.1f) // 이동 거리가 임계값을 초과하면
                {
                    MoveSpoon(deltaX * touchSensitivity); // 스푼 이동
                    lastTouchPosition = mouseWorldPos; // 마지막 위치 업데이트
                    PlayStirEffect(); // 혼합 효과 재생
                    UpdateStirProgress(); // 혼합 진행도 업데이트
                }
            }
        }
        else // 마우스 버튼이 눌리지 않으면
        {
            if (isDragging) // 드래그 중이었다면
            {
                isDragging = false; // 드래깅 상태 해제
                currentTouchTime = 0f; // 드래그 시간 초기화
            }
            ResetStirProgress(); // 혼합 진행도 초기화
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
        else
        {
            StopStirEffect();
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
                StopStirEffect();
            }
        }
    }

    private void LoadNextScene()
    {
        StopStirEffect();
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
        if (!isPlayingSound && !isFadingOut && SoundsManager.instance != null)
        {
            SoundsManager.instance.PlaySFXWithFade("stirring", soundFadeInDuration);
            isPlayingSound = true;
        }
    }

    private void StopStirEffect()
    {
        if (isPlayingSound && !isFadingOut && SoundsManager.instance != null)
        {
            SoundsManager.instance.StopSFXWithFade("stirring", soundFadeOutDuration);
            isPlayingSound = false;
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
    
    private void UpdateTextTransparency()
    {
        // Check if textMeshProObject is assigned
        if (textMeshProObject == null)
        {
            Debug.LogWarning("TextMeshPro object is not assigned!");
            return;
        }

        // Get the current color
        Color color = textMeshProObject.color;

        // Set alpha to 0 if either isBeingTouched or isShaking is true, otherwise set it to full opacity
        color.a = (isDragging || isStirring) ? 0 : 1;
        
        // Apply the updated color back to the text object
        textMeshProObject.color = color;
    }

    private void OnDisable()
    {
        StopStirEffect();
    }

    private void OnDestroy()
    {
        StopStirEffect();
        currentShakeTween?.Kill();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 1f, 1f));
    }
}