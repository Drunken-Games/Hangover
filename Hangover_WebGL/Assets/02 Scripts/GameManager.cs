// using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
//using DialogueLoader;
public class GameManager : MonoBehaviour
{

    // Singleton Instance
    public static GameManager instance { get; private set; }
    
    
    #region 아케이드 모드용 전역 변수
    // New global variables for Arcade Mode
    public int NPC_ID = -1; // 아케이드 모드 NPC id
    public List<int> Correct_ID = new List<int>(); // 아케이드 모드 주문 술
    public bool isReactionPhase = false; // 아케이드 모드 단계
    public int ArcadeGold = 0; // 아케이드 모드 골드
    public float arcadeTimer; // 아케이드 모드 타이머
    public bool isArcadeTimerRunning; // 아케이드 모드 타이머 실행 여부
    public bool hasTimerEnded = false; // 타이머 종료 상태 확인 변수
    public int life = 3;
    
    // 타이머 시작 메서드
    public void StartArcadeTimer(float time)
    {
        arcadeTimer = time;
        isArcadeTimerRunning = true;
    }

    // 타이머 정지 메서드
    public void StopArcadeTimer()
    {
        isArcadeTimerRunning = false;
    }

    // 남은 타이머 시간 가져오기
    public float GetRemainingArcadeTime()
    {
        return arcadeTimer;
    }

    // Update 메서드에서 타이머 관리
        private void Update()
        {
            if (isArcadeTimerRunning && arcadeTimer > 0)
            {
                arcadeTimer -= Time.deltaTime;
                if (arcadeTimer <= 0)
                {
                    arcadeTimer = 0;
                    isArcadeTimerRunning = false;
                    hasTimerEnded = true; // 타이머 종료 상태 설정
                    Debug.Log("Arcade Timer has ended.");
                }
            }
        }

// 타이머가 종료되었는지 여부를 확인하는 메서드
    public bool HasArcadeTimerEnded()
    {
        return hasTimerEnded;
    }
    
    #endregion
    
    // 씬 매니저들
    public string previousSceneName = ""; // 이전 씬 이름을 저장할 변수
    public bool GameSceneNeedsProceed { get; set; } = false; // GameScene에서 진행 여부 플래그
    public bool DayResultProceed { get; set; } = false; // DayResultScene에서 진행 여부 플래그
    private DayResultManager dayResultManager;
    private BuildManager buildManager;
    public List<DialogueEntry> dialogues;

    //전역 관리 데이터
    public DayResultData dayResultData;

    //public string playerName; // 유저 이름 저장 변수 추가
    public GameObject nameInputPanel; // NameInput UI 오브젝트 참조

    // 대화 상태 저장 변수
    public int currentDialogueIndex; // 현재 대화 인덱스

    // 칵테일 파라미터

    public int[] cocktailParameters; // [0-4]:재료수량, [5]:제조방법

    // CocktailCheck를 위한 필드 선언
    private CocktailCheck cocktailCheck;

    // 이전에 찾은 칵테일 ID 목록
    public List<int> foundCocktailIds = new List<int>();
    public int completedRecipeId = -1; // -1은 실패하거나 없을 경우

    private SaveSystem saveSystem;
    public int[] dialogueIndices = { 0, 42, 94, 139, 193 }; // 일차별 대사 인덱스

    // 엔딩 분기 배열
    //public int endingTrigger;
    //public int fireCount;
    //public int robotCount;
    //public int branchIdx;
    public int endingNumber;
    //
    // 특수 분기를 처리할 임시 변수
    public int tempSpecialBranch = -1; // null로 초기화, 필요 시 설정

    // 아케이드 스토리모드 여부
    public bool ArcadeStory = false;


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
        currentDialogueIndex = PlayerPrefs.GetInt("SavedDialogueIndex", 0);

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
                    dialogueData.CocktailIds,
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
    // 특수 분기를 처리하는 메서드
    public void ProcessSpecialBranch()
    {
        if (tempSpecialBranch != -1) // -1이 아닐 때만 분기 처리
        {
            // 특수 분기 처리 로직
            int branchId = tempSpecialBranch;

            if (branchId == 1)
            {
                SceneManager.LoadScene("SpecialScene1");
            }
            else if (branchId == 2)
            {
                SceneManager.LoadScene("SpecialScene2");
            }

            // 특수 분기 처리가 끝나면 변수 초기화
            ClearSpecialBranch();
        }
        else
        {
            Debug.Log("특수 분기가 설정되지 않았습니다.");
        }
    }

    // 특수 분기 초기화 메서드
    public void ClearSpecialBranch()
    {
        tempSpecialBranch = -1;
        Debug.Log("특수 분기 변수가 초기화되었습니다.");
    }


    // 유저 이름을 설정하는 메서드 추가
    public void SetPlayerName(string name)
    {
        dayResultData.playerName = name;
        Debug.Log($"Player name set to: {dayResultData.playerName}");
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
        if (scene.name == "GameScene")
        {
            ProcessSpecialBranch();
            if (previousSceneName == "CreaftingResultScene")
            {
                int currentRecipeId = GetRecipeId();
                Debug.Log($"BuildScene에서 GameScene으로 돌아왔습니다. 현재 칵테일 ID: {currentRecipeId}");
                GameSceneNeedsProceed = true;
            }
        }
        if (previousSceneName == "DayResultScene" && scene.name == "GameScene")
        {
            DayResultProceed = true;
            Debug.Log("BYE");
        }
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
            endingTrigger = 0 
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
            int beforeMoney = saveData.beforeMoney;
            int totalProfit = saveData.totalProfit;
            int tip = saveData.tip;
            int refund = saveData.refund;
            int materials = saveData.materials;
            int netProfit = saveData.netProfit;
            int afterMoney = saveData.afterMoney;

            dayResultData.playerName = saveData.playerName;
            int endingTrigger = saveData.endingTrigger;
            int fireCount = saveData.fireCount;
            int robotCount = saveData.robotCount;


            int dayNumIdx = dayNum - 1;

            if (dayNumIdx >= 0 && dayNumIdx < dialogueIndices.Length)
            {
                currentDialogueIndex = dialogueIndices[dayNumIdx];
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
            dayResultData.dayNum = 0;
            dayResultData.beforeMoney = 0;
            dayResultData.totalProfit = 0;
            dayResultData.tip = 0;
            dayResultData.refund = 0;
            dayResultData.materials = 0;
            dayResultData.netProfit = 0;
            dayResultData.afterMoney = 0;

            dayResultData.playerName = null;
            dayResultData.endingTrigger = 0;
            dayResultData.fireCount = 0;
            dayResultData.robotCount = 0;


            int dayNumIdx = 0;
            Debug.LogWarning("저장 데이터를 찾을 수 없습니다.");
            currentDialogueIndex = 0; // 기본값으로 초기화
        }
    }
}