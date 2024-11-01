using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditManager : MonoBehaviour
{
    public TextMeshProUGUI creditText; // 크레딧을 표시할 텍스트 UI
    public EndingManager endingManager; // EndingManager 스크립트의 참조 (새로 추가)

    private Dictionary<int, string[]> creditsByScene = new Dictionary<int, string[]>(); // 씬별 크레딧 문구 저장

    private int currentCreditIndex = 0; // 현재 표시 중인 크레딧의 인덱스
    private string[] currentCredits; // 현재 씬의 크레딧 배열

    void Awake()
    {
        // 씬 인덱스별 크레딧 문구 추가
        creditsByScene.Add(0, new string[]
        {
            "어떻게 주문을 \n 한번을 성공을 못하냐!",
            "바로 짐 싸서 \n 내일부터는 나오지 말게!"
        });

        creditsByScene.Add(1, new string[]
        {
            "언젠간 이런날이 \n 올 줄 알았지만... \n 그게 오늘일 줄이야....",
            "자네는 대체 장사를 \n 어떻게 했으면 \n 수익률이 마이너스인가!?"
        });
        
        creditsByScene.Add(2, new string[]
        {
            "저는 스테이크의 \n 굽기 정도를 \n 중요시 여기거든요.",
            "음...\n 이 스테이크는 \n 이븐하게 익었군요"
        });
        
        creditsByScene.Add(3, new string[]
        {
            "정비는 예술이다!",
            "정비의 정상화!"
        });
        
    }

    // 특정 씬 번호에 해당하는 크레딧 문구를 표시하는 함수
    public void DisplayCreditsForScene(int sceneIndex)
    {
        // creditText가 올바르게 할당되었는지 확인
        if (creditText == null)
        {
            Debug.LogError("CreditText가 할당되지 않았습니다.");
            return;
        }

        // 해당 씬의 크레딧이 존재하는지 확인
        if (creditsByScene.ContainsKey(sceneIndex))
        {
            // 현재 씬의 크레딧 배열 설정
            currentCredits = creditsByScene[sceneIndex];

            // 크레딧 인덱스 초기화
            currentCreditIndex = 0;

            // 첫 번째 크레딧 라인 표시
            creditText.text = currentCredits[currentCreditIndex];
        }
        else
        {
            creditText.text = "크레딧: 준비 중입니다.";
        }
    }

    void Update()
    {
        // 터치 또는 클릭 입력 감지
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (currentCredits != null && currentCreditIndex < currentCredits.Length - 1)
            {
                // 다음 크레딧 라인으로 이동
                currentCreditIndex++;
                creditText.text = currentCredits[currentCreditIndex];
            }
            else
            {
                // 모든 크레딧이 표시되었으면 EndingManager에 동작 요청
                Debug.Log("크레딧이 모두 표시되었습니다.");

                // EndingManager의 함수 호출
                if (endingManager != null)
                {
                    endingManager.StartFadeOutAndShowFinalMessage();
                }
                else
                {
                    Debug.LogWarning("EndingManager가 할당되지 않았습니다.");
                }

                // 크레딧 텍스트 숨기기 (필요에 따라)
                creditText.gameObject.SetActive(false);
            }
        }
    }
}
