using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditManager : MonoBehaviour
{
    public TextMeshProUGUI creditText; // 크레딧을 표시할 텍스트 UI

    private Dictionary<int, string[]> creditsByScene = new Dictionary<int, string[]>(); // 씬별 크레딧 문구 저장

    void Awake()
    {
        // 씬 인덱스별 크레딧 문구 추가
        creditsByScene.Add(0, new string[]
        {
            "당신은 칵테일을 \n 만드는데 소질이 \n 없었습니다.",
             "손님의 모든 주문을 \n 틀린 당신은 결국 \n 해고를 당했습니다.",
             "뭐...\n 각자 잘하는게 \n 있는거니까요.. \n 너무 낙심하지 \n 맙시다."
        });

        creditsByScene.Add(1, new string[]
        {
            "분명 일을 하였지만..왜 잔고가 점점 비어가는 걸까요?",
            "모든 돈을 잃은 행오버 바는 결국 폐업 절차를 걷게 되었습니다.",
            "아쉽지만.."
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
            // 모든 크레딧 라인을 하나의 문자열로 결합
            string fullCreditText = string.Join("\n\n", creditsByScene[sceneIndex]);

            // 텍스트 설정
            creditText.text = fullCreditText;
        }
        else
        {
            creditText.text = "크레딧: 준비 중입니다.";
        }
    }
}