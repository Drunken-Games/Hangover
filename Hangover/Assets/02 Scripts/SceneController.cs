using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
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