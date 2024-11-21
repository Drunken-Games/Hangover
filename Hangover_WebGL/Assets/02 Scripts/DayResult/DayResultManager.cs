using UnityEngine;
using TMPro;
using System;

public class DayResultManager : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private EndingManager endingManager;
    private SaveSystem saveSystem;
    public DayResultData dayResultData; 


     // UI 요소를 연결하기 위한 변수 선언
    [SerializeField] private TMP_Text dayText; // 일차 텍스트
    [SerializeField] private TMP_Text beforeMoneyText; // 이전 자산
    [SerializeField] private TMP_Text totalProfitText; // 총 수익 텍스트
    [SerializeField] private TMP_Text materialsText; // 총 재료값 텍스트
    [SerializeField] private TMP_Text netProfitText; // 순 수익 텍스트
    [SerializeField] private TMP_Text afterMoneyText; // 일차 후  총 자산

    private void Awake()
    {
        saveSystem = gameObject.AddComponent<SaveSystem>(); // SaveSystem 인스턴스 생성
        dayResultData = GameManager.instance.dayResultData;
    }


    private void Start()
    {
        dayResultData.netProfit = dayResultData.totalProfit - dayResultData.materials;
        dayResultData.afterMoney = dayResultData.beforeMoney + dayResultData.netProfit;

        // UI 요소에 초기 값 설정
        dayText.text = dayResultData.dayNum.ToString(); // 완료한 일차
        beforeMoneyText.text = dayResultData.beforeMoney.ToString(); // 이전 자산 
        totalProfitText.text = dayResultData.totalProfit.ToString(); // 총 수익
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
        saveSystem.SaveGame(
            dayResultData.dayNum,
            dayResultData.afterMoney,
            dayResultData.playerName,
            dayResultData.fireCount,
            dayResultData.robotCount,
            dayResultData.endingTrigger, 
            saveDateTime
        );
    }

    public void ContinueNext() 
    {
        Debug.Log("dayResultData.fireCount: " + dayResultData.fireCount);
        Debug.Log("dayResultData.afterMoney: " + dayResultData.afterMoney);

        if(dayResultData.fireCount >= 7)
        {
            GameManager.instance.endingNumber = 0;
            sceneController.LoadSceneByName("EndingScene");
        }
        else if(dayResultData.afterMoney < 0)
        {
            GameManager.instance.endingNumber = 1;
            sceneController.LoadSceneByName("EndingScene");
        }
        else 
        {
            // dialogueIndex = dayNum + 1 - 1
            GameManager.instance.dayResultData.beforeMoney = dayResultData.afterMoney;
            GameManager.instance.currentDialogueIndex = GameManager.instance.dialogueIndices[dayResultData.dayNum];
            Debug.Log("GameManager.instance.currentDialogueIndex: " + GameManager.instance.currentDialogueIndex);
            sceneController.LoadSceneByName("GameScene");
        }
    }

}
