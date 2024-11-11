using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // TextMeshPro 사용을 위해 추가

public class ArcadeManager : MonoBehaviour
{
    // 변수 선언
    public int Made_ID;  // 제작된 술 ID 저장
    public int[] Ingredient_Arr;  // 사용한 재료 배열
    public int Ingredient_Cost;  // 현재 술의 원재료 값
    public int Ingredient_Total_Cost;  // 이번 판에 사용된 모든 원재료 값의 합
    public int Sale_Cost;  // 판매한 칵테일의 가격
    public int Sale_Total_Cost;  // 판매한 칵테일의 총 가격
    public int Gold;  // 자산 저장
    public int Life;  // 남은 목숨

    private float timer;  // 타이머 변수
    private bool isTimerEnded;  // 타이머 종료 트리거

    public TextMeshProUGUI timerText;  // 타이머를 표시할 UI 텍스트
    
    public ArcadeDialogue arcadeDialogue;  // ArcadeDialogue 참조

    // Start is called before the first frame update
    void Start()
    {
        // 초기값 설정
        Ingredient_Total_Cost = 0;
        Sale_Total_Cost = 0;
        Gold = 100;  // 예시 초기값, 필요에 따라 조정
        Life = 3;    // 예시 초기값, 필요에 따라 조정

        // 타이머 초기화 (5초)
        timer = 5f;
        isTimerEnded = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 타이머 감소 처리
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();  // UI 업데이트
        }
        else if (!isTimerEnded)
        {
            // 타이머가 끝난 경우 트리거 설정
            isTimerEnded = true;
            timerText.text = "Time's up!";  // 타이머 종료 시 메시지 표시 
        }
    }

    // 타이머 UI 업데이트 메서드
    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    
    }
    
    private void StartOrderPhase()
    {
        // ArcadeDialogue에서 주문 단계 초기화
        if (arcadeDialogue != null)
        {
            arcadeDialogue.InitializeOrderPhase();
        }
        else
        {
            Debug.LogError("ArcadeDialogue 참조가 설정되지 않았습니다.");
        }
    }
}