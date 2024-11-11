using System.IO; // 파일 입출력을 위한 네임스페이스
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위한 네임스페이스 추가

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText; // 결과를 표시할 텍스트

    // 데이터를 저장하는 함수
    public void SaveGame(int dayNum, int nowMoney, string playerName, int fireCount, int robotCount, int endingTrigger, string saveDateTime)
    {
        // SaveData 객체 생성 및 값 할당
        SaveData saveData = new SaveData
        {
            dayNum = dayNum,
            nowMoney = nowMoney,
            beforeMoney = 0,
            totalProfit = 0,
            tip = 0,
            refund = 0,
            materials = 0,
            netProfit = 0,
            afterMoney = 0,
            playerName = playerName,
            fireCount = fireCount,
            robotCount = robotCount,
            endingTrigger = endingTrigger,
            saveDateTime = saveDateTime
        };

        // SaveData를 JSON 문자열로 변환
        string json = JsonUtility.ToJson(saveData);

        // JSON 문자열을 PlayerPrefs에 저장
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save(); // 변경 사항을 저장
        Debug.Log("게임 저장 완료: " + json);
    }

    // 데이터를 로드하는 함수
    public SaveData LoadGame()
    {
        // PlayerPrefs에서 JSON 문자열 읽기
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");

            // JSON 문자열을 SaveData 객체로 변환
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("게임 로드 완료: " + json);

            // GameManager에 데이터 할당
            GameManager.instance.dayResultData.dayNum = saveData.dayNum + 1; // 이전 완료한 일차는 다음으로 진행하기 위해 +1
            GameManager.instance.dayResultData.beforeMoney = saveData.nowMoney;
            GameManager.instance.dayResultData.playerName = saveData.playerName;
            GameManager.instance.dayResultData.fireCount = saveData.fireCount;
            GameManager.instance.dayResultData.robotCount = saveData.robotCount;
            GameManager.instance.dayResultData.endingTrigger = saveData.endingTrigger;

            Debug.Log(" GameManager.instance.dayResultData.dayNum: "+ GameManager.instance.dayResultData.dayNum);

            return saveData;
        }
        else
        {
            Debug.LogWarning("저장 파일이 존재하지 않습니다.");
            return null;
        }
    }

    // 버튼 클릭 시 호출되는 함수
    public void LoadButtonClicked()
    {
        SaveData loadedData = LoadGame(); // LoadGame 호출하여 데이터 로드

        if (loadedData != null)
        {
            // 로드된 데이터를 JSON 문자열로 변환
            string json = JsonUtility.ToJson(loadedData, true);
            Debug.Log("로드된 데이터: " + json); // JSON 출력

            string result = "일차: " + loadedData.dayNum + "\n" +
                            "자산: " + loadedData.nowMoney + "\n" +
                            "플레이어: " + loadedData.playerName + "\n" +
                            "분기: " + loadedData.endingTrigger + "\n" +
                            "저장일시: " + loadedData.saveDateTime;

            // ResultText에 출력
            resultText.text = result;
            // GameScene으로 전환
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("로드된 데이터가 없습니다.");
        }
    }
}
