using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
//using DialogueLoader;
public class GameManager : MonoBehaviour
{

    // Singleton Instance
    public static GameManager instance { get; private set; }
    // 씬 매니저들
    private string previousSceneName = ""; // 이전 씬 이름을 저장할 변수
    public bool GameSceneNeedsProceed { get; set; } = false; // GameScene에서 진행 여부 플래그
    public bool DayResultProceed { get; set; } = false; // DayResultScene에서 진행 여부 플래그
    private DayResultManager dayResultManager;
    private BuildManager buildManager;
    public List<DialogueEntry> dialogues;

    //전역 관리 데이터
    public DayResultData dayResultData;

    public string playerName; // 유저 이름 저장 변수 추가
    public GameObject nameInputPanel;         // NameInput UI 오브젝트 참조

    // 대화 상태 저장 변수
    public int currentDialogueIndex; // 현재 대화 인덱스

    // 칵테일 파라미터

    public int[] cocktailParameters; // [0-4]:재료수량, [5]:제조방법

    // CocktailCheck를 위한 필드 선언
    private CocktailCheck cocktailCheck;

    // 이전에 찾은 칵테일 ID 목록
    [SerializeField]
    private List<int> foundCocktailIds = new List<int>();
    private int completedRecipeId = -1; // -1은 실패하거나 없을 경우

    private SaveSystem saveSystem;
    private int[] dialogueIndices = { 0, 43, 94, 138, 192 }; // 일차별 대사 인덱스

    // 엔딩 분기 배열
    public int endingTrigger;
    public int fireCount;
    public int robotCount;
    //

    private void Awake()
    {

        // Singleton 설정
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 5개 재료 + 1개 제조방법
        cocktailParameters = new int[6];

        // 대화 인덱스를 초기화 (이전에 저장된 상태 불러오기)
        currentDialogueIndex = PlayerPrefs.GetInt("SavedDialogueIndex", 1);

        // 대화 데이터 초기화(여기서 데이터를 로드하거나 초기화)
        //dialogues = DialogueLoader.LoadDialogues(Application.dataPath + "/04 Resources/Dialogues.csv");

        LoadDialogueDatabase();

        // 전역 관리 데이터 기본값 초기화
        dayResultData = new DayResultData();
        cocktailCheck = gameObject.AddComponent<CocktailCheck>();  // CocktailCheck 추가
        saveSystem = gameObject.AddComponent<SaveSystem>(); 
    }

    // DialogueDatabase.asset 파일에서 데이터를 로드하여 dialogues 리스트에 할당하는 메서드
    private void LoadDialogueDatabase()
    {
        DialogueDatabase database = Resources.Load<DialogueDatabase>("DialogueDataAssets/DialogueDatabase");

        if (database != null)
        {
            dialogues = new List<DialogueEntry>();

            foreach (var dialogueData in database.dialogues)
            {
                DialogueEntry dialogueEntry = new DialogueEntry(
                    dialogueData.Id,
                    dialogueData.Day,
                    dialogueData.Character,
                    dialogueData.Text,
                    dialogueData.CocktailId,
                    dialogueData.NextDialogueIds
                );
                dialogues.Add(dialogueEntry);
            }

            Debug.Log("DialogueDatabase에서 대화 데이터가 성공적으로 로드되었습니다.");
        }
        else
        {
            Debug.LogError("DialogueDatabase.asset 파일을 찾을 수 없습니다. 경로와 파일 이름을 확인해주세요.");
        }
    }


    // 유저 이름을 설정하는 메서드 추가
    public void SetPlayerName(string name)
    {
        playerName = name;
        Debug.Log($"Player name set to: {playerName}");
    }

    // 대화 인덱스 확인 후 이름 입력 패널 활성화
    public void CheckDialogueForNameInput()
    {
        if (currentDialogueIndex == 2 && nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);  // 특정 인덱스에 도달하면 패널 활성화
        }
    }


    // 씬 전환 전에 데이터 저장
    private void SaveCurrentSceneData()
    {
        if (buildManager != null)
        {
            cocktailParameters = buildManager.GetParameters();
            string ingredients = string.Join(", ", 
                new int[] { 
                    cocktailParameters[0], 
                    cocktailParameters[1], 
                    cocktailParameters[2], 
                    cocktailParameters[3], 
                    cocktailParameters[4] 
                });
            Debug.Log($"칵테일 데이터 저장 - 재료: [{ingredients}], 제조방법: {cocktailParameters[5]}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"OnSceneLoaded 호출됨 - 현재 씬: {scene.name}, 이전 씬: {previousSceneName}");
        // BuildScene에서 GameScene으로 이동한 경우 현재 칵테일 ID 출력
        if (previousSceneName == "CreaftingResultScene" && scene.name == "GameScene")
        {
            int currentRecipeId = GetRecipeId();
            Debug.Log($"BuildScene에서 GameScene으로 돌아왔습니다. 현재 칵테일 ID: {currentRecipeId}");
            
            // GameScene에서 대화 진행 플래그 설정
            GameSceneNeedsProceed = true;
        }
        //if(previousSceneName == "DayResultScene" && scene.name == "GameScene")
        //{
        //    DayResultProceed = true;
        //}
        previousSceneName = scene.name; // 현재 씬 이름을 이전 씬 이름으로 저장

        // 씬이 DayResult 씬인 경우에만 매니저 찾기
        if (scene.name == "DayResultScene")
        {
            dayResultManager = FindObjectOfType<DayResultManager>();
            
            if (dayResultManager != null)
            {
                Debug.Log("DayResultManager 참조 완료");
                InitializeDayResultScene();
            }
            else
            {
                Debug.LogWarning("DayResultManager를 찾을 수 없습니다!");
            }
        }


        if (scene.name == "BuildScene")
        {
            buildManager = FindObjectOfType<BuildManager>();

            if (buildManager != null)
            {
                Debug.Log("BuildManager 참조 완료");
                InitializeDayResultScene();
            }
            else
            {
                Debug.LogWarning("BuildManager 찾을 수 없습니다!");
            }
        }
    }

    private void InitializeDayResultScene()
    {
        // DayResult 씬 초기화 로직
        dayResultManager.dayResultData = dayResultData;

        //  테스트 더미
        /*
        dayResultManager.dayResultData = new DayResultData 
        {
            dayNum = 1,
            beforeMoney = 0,
            totalProfit = 82,
            tip = 11,
            refund = 16,
            materials = 49,
            netProfit = 2705,
            afterMoney = 2705,
            playerName = "봉균",
            branchIdx = 0 
        };
        // */

        Debug.Log(dayResultManager.dayResultData);
    }

    public DayResultManager GetDayResultManager()
    {
        return dayResultManager;
    }

    private void InitializeBuildScene()
    {
        // Build 씬 초기화 로직
    }
    
    public int[] GetCocktailParameters()
    {
        return cocktailParameters;
    }
    
    // 완성된 레시피 ID를 저장하는 메서드
    public void SetRecipeId(int id)
    {
        completedRecipeId = id;
        Debug.Log($"레시피 ID 저장됨: {id}");
        // completedRecipeId를 무조건 리스트에 추가
        foundCocktailIds.Add(completedRecipeId);
        Debug.Log($"레시피 ID {completedRecipeId}가 foundCocktailIds에 추가되었습니다.");
    }

// 저장된 레시피 ID를 가져오는 메서드
    public int GetRecipeId()
    {
        return completedRecipeId;
    }

// 레시피 ID 초기화 메서드
    public void ResetRecipeId()
    {
        completedRecipeId = -1;
    }
    
    public void PrepareForSceneChange()
    {
        SaveCurrentSceneData();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadSaveDataAndSetDialogueIndex()
    {
        // SaveData 불러오기
        SaveData saveData = saveSystem.LoadGame();

        if (saveData != null)
        {
            int dayNum = saveData.dayNum + 1;

            if (dayNum >= 0 && dayNum < dialogueIndices.Length)
            {
                currentDialogueIndex = dialogueIndices[dayNum];
                Debug.Log($"대사 인덱스 설정됨: Day {dayNum}, Index {currentDialogueIndex}");
            }
            else
            {
                Debug.LogWarning("잘못된 dayNum입니다.");
                currentDialogueIndex = 0; // 기본값으로 초기화
            }
        }
        else
        {
            Debug.LogWarning("저장 데이터를 찾을 수 없습니다.");
            currentDialogueIndex = 0; // 기본값으로 초기화
        }
    }
}