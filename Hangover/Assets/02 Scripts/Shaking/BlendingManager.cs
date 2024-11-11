using UnityEngine;

public class BlendingManager : MonoBehaviour
{
    [Header("블렌더 설정")]
    [SerializeField] private BlenderParticleSystem[] blenderParticleSystems;
    
    [Header("씬 전환 설정")]
    [SerializeField] private float requiredBlendTime = 1.5f;
    [SerializeField] private string nextSceneName = "CreaftingResultScene";
    [SerializeField] private bool showDebugInfo = true;
    
    private bool isPlaying = false;
    private float currentBlendTime = 0f;
    private SceneController sceneController;

    private void Start()
    {
        InitializeSceneController();
    }

    private void InitializeSceneController()
    {
        sceneController = FindObjectOfType<SceneController>();
        if (sceneController == null)
        {
            Debug.LogWarning("SceneController를 찾을 수 없습니다! 새로 추가합니다...");
            GameObject controllerObject = new GameObject("SceneController");
            sceneController = controllerObject.AddComponent<SceneController>();
        }
    }

    private void Update()
    {
        if (isPlaying)
        {
            UpdateBlendProgress();
        }
        else
        {
            ResetBlendProgress();
        }

        if (showDebugInfo && isPlaying)
        {
            Debug.Log($"블렌딩 진행도: {currentBlendTime:F2}/{requiredBlendTime:F2}");
        }
    }

    public void ToggleBlending()
    {
        isPlaying = !isPlaying;
        
        foreach (BlenderParticleSystem blenderParticleSystem in blenderParticleSystems)
        {
            blenderParticleSystem.ToggleBlending();
        }
    }

    private void UpdateBlendProgress()
    {
        currentBlendTime += Time.deltaTime;
        
        if (currentBlendTime >= requiredBlendTime)
        {
            LoadNextScene();
        }
    }

    private void ResetBlendProgress()
    {
        currentBlendTime = 0f;
    }

    private void LoadNextScene()
    {
        if (sceneController != null)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                sceneController.LoadSceneByName(nextSceneName);
            }
            else
            {
                sceneController.LoadNextScene();
            }
        }
    }
}