using UnityEngine;
using TMPro;
using System;

public class DayResultManager : MonoBehaviour
{
    private SaveSystem saveSystem;
    public DayResultData dayResultData;

     // UI 요소를 연결하기 위한 변수 선언
    [SerializeField] private TMP_Text dayText; // 일차 텍스트
    [SerializeField] private TMP_Text beforeMoneyText; // 이전 자산
    [SerializeField] private TMP_Text totalProfitText; // 총 수익 텍스트
    [SerializeField] private TMP_Text tipText; // 팁 텍스트
    [SerializeField] private TMP_Text refundText; // 환불 텍스트
    [SerializeField] private TMP_Text materialsText; // 총 재료값 텍스트
    [SerializeField] private TMP_Text netProfitText; // 순 수익 텍스트
    [SerializeField] private TMP_Text afterMoneyText; // 일차 후  총 자산

    private void Awake()
    {
        saveSystem = gameObject.AddComponent<SaveSystem>(); // SaveSystem 인스턴스 생성
    }


    private void Start()
    {
        // UI 요소에 초기 값 설정
        dayText.text = dayResultData.dayNum.ToString(); // 완료한 일차
        beforeMoneyText.text = dayResultData.beforeMoney.ToString(); // 이전 자산 
        totalProfitText.text = dayResultData.totalProfit.ToString(); // 총 수익
        tipText.text = dayResultData.tip.ToString(); // 팁
        refundText.text = "-" + dayResultData.refund.ToString(); // 환불 값
        materialsText.text = "-" + dayResultData.materials.ToString(); // 총 재료값
        netProfitText.text = dayResultData.netProfit.ToString(); // 순 수익
        afterMoneyText.text = dayResultData.afterMoney.ToString(); // 일차 후 총 자산

        // DayResultScene 로드 시 SaveGame 호출
        AutoSaveGame();
    }

    private void AutoSaveGame()
    {
        string saveDateTime = DateTime.Now.ToString(); // 저장일시

        // SaveGame 메서드 호출
        saveSystem.SaveGame(dayResultData.dayNum, dayResultData.afterMoney, dayResultData.playerName, dayResultData.branchIdx, saveDateTime);
    }

}
