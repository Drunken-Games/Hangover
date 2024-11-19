using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingButtonController : MonoBehaviour
{
    // Singleton 인스턴스
    public static SettingButtonController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SettingButtonController Instance Initialized"); // 초기화 확인
        }
    }
    public void ExitGame()
    {
        Debug.Log("Exit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
        Application.Quit();
#elif UNITY_IOS
        // iOS에서는 앱 종료 기능이 필요하지 않음
#endif
    }

    public void ReturnToMainMenu()
    {
        
        GameManager.instance.NPC_ID = 0; // 아케이드 모드 NPC id
        GameManager.instance.Correct_ID = new List<int>(); // 아케이드 모드 주문 술
        GameManager.instance.isReactionPhase = false; // 아케이드 모드 단계
        GameManager.instance.ArcadeGold = 0; // 아케이드 모드 골드 
        // 타이머 초기화 및 정지
        GameManager.instance.StopArcadeTimer(); // 타이머 정지
        GameManager.instance.arcadeTimer = 0; // 타이머 초기화
        GameManager.instance.hasTimerEnded = false; // 타이머 종료 상태 확인 변수
        GameManager.instance.ArcadeStory = false;
        GameManager.instance.life = 3;
        GameManager.instance.DialoguesLog = new List<GameManager.DialogueLog>();
        SceneManager.LoadScene("MainMenuScene");
        
    }

}
