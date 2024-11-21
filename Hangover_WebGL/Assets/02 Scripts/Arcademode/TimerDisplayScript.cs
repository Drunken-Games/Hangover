using UnityEngine;
using TMPro;

public class TimerDisplayScript : MonoBehaviour
{
    public TextMeshProUGUI timerText;  // 타이머를 표시할 UI 텍스트

    void Start()
    {
        // ArcadeStory 상태에 따라 UI 활성화 여부 설정
        timerText.gameObject.SetActive(GameManager.instance.ArcadeStory);
        UpdateTimerUI(); // 초기 시간 표시
    }

    void Update()
    {
        // ArcadeStory 상태가 변경되면 UI 활성화 여부 업데이트
        if (GameManager.instance.ArcadeStory != timerText.gameObject.activeSelf)
        {
            timerText.gameObject.SetActive(GameManager.instance.ArcadeStory);
        }

        // 타이머가 종료되었는지 확인
        if (GameManager.instance.GetRemainingArcadeTime() <= 0)
        {
            timerText.text = "Time's up!"; // 타임 업 메시지
        }
        else
        {
            UpdateTimerUI(); // 남은 시간 표시
        }
    }

    private void UpdateTimerUI()
    {
        float remainingTime = GameManager.instance.GetRemainingArcadeTime();
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}