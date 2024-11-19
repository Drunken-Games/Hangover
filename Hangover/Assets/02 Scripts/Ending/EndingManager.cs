using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    public GameObject[] endingImages; // 엔딩 이미지를 담은 게임 오브젝트 배열
    public CreditManager creditManager; // CreditManager 스크립트의 참조

    [SerializeField]
    private int endingIndex = 0; // 표시할 엔딩 번호 (외부에서 설정 가능하도록 SerializeField 사용)

    void Start()
    {
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
}