using UnityEngine;
using DG.Tweening;

public class BarSpoonController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float maxXPosition = 2f;  // X축 최대 이동 범위
    [SerializeField] private float minXPosition = -2f; // X축 최소 이동 범위
    
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

    private Vector3 originalPosition;
    private Vector3 lastAcceleration;
    private float lastEffectTime;
    private Camera mainCamera;
    private Vector2 lastTouchPosition;
    private Tween currentShakeTween;
    private bool isDragging = false;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다!");
            return;
        }

        SetupAudioSource();
        
        originalPosition = transform.position;
        lastAcceleration = Input.acceleration;
        lastEffectTime = Time.time;
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
                    // 터치 위치가 오브젝트 근처인지 확인
                    float touchDistance = Mathf.Abs(touchWorldPos.x - transform.position.x);
                    if (touchDistance < 1f) // 터치 인식 범위 설정
                    {
                        isDragging = true;
                        lastTouchPosition = touchWorldPos;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        float deltaX = touchWorldPos.x - lastTouchPosition.x;
                        MoveSpoon(deltaX * touchSensitivity);
                        lastTouchPosition = touchWorldPos;
                        PlayStirEffect();
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
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
        }

        lastAcceleration = acceleration;
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
        // 터치 가능 영역 시각화 (디버그용)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 1f, 1f));
    }
}