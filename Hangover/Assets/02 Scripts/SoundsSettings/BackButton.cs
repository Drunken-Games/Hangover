using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    // 하위 자식 중 "Canvas" 이름을 가진 게임 오브젝트 참조
    private GameObject exitCanvas;
    
    [SerializeField]
    private Button MenuButton;
    [SerializeField]
    private Button GameCloseButton;
    [SerializeField]
    private Button CloseButton;
    [SerializeField]
    private GameObject Backpopup;

    void Start()
    {
        // 현재 오브젝트의 자식 중 "Canvas"라는 이름을 가진 오브젝트를 찾음
        exitCanvas = transform.Find("Canvas")?.gameObject;

        // 시작 시 캔버스를 비활성화하여 기본 상태를 설정
        if (exitCanvas != null)
        {
            exitCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Canvas 오브젝트를 찾을 수 없습니다.");
        }
        
        // SettingButtonController 인스턴스가 존재하는지 확인합니다.
        if (SettingButtonController.Instance == null)
        {
            GameObject settingControllerObject = new GameObject("SettingButtonController");
            settingControllerObject.AddComponent<SettingButtonController>();
            Debug.Log("SettingButtonController가 생성되었습니다.");
        }

        // 버튼 클릭 시 각기 다른 동작을 수행하도록 리스너 추가
        if (MenuButton != null)
        {
            MenuButton.onClick.AddListener(OnMenuButtonClick);
        }

        if (GameCloseButton != null)
        {
            GameCloseButton.onClick.AddListener(OnGameCloseButtonClick);
        }

        if (CloseButton != null)
        {
            CloseButton.onClick.AddListener(OnCloseButtonClick);
        }
    }
    
    private void Update()
    {
        // Android의 뒤로 가기 버튼을 누를 때 종료 캔버스를 활성화
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowExitPanel();
        }
    }

    // 종료 패널을 표시하는 메서드
    public void ShowExitPanel()
    {
        if (exitCanvas != null)
        {
            exitCanvas.SetActive(true);
        }
    }

    public void OnMenuButtonClick()
    {
        Debug.Log("Menu Button Clicked");
        SettingButtonController.Instance?.ReturnToMainMenu();
    }

    public void OnGameCloseButtonClick()
    {
        Debug.Log("Game Close Button Clicked");
        SettingButtonController.Instance?.ExitGame();
    }

    public void OnCloseButtonClick()
    {
        Debug.Log("Close Button Clicked");
        Backpopup?.SetActive(false);
    }
}
