using UnityEngine;
using TMPro;
using System;

public class DayResultManager : MonoBehaviour
{
    private SaveSystem saveSystem;
     // UI 요소를 연결하기 위한 변수 선언
    public TMP_Text dayText; // "11일째" 텍스트
    public TMP_Text nowMoneyText; //현재 자산
    public TMP_Text totalProfitText; // 총 수익 텍스트
    public TMP_Text tipText; // 팁 텍스트
    public TMP_Text refundText; // 환불 텍스트
    public TMP_Text materialsText; // 사용 재료 텍스트
    public TMP_Text netProfitText; // 순 수익 텍스트
    public TMP_Text totalMoneyText; // 자산 텍스트

    private void Awake()
    {
        // SaveSystem 인스턴스 생성
        saveSystem = gameObject.AddComponent<SaveSystem>();
    }

    private void Start()
    {
        // UI 요소에 초기 값 설정
        dayText.text = "11"; // 예시로 11 설정
        nowMoneyText.text = "0"; // 총 수익 값
        totalProfitText.text = "82.50"; // 팀 값
        tipText.text = "11.32"; // 팁 값
        refundText.text = "-16.11"; // 환불 값
        materialsText.text = "-49.86"; // 사용 재료 값
        netProfitText.text = "2705"; // 순 수익 값
        totalMoneyText.text = "2705"; // 자산 값

        // DayResultScene 로드 시 SaveGame 호출
        SaveGame();
    }

    private void SaveGame()
    {
        int dayNum = 1; // 예시: 현재 일차를 설정
        int nowMoney = 100; // 예시: 현재 금액을 설정
        string playerName = "Player"; // 예시: 플레이어 이름을 설정
        int branchIdx = 0; // 예시: 분기 인덱스
        string saveDateTime = DateTime.Now.ToString(); // 예시: 저장일시

        // SaveGame 메서드 호출
        saveSystem.SaveGame(dayNum, nowMoney, playerName, branchIdx, saveDateTime);
    }
}
