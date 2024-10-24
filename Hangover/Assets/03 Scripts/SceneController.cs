using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // 다음 씬으로 전환
    public void LoadNextScene()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
    }

    // 특정 씬 이름으로 전환
    public void LoadSceneByName(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    // 특정 씬 인덱스로 전환
    public void LoadSceneByIndex(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    // 씬 비동기 로드
    public async void LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // 씬 리로드
    public void ReloadCurrentScene()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
    }
}