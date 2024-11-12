using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // TextMeshPro 사용을 위해 추가

public class ArcadeManager : MonoBehaviour
{
    // 변수 선언
    public TextMeshProUGUI timerText;  // 타이머를 표시할 UI 텍스트
    public TextMeshProUGUI goldText;   // 골드를 표시할 UI 텍스트
    
    public ArcadeDialogue arcadeDialogue;  // ArcadeDialogue 참조
    public GameObject[] lifeImages;  // 생명 수에 따라 비활성화할 이미지 배열
    
    // Start is called before the first frame update
    void Start()
    {
        // 타이머가 이미 실행 중이고 종료되지 않은 경우에만 타이머 시작
        if (!GameManager.instance.isArcadeTimerRunning && !GameManager.instance.hasTimerEnded)
        {
            GameManager.instance.StartArcadeTimer(3f); // 예시로 3분 (180초) 타이머 설정
        }

        // ArcadeDialogue 초기화 호출
        if (arcadeDialogue != null)
        {
            arcadeDialogue.InitializePhase();  // ArcadeDialogue에서 단계 결정
        }
        else
        {
            Debug.LogError("ArcadeDialogue 참조가 설정되지 않았습니다.");
        }

        UpdateGoldUI();  // 초기 골드 표시
    }

    // Update is called once per frame
    void Update()
    {
        // 타이머가 종료되었는지 확인
        if (GameManager.instance.GetRemainingArcadeTime() <= 0 && !GameManager.instance.isArcadeTimerRunning)
        {
            // 타이머 종료 상태를 한 번만 처리
            if (timerText.text != "Time's up!")
            {
                timerText.text = "Time's up!";
            }
        }
        else
        {
            // GameManager의 타이머 정보를 UI에 업데이트
            UpdateTimerUI();
        }
        // life에 따른 이미지 비활성화 처리
        switch (GameManager.instance.life)
        {
            case 2:
                if (lifeImages.Length > 0) lifeImages[0].SetActive(false);
                break;
            case 1:
                if (lifeImages.Length > 1)
                {
                    lifeImages[0].SetActive(false);
                    lifeImages[1].SetActive(false);
                }
                break;
            case 0:
                if (lifeImages.Length > 2)                 
                {
                    lifeImages[0].SetActive(false);
                    lifeImages[1].SetActive(false);
                    lifeImages[2].SetActive(false);
                }
                break;
        }
    }
    
    public void DeductMaterialCost(int cost)
    {
        GameManager.instance.ArcadeGold -= cost;
        if (GameManager.instance.ArcadeGold < 0) GameManager.instance.ArcadeGold = 0; // 골드가 0 미만으로 내려가지 않도록 처리
        Debug.Log($"재료 비용 {cost} 골드가 차감되었습니다. 현재 골드: {GameManager.instance.ArcadeGold}");
        UpdateGoldUI(); // UI에 반영
    }
    
    public void AddGold(int amount)
    {
        // GameManager의 ArcadeGold 증가
        GameManager.instance.ArcadeGold += amount;
        Debug.Log($"골드가 {amount}만큼 증가했습니다. 현재 골드: {GameManager.instance.ArcadeGold}");

        UpdateGoldUI();  // 골드 UI 업데이트
    }
    
    // 골드 UI 업데이트 메서드
    private void UpdateGoldUI()
    {
        goldText.text = $"{GameManager.instance.ArcadeGold}G";
    }

    // 타이머 UI 업데이트 메서드
    private void UpdateTimerUI()
    {
        float remainingTime = GameManager.instance.GetRemainingArcadeTime();
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
