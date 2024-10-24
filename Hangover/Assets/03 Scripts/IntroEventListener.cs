using UnityEngine;

public class IntroEventListener : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float minDragDistance = 100f; // 최소 드래그 거리
    [SerializeField] private float dragMovementSpeed = 5f; // 드래그 시 오브젝트 이동 속도
    [SerializeField] private float maxDragMovement = 10f; // 드래그로 이동 가능한 최대 거리

    [Header("Sensor Settings")]
    [SerializeField] private float shakeThreshold = 2.0f; // 흔들기 감지 임계값
    [SerializeField] private float sensorUpdateInterval = 0.1f; // 센서 업데이트 간격
    [SerializeField] private float shakeMovementAmount = 0.1f; // 흔들기 시 이동량
    
    [Header("Movement Settings")]
    [SerializeField] private float returnSpeed = 2f; // 원위치로 돌아오는 속도
    [SerializeField] private Vector3 originalPosition; // 초기 위치 저장

    private Vector2 touchStartPos;
    private bool isDragging = false;
    private float lastUpdateTime;
    private Vector3 lastAcceleration;
    private SceneController sceneController;
    private Vector3 targetPosition;

    private void Start()
    {
        sceneController = FindObjectOfType<SceneController>();
        if (sceneController == null)
        {
            Debug.LogError("SceneController not found in the scene!");
        }

        originalPosition = transform.position;
        targetPosition = originalPosition;
        lastAcceleration = Input.acceleration;
        lastUpdateTime = Time.time;
    }

    private void Update()
    {
        CheckDragInput();
        CheckShakeInput();
        UpdateObjectPosition();
    }

    private void CheckDragInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        // 드래그 중 오브젝트 이동
                        float verticalDelta = (touch.position.y - touchStartPos.y) * dragMovementSpeed;
                        verticalDelta = Mathf.Clamp(verticalDelta, -maxDragMovement, maxDragMovement);
                        targetPosition = originalPosition + new Vector3(0, verticalDelta, 0);
                    }
                    break;

                case TouchPhase.Ended:
                    if (isDragging)
                    {
                        float verticalDragDistance = touch.position.y - touchStartPos.y;
                        if (Mathf.Abs(verticalDragDistance) > minDragDistance)
                        {
                            LoadNextScene();
                        }
                        isDragging = false;
                        // 드래그 종료 시 원위치로 돌아가기
                        targetPosition = originalPosition;
                    }
                    break;
            }
        }
    }

    private void CheckShakeInput()
    {
        if (Time.time - lastUpdateTime < sensorUpdateInterval) return;

        Vector3 acceleration = Input.acceleration;
        Vector3 accelerationDelta = acceleration - lastAcceleration;
        float shakeMagnitude = accelerationDelta.magnitude;

        if (shakeMagnitude > shakeThreshold)
        {
            // 흔들기 감지 시 랜덤한 방향으로 오브젝트 이동
            Vector3 randomOffset = Random.insideUnitSphere * shakeMovementAmount;
            targetPosition = originalPosition + randomOffset;
            
            if (shakeMagnitude > shakeThreshold * 1.5f) // 강한 흔들기
            {
                LoadNextScene();
            }
        }

        lastAcceleration = acceleration;
        lastUpdateTime = Time.time;
    }

    private void UpdateObjectPosition()
    {
        // 부드러운 이동 처리
        if (!isDragging && Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
        }
        else if (isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, dragMovementSpeed * Time.deltaTime);
        }
    }

    private void LoadNextScene()
    {
        if (sceneController != null)
        {
            sceneController.LoadNextScene();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 이동 가능 범위 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(originalPosition, new Vector3(0.1f, maxDragMovement * 2, 0.1f));
    }
}