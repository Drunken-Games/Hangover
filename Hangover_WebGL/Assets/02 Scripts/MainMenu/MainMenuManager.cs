using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필요
using UnityEngine.UI; // UI 요소를 사용하기 위해 필요

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPrefab; // 띄울 팝업 prefab
    [SerializeField] private SceneController sceneController;
    private SaveSystem saveSystem; // SaveSystem 인스턴스
    private SaveData currentData; // 현재 게임 상태 데이터
    private GameObject currentPopup; // 현재 띄워진 팝업

    void Start()
    {
        // 저장 파일을 체크 (예시로 null로 초기화)
        saveSystem = gameObject.AddComponent<SaveSystem>(); // SaveSystem 인스턴스 초기화
        currentData = saveSystem.LoadGame(); // 저장된 데이터 불러오기

    }

    // 버튼 클릭 시 호출되는 메소드
    public void OnButtonClick()
    {
        // 저장 파일이 있을 경우
        if (currentData != null)
        {
            ShowPopup(); // 팝업 띄우기
        }
        else
        {
            sceneController.LoadSceneByName("GameScene");
        }
    }
    
    
    public void OnButtonClick2()
    {
        GameManager.instance.ArcadeStory = true;
        Debug.Log(GameManager.instance.ArcadeStory);
        sceneController.LoadSceneByName("ArcadeScene");
    }
    
    
    

    public void ClosePopup() 
    {
        Object popup = GameObject.Find("StoryModePopup(Clone)");
        // 이미 팝업이 열려 있다면 이전 팝업을 삭제
        if (popup != null)
        {
            Destroy(popup);
        }
    }

    // 팝업을 띄우는 메소드
    // 버튼 클릭 시 호출되는 메소드
    private void ShowPopup()
    {
        // 이미 팝업이 열려 있다면 이전 팝업을 삭제
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }

        // 팝업 prefab을 인스턴스화
        currentPopup = Instantiate(popupPrefab, Vector3.zero, Quaternion.identity);

        // Canvas의 자식으로 설정
        currentPopup.transform.SetParent(GameObject.Find("MainMenu").transform, false);

        // 중앙에 배치하기
        RectTransform rectTransform = currentPopup.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero; // 중앙 위치로 설정
    }


    // 게임 씬으로 이동하는 메소드
    public void LoadGameScene()
    {
        // GameScene이라는 이름의 씬으로 이동
        GameManager.instance.LoadSaveDataAndSetDialogueIndex();
        sceneController.LoadSceneByName("GameScene");
    }

    
    
    
}

