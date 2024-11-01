using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// 씬 관리를 위해 네임스페이스 추가
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    public GameObject[] endingImages; // 엔딩 이미지를 담은 게임 오브젝트 배열
    public CreditManager creditManager; // CreditManager 스크립트의 참조

    [SerializeField]
    private int endingIndex = 0; // 표시할 엔딩 번호 (외부에서 설정 가능하도록 SerializeField 사용)

    public Image fadeImage; // 화면 전체를 덮을 투명한 이미지 (검은색)
    public Image fadeImage2; // 페이드 아웃 전용 이미지
    public TextMeshProUGUI finalMessageText; // 최종 마무리 문구를 표시할 텍스트

    // 씬 번호별 마무리 문구 저장용 딕셔너리 (각각 배열로 저장)
    private Dictionary<int, string[]> finalMessagesByScene = new Dictionary<int, string[]>();

    private int currentFinalMessageIndex = 0; // 현재 표시 중인 마무리 문구의 인덱스
    private string[] currentFinalMessages; // 현재 씬의 마무리 문구 배열

    private bool isShowingFinalMessages = false; // 마무리 문구 표시 중인지 여부
    private bool fadeCompleted = false; // 페이드 효과가 완료되었는지 여부

    void Start()
    {
        // 마무리 문구 초기화
        InitializeFinalMessages();

        // endingImages 배열이 올바르게 할당되었는지 확인
        if (endingImages == null || endingImages.Length == 0)
        {
            Debug.LogError("Ending images are not assigned or the array is empty.");
            return;
        }

        // 모든 엔딩 이미지를 비활성화합니다.
        foreach (GameObject image in endingImages)
        {
            image.SetActive(false);
        }

        // 설정된 엔딩 번호를 표시
        ShowEnding(endingIndex);

        // 페이드 이미지와 최종 문구를 초기화
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }
        if (fadeImage2 != null)
        {
            fadeImage2.gameObject.SetActive(false); // 새롭게 추가된 페이드 이미지 초기화
        }
        if (finalMessageText != null)
        {
            finalMessageText.gameObject.SetActive(false);
        }
    }

    // 마무리 문구 초기화 함수
    private void InitializeFinalMessages()
    {
        finalMessagesByScene.Add(0, new string[]
        {
            "당신은 칵테일을 \n 만드는데 소질이 \n 없었습니다.",
            "뭐..각자 잘하는게 \n 있는거니까요.. \n \n 너무 낙심하지 \n 맙시다.",
            "...혹시 일부러 \n 다 틀린거 아니죠?",
            "<color=#D55200>Ending A: 해고</color>"
            
        });

        finalMessagesByScene.Add(1, new string[]
        {
            "당신의 실수와 \n 넉넉한 인심은 \n 행오버의 잔고를 \n 거덜 냈습니다.",
            "계속되는 적자에 \n 시달리던 행오버는 \n 결국 폐업 절차를 \n 밟았습니다.",
            "자영업이 원래 \n 쉽지 않죠. \n \n 너무 낙심하지 \n 맙시다.",
            "뭐...\n 아르바이트생인 \n 당신에게는 크게 \n 상관 없을지 \n 모르겠지만요.",
            "<color=#D55200>Ending B: 파산</color>"
        });

        finalMessagesByScene.Add(2, new string[]
        {
            "요리사의 길을 \n 꾸준히 걸은 \n 안재성은 \n 결국 우주적으로 \n 알아주는 쉐프가 \n 되었습니다.",
            "최근에는 \n '컬러요리사' \n 라는 요리쇼에 \n 심사위원으로 \n 초대까지 받았더군요.",
            "꾸준한 노력과 \n 열정은 꿈을 \n 현실로 만들어줍니다!" ,
            "항상 순탄하지는 \n 않겠지만 \n 당당하게 나아가세요!",
            "<color=#00A2FF>Ending C: 쉐프</color>"
        });

        finalMessagesByScene.Add(3, new string[]
        {
            "요리사의 꿈을 \n 떠난 안재성은 \n 지금은 수많은 장비를 \n 살리는 정비병이 \n 되었습니다",
            "요리사 준비가 \n 시간낭비가 \n 아닐까라는 \n 걱정과 달리",
            "요리 기술의 \n 섬세함은 \n 정비병으로 일하는 \n 안재성에게 \n 강점이 되었습니다",
            "지금까지의 삶이 \n 낭비라고 \n 생각하지 마세요. \n \n 모든 경험은 \n 가치가 있어요!",
            "<color=#6666FF>Ending D: 장교</color>"
            
        });
    }

    // 외부에서 엔딩 인덱스를 설정할 수 있는 메서드
    public void SetEndingIndex(int index)
    {
        endingIndex = index;
    }

    // 특정 엔딩 이미지를 활성화하고 크레딧을 표시하는 함수
    public void ShowEnding(int endingIndex)
    {
        // 모든 엔딩 이미지를 비활성화
        foreach (GameObject image in endingImages)
        {
            image.SetActive(false);
        }

        // 유효한 인덱스일 때만 선택한 엔딩 이미지를 활성화
        if (endingIndex >= 0 && endingIndex < endingImages.Length)
        {
            endingImages[endingIndex].SetActive(true);

            // CreditManager를 통해 해당 씬의 크레딧 표시
            if (creditManager != null)
            {
                creditManager.DisplayCreditsForScene(endingIndex);
            }
            else
            {
                Debug.LogWarning("CreditManager가 할당되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning("유효하지 않은 엔딩 인덱스입니다: " + endingIndex);
        }
    }

    // 크레딧이 종료된 후 호출될 함수
    public void StartFadeOutAndShowFinalMessage()
    {
        // 이미 페이드가 완료되었다면 다시 실행하지 않음
        if (!fadeCompleted)
        {
            StartCoroutine(FadeOutAndPrepareFinalMessagesCoroutine());
        }
    }

    // 이미지의 배경을 서서히 검게 만들고 마무리 문구를 표시할 준비를 하는 코루틴
    private IEnumerator FadeOutAndPrepareFinalMessagesCoroutine()
    {
        fadeCompleted = true; // 페이드 시작을 표시

        // 페이드 이미지를 활성화하고 투명하게 만듭니다.
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }

        // 페이드 인 시간 (조절 가능)
        float fadeDuration = 2f;
        float elapsedTime = 0f;

        // 배경을 서서히 검게 만듭니다.
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = alpha;
                fadeImage.color = color;
            }

            yield return null;
        }

        // 마무리 문구 초기화 및 첫 번째 문구 표시
        if (finalMessageText != null)
        {
            finalMessageText.gameObject.SetActive(true);

            if (finalMessagesByScene.ContainsKey(endingIndex))
            {
                currentFinalMessages = finalMessagesByScene[endingIndex];
                currentFinalMessageIndex = 0;
                finalMessageText.text = currentFinalMessages[currentFinalMessageIndex];
                isShowingFinalMessages = true;
            }
            else
            {
                finalMessageText.text = "마무리 문구가 준비되지 않았습니다.";
                isShowingFinalMessages = false;
            }
        }
    }

    void Update()
    {
        // 마무리 문구 표시 중일 때 입력 감지
        if (isShowingFinalMessages && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            if (currentFinalMessages != null && currentFinalMessageIndex < currentFinalMessages.Length - 1)
            {
                // 다음 마무리 문구로 이동
                currentFinalMessageIndex++;
                finalMessageText.text = currentFinalMessages[currentFinalMessageIndex];
            }
            else
            {
                // 모든 마무리 문구가 표시되었을 때
                Debug.Log("마무리 문구가 모두 표시되었습니다.");
                isShowingFinalMessages = false;

                // 모든 대사가 끝난 후 페이드 아웃 시작
                StartCoroutine(FadeOutAndLoadNextSceneCoroutine());
            }
        }
    }

    // 모든 대사가 끝난 후 페이드 아웃 및 씬 전환
    private IEnumerator FadeOutAndLoadNextSceneCoroutine()
    {
        fadeCompleted = true; // 페이드 시작을 표시

        // 새로운 페이드 이미지(fadeImage2)를 활성화하고 투명하게 만듭니다.
        if (fadeImage2 != null)
        {
            fadeImage2.gameObject.SetActive(true);
            Color color = fadeImage2.color;
            color.a = 0f;
            fadeImage2.color = color;
        }

        float fadeDuration = 1f;
        float elapsedTime = 0f;

        // 서서히 알파 값을 증가시켜 화면을 검게 만듭니다.
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            if (fadeImage2 != null)
            {
                Color color = fadeImage2.color;
                color.a = alpha;
                fadeImage2.color = color;
            }

            yield return null;
        }

        // 페이드 아웃이 완료된 후 씬 전환
        SceneManager.LoadScene("IntroScene"); 
    }
}
