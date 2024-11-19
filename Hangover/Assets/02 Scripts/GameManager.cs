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
    private DayResultManager dayResultManager;
    private BuildManager buildManager;
    public List<DialogueEntry> dialogues;
    
    // 칵테일 파라미터
    private int[] cocktailParameters; // [0-4]:재료수량, [5]:제조방법
    
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

        // 대화 데이터 초기화(여기서 데이터를 로드하거나 초기화)
        dialogues = DialogueLoader.LoadDialogues(Application.dataPath + "/04 Resources/Dialogues.csv");
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
    
    public void PrepareForSceneChange()
    {
        SaveCurrentSceneData();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}