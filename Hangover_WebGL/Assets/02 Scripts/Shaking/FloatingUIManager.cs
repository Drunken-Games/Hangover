using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FloatingUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private Button floatingButton; // 플로팅 버튼

    [SerializeField] private Button closeButton; // 닫기 버튼
    [SerializeField] private RectTransform floatingPanel; // 플로팅 패널
    [SerializeField] private BuildManager buildManager; // BuildManager 참조

    [Header("Animation Settings")]
    [SerializeField]
    private float showDuration = 0.5f;

    [SerializeField] private float hideDuration = 0.3f;
    [SerializeField] private Vector2 hiddenPosition = new Vector2(100f, 0f); // 숨겨진 위치 (오른쪽)
    [SerializeField] private Vector2 shownPosition = Vector2.zero; // 보여질 위치

    [Header("Close Button Animation")]
    [SerializeField]
    private float closeButtonRotation = 180f; // 닫기 버튼 회전 각도

    [SerializeField] private float closeButtonAnimDuration = 0.3f; // 닫기 버튼 애니메이션 시간

    private bool isPanelVisible = false;
    private const int MIN_INGREDIENTS_COUNT = 6; // 최소 재료 개수

    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }

    private void InitializeUI()
    {
        // 플로팅 버튼 초기 설정
        if (floatingButton)
        {
            floatingButton.gameObject.SetActive(false);
        }

        // 플로팅 패널 초기 설정
        if (floatingPanel)
        {
            floatingPanel.anchoredPosition = hiddenPosition;
            floatingPanel.gameObject.SetActive(false);
        }

        // 닫기 버튼 초기 설정
        if (closeButton)
        {
            // 닫기 버튼의 이미지가 있다면 초기 회전 설정
            if (closeButton.transform.GetComponent<RectTransform>())
            {
                closeButton.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void SetupEventListeners()
    {
        // 플로팅 버튼 클릭 이벤트
        if (floatingButton)
        {
            floatingButton.onClick.AddListener(TogglePanel);
        }

        // 닫기 버튼 클릭 이벤트
        if (closeButton)
        {
            closeButton.onClick.AddListener(() =>
            {
                AnimateCloseButton();
                HidePanel();
            });
        }

        // BuildManager 이벤트 구독
        if (buildManager)
        {
            buildManager.OnIngredientsChanged += CheckIngredientCount;
        }
    }

    private void AnimateCloseButton()
    {
        if (!closeButton) return;

        // 닫기 버튼 회전 애니메이션
        closeButton.transform
            .DORotate(new Vector3(0, 0, closeButtonRotation), closeButtonAnimDuration)
            .SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                // 애니메이션 완료 후 원래 상태로 리셋
                closeButton.transform.rotation = Quaternion.identity;
            });
    }

    private void CheckIngredientCount(int count)
    {
        if (floatingButton)
        {
            bool shouldBeActive = count >= MIN_INGREDIENTS_COUNT;

            // 버튼이 활성화되어야 하고 현재 비활성화 상태일 때
            if (shouldBeActive && !floatingButton.gameObject.activeSelf)
            {
                ShowFloatingButton();
            }
            // 버튼이 비활성화되어야 하고 현재 활성화 상태일 때
            else if (!shouldBeActive && floatingButton.gameObject.activeSelf)
            {
                HideFloatingButton();
            }
        }
    }

    private void ShowFloatingButton()
    {
        floatingButton.gameObject.SetActive(true);
        floatingButton.transform.localScale = Vector3.zero;
        floatingButton.transform.DOScale(1f, showDuration).SetEase(Ease.OutBack);
    }

    private void HideFloatingButton()
    {
        floatingButton.transform.DOScale(0f, hideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                floatingButton.gameObject.SetActive(false);

                // 패널이 열려있다면 닫기
                if (isPanelVisible)
                {
                    HidePanel();
                }
            });
    }

    private void TogglePanel()
    {
        if (isPanelVisible)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    private void ShowPanel()
    {
        if (!floatingPanel) return;

        floatingPanel.gameObject.SetActive(true);
        floatingPanel.DOAnchorPos(shownPosition, showDuration)
            .SetEase(Ease.OutBack);

        // 닫기 버튼 초기 상태 설정
        if (closeButton)
        {
            closeButton.transform.rotation = Quaternion.identity;
        }

        isPanelVisible = true;
    }

    private void HidePanel()
    {
        if (!floatingPanel) return;

        floatingPanel.DOAnchorPos(hiddenPosition, hideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => floatingPanel.gameObject.SetActive(false));

        isPanelVisible = false;
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 제거 및 실행 중인 모든 트윈 종료
        if (buildManager)
        {
            buildManager.OnIngredientsChanged -= CheckIngredientCount;
        }

        if (floatingButton)
        {
            floatingButton.onClick.RemoveListener(TogglePanel);
            DOTween.Kill(floatingButton.transform);
        }

        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            DOTween.Kill(closeButton.transform);
        }

        if (floatingPanel)
        {
            DOTween.Kill(floatingPanel);
        }
    }
}