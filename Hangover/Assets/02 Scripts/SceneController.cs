using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private string previousSceneName = "";

    private void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
        // 현재 씬에 맞는 BGM을 초기화
        OnSceneChanged(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene previousScene, Scene newScene)
    {
        string newSceneName = newScene.name;

        // IntroScene이나 MainMenuScene으로 전환될 때만 테마곡 변경
        if (newSceneName.Equals("MainMenuScene") || (newSceneName.Equals("GameScene") && !previousSceneName.Equals("CreaftingResultScene") ))
        {
            if (SoundsManager.instance != null)
            {
                SoundsManager.instance.PlayNextBGM(newSceneName);
                Debug.Log(previousSceneName);
            }
        }
        // 이전 씬이 IntroScene 또는 MainMenuScene이었고, 현재 씬이 다른 씬으로 전환된 경우에도 테마곡 유지
        else if (previousSceneName.Equals("IntroScene") || previousSceneName.Equals("MainMenuScene") || previousSceneName.Equals("CreaftingResultScene"))
        {
            if (SoundsManager.instance != null)
            {
                SoundsManager.instance.PlayNextBGM("GameScene");
            }
        }

        // 이전 씬 이름 갱신
        previousSceneName = newSceneName;
    }
    
    // 다음 씬으로 전환
    public void LoadNextScene()
    {
        // GameManager 생성 또는 찾기
        GameManager gameManager = FindObjectOfType<GameManager>();
        
        // GameManager 생성 로직 추가
        if (FindObjectOfType<GameManager>() == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManager = gameManagerObj.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObj);
        }
        
        // 씬 전환 전에 데이터 저장
        gameManager.PrepareForSceneChange();

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    // 특정 씬 이름으로 전환
    public void LoadSceneByName(string sceneName)
    {
        if(GameManager.instance.ArcadeStory == true && sceneName == "GameScene")
        {
            sceneName = "ArcadeScene";
        }
        // GameManager 생성 또는 찾기
        GameManager gameManager = FindObjectOfType<GameManager>();
        
        // GameManager 생성 로직 추가
        if (FindObjectOfType<GameManager>() == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManager = gameManagerObj.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObj);
        }
        
        // 씬 전환 전에 데이터 저장
        gameManager.PrepareForSceneChange();
        
        SceneManager.LoadScene(sceneName);
    }

    // 특정 씬 인덱스로 전환
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // 씬 비동기 로드
    public async void LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // 로딩이 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            float progress = asyncLoad.progress;
            Debug.Log($"Loading progress: {progress * 100}%");
            await System.Threading.Tasks.Task.Yield();
        }
    }

    // 첫 번째 씬으로 돌아가기
    public void ReturnToFirstScene()
    {
        SceneManager.LoadScene(0);
    }

    // 씬 리로드
    public void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}