using UnityEngine;
using DG.Tweening;
using TMPro;

public class BlendingManager : MonoBehaviour
{
    [Header("블렌더 설정")]
    [SerializeField] private BlenderParticleSystem[] blenderParticleSystems;
    
    [Header("씬 전환 설정")]
    [SerializeField] private float requiredBlendTime = 1.5f;
    [SerializeField] private string nextSceneName = "CreaftingResultScene";
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("효과음 설정")]
    [SerializeField] private string blendingSoundName = "blending";
    [SerializeField] private float soundFadeInDuration = 0.2f;
    [SerializeField] private float soundFadeOutDuration = 0.3f;
    [SerializeField] private float sceneTransitionSoundFadeTime = 0.5f; // 씬 전환 시 페이드아웃 시간
    [SerializeField] private float soundFadeStartOffset = 0.2f; // 씬 전환 전 페이드아웃 시작 오프셋
    
    [Header("진동 설정")]
    [SerializeField] private bool useVibration = true;
    [SerializeField] private float vibrationInterval = 0.1f;
    
    [Header("UI 설정")]
    [SerializeField] private TextMeshProUGUI uiTextMeshProObject; 
    
    
    private bool isPlaying = false;
    private bool isPlayingSound = false;
    private bool isFadingOut = false;
    private float currentBlendTime = 0f;
    private float lastEffectTime;
    private SceneController sceneController;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        InitializeSceneController();
        lastEffectTime = Time.time;
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
            UpdateEffects();
            UpdateTextTransparency();
        }
        else
        {
            ResetBlendProgress();
            StopEffects();
        }

        if (showDebugInfo && isPlaying)
        {
            Debug.Log($"블렌딩 진행도: {currentBlendTime:F2}/{requiredBlendTime:F2}");
        }
    }

    private void UpdateEffects()
    {
        // 진동 효과 업데이트
        if (useVibration && Time.time - lastEffectTime > vibrationInterval)
        {
            TriggerVibration();
            lastEffectTime = Time.time;
        }

        // 사운드 효과 업데이트
        if (!isPlayingSound && !isFadingOut)
        {
            PlayBlendingSound();
        }
        
        // 씬 전환 전 페이드아웃 시작 체크
        float remainingTime = requiredBlendTime - currentBlendTime;
        if (!isFadingOut && remainingTime <= sceneTransitionSoundFadeTime + soundFadeStartOffset)
        {
            StartTransitionFade();
        }
    }

    private void StartTransitionFade()
    {
        if (isPlayingSound && !isFadingOut)
        {
            isFadingOut = true;
            if (SoundsManager.instance != null)
            {
                SoundsManager.instance.StopSFXWithFade(blendingSoundName, sceneTransitionSoundFadeTime);
            }
            isPlayingSound = false;
        }
    }

    private void StopEffects()
    {
        if (isPlayingSound)
        {
            StopBlendingSound();
        }
    }

    public void ToggleBlending()
    {
        isPlaying = !isPlaying;
        isFadingOut = false;
        
        foreach (BlenderParticleSystem blenderParticleSystem in blenderParticleSystems)
        {
            blenderParticleSystem.ToggleBlending();
        }

        // 블렌딩 시작/정지에 따른 사운드 처리
        if (isPlaying)
        {
            PlayBlendingSound();
        }
        else
        {
            StopBlendingSound();
        }
    }

    private void PlayBlendingSound()
    {
        if (!isPlayingSound && !isFadingOut)
        {
            if (SoundsManager.instance != null)
            {
                SoundsManager.instance.PlaySFXWithFade(blendingSoundName, soundFadeInDuration);
                isPlayingSound = true;
            }
            else
            {
                Debug.LogWarning("SoundsManager 인스턴스를 찾을 수 없습니다!");
            }
        }
    }

    private void StopBlendingSound()
    {
        if (isPlayingSound)
        {
            if (SoundsManager.instance != null)
            {
                SoundsManager.instance.StopSFXWithFade(blendingSoundName, soundFadeOutDuration);
            }
            isPlayingSound = false;
        }
    }

    private void TriggerVibration()
    {
        #if UNITY_IOS
        // iOS용 햅틱 패턴
        // HapticPatterns.GenerateHapticPattern(HapticPatterns.PresetType.MediumImpact);
        #elif UNITY_ANDROID
        Handheld.Vibrate();
        #endif
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
        isFadingOut = false;
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
    
    private void UpdateTextTransparency()
    {
        // Check if uiTextMeshProObject is assigned
        if (uiTextMeshProObject == null)
        {
            Debug.LogWarning("TextMeshPro - Text (UI) object is not assigned!");
            return;
        }

        // Get the current color
        Color color = uiTextMeshProObject.color;

        // Set alpha to 0 if either isBeingTouched or isShaking is true, otherwise set it to full opacity
        color.a = (isPlaying) ? 0 : 1;

        // Apply the updated color back to the TextMeshPro object
        uiTextMeshProObject.color = color;
    }

    private void OnDisable()
    {
        StopBlendingSound();
    }

    private void OnDestroy()
    {
        StopBlendingSound();
    }
}