using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager instance { get; private set; }

    private AudioSource bgmSource;
    private List<AudioSource> sfxSources;

    [SerializeField] private Dictionary<string, AudioClip> bgmClips;
    [SerializeField] private Dictionary<string, AudioClip> sfxClips;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float masterVolume = 1f;

    private bool isMasterMuted = false;
    private bool isBGMMuted = false;
    private bool isSFXMuted = false;

    private float lastMasterVolume = 1f;
    private float lastBGMVolume = 1f;
    private float lastSFXVolume = 1f;

    private const int MAX_SFX_SOURCES = 5;
    private List<AudioClip> bgmList;
    public int currentBGMIndex = 0;
    private string previousSceneName = "IntroScene";

    private const float DEFAULT_FADE_DURATION = 0.1f;
    private Dictionary<AudioSource, Tween> activeFades = new Dictionary<AudioSource, Tween>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 기본값 설정
            masterVolume = 1f;
            bgmVolume = 1f;
            sfxVolume = 1f;
            lastMasterVolume = 1f;
            lastBGMVolume = 1f;
            lastSFXVolume = 1f;

            // SoundSaveSystem 컴포넌트 추가
            if (GetComponent<SoundSaveSystem>() == null)
            {
                gameObject.AddComponent<SoundSaveSystem>();
            }

            InitializeSoundManager();

            // 설정 로드
            SoundSaveSystem.LoadSoundSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSoundManager()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = false;

        sfxSources = new List<AudioSource>();
        for (int i = 0; i < MAX_SFX_SOURCES; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSources.Add(sfxSource);
        }

        bgmClips = new Dictionary<string, AudioClip>();
        sfxClips = new Dictionary<string, AudioClip>();

        LoadAudioClips();
        bgmList = new List<AudioClip>(bgmClips.Values);
        PlayNextBGM(previousSceneName);
    }

    private void LoadAudioClips()
    {
        AudioClip[] loadedBGMs = Resources.LoadAll<AudioClip>("Audios/BGM");
        foreach (AudioClip clip in loadedBGMs)
        {
            bgmClips.Add(clip.name, clip);
        }

        AudioClip[] loadedSFXs = Resources.LoadAll<AudioClip>("Audios/SFX");
        foreach (AudioClip clip in loadedSFXs)
        {
            sfxClips.Add(clip.name, clip);
        }
    }

    // Volume Getters
    public float GetMasterVolume() => masterVolume * 100f;
    public float GetBGMVolume() => bgmVolume * 100f;
    public float GetSFXVolume() => sfxVolume * 100f;
    public float GetLastMasterVolume() => lastMasterVolume * 100f;
    public float GetLastBGMVolume() => lastBGMVolume * 100f;
    public float GetLastSFXVolume() => lastSFXVolume * 100f;

    // Mute State Getters
    public bool IsMasterMuted() => isMasterMuted;
    public bool IsBGMMuted() => isBGMMuted;
    public bool IsSFXMuted() => isSFXMuted;

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        masterVolume = volume;
        lastMasterVolume = volume;
        UpdateVolumes();
        SoundSaveSystem.SaveSoundSettings();
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        bgmVolume = volume;
        lastBGMVolume = volume;
        UpdateVolumes();
        SoundSaveSystem.SaveSoundSettings();
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        sfxVolume = volume;
        lastSFXVolume = volume;
        UpdateVolumes();
        SoundSaveSystem.SaveSoundSettings();
    }

    public void SetMasterMute(bool mute)
    {
        isMasterMuted = mute;
        UpdateVolumes();
        SoundSaveSystem.SaveSoundSettings();
    }

    public void SetBGMMute(bool mute)
    {
        isBGMMuted = mute;
        UpdateVolumes();
        SoundSaveSystem.SaveSoundSettings();
    }

    public void SetSFXMute(bool mute)
    {
        isSFXMuted = mute;
        UpdateVolumes();
        SoundSaveSystem.SaveSoundSettings();
    }

    private void UpdateVolumes()
    {
        AudioListener.volume = isMasterMuted ? 0f : masterVolume;

        if (bgmSource != null)
        {
            bgmSource.volume = (isMasterMuted || isBGMMuted) ? 0f : bgmVolume;
        }

        foreach (AudioSource sfxSource in sfxSources)
        {
            sfxSource.volume = (isMasterMuted || isSFXMuted) ? 0f : sfxVolume;
        }
    }

    public void PlayNextBGM(string sceneName)
    {
        if (bgmList.Count > 0)
        {
            if (sceneName == "IntroScene")
            {
                currentBGMIndex = 0;
            }
            else if (sceneName == "MainMenuScene")
            {
                currentBGMIndex = 1;
            }
            else
            {
                if (currentBGMIndex < 2)
                {
                    currentBGMIndex = 2;
                }
            }

            bgmSource.clip = bgmList[currentBGMIndex];
            if (bgmSource.isPlaying == false)
            {
                bgmSource.Play();
            }

            Debug.Log($"BGM 재생 시작: {bgmList[currentBGMIndex].name}");

            if (currentBGMIndex >= 2)
            {
                currentBGMIndex = UnityEngine.Random.Range(2, bgmList.Count);
            }
            else
            {
                currentBGMIndex = (currentBGMIndex + 1) % bgmList.Count;
            }

            previousSceneName = sceneName;

            StartCoroutine(WaitForBGMToEnd());
        }
    }

    private System.Collections.IEnumerator WaitForBGMToEnd()
    {
        yield return new WaitUntil(() => !bgmSource.isPlaying);
        PlayNextBGM(previousSceneName);
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string sfxName)
    {
        if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
        {
            AudioSource availableSource = sfxSources.Find(source => !source.isPlaying);
            if (availableSource != null)
            {
                availableSource.clip = clip;
                availableSource.Play();
            }
        }
    }

    public void StopAllSFX()
    {
        foreach (AudioSource sfxSource in sfxSources)
        {
            sfxSource.Stop();
        }
    }

    public void PlaySFXWithFade(string sfxName, float fadeInDuration = DEFAULT_FADE_DURATION)
    {
        if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
        {
            AudioSource availableSource = sfxSources.Find(source => !source.isPlaying);
            if (availableSource != null)
            {
                // 기존 페이드가 있다면 제거
                if (activeFades.ContainsKey(availableSource))
                {
                    activeFades[availableSource].Kill();
                    activeFades.Remove(availableSource);
                }

                availableSource.clip = clip;
                availableSource.volume = 0f;
                availableSource.Play();

                // 페이드 인 시작
                Tween fadeTween = availableSource.DOFade(sfxVolume, fadeInDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => { activeFades.Remove(availableSource); });

                activeFades.Add(availableSource, fadeTween);
                Debug.Log($"효과음 페이드 인 시작: {sfxName}");
            }
            else
            {
                Debug.LogWarning("모든 효과음 채널이 사용 중입니다!");
            }
        }
        else
        {
            Debug.LogWarning($"효과음을 찾을 수 없음: {sfxName}");
        }
    }

    public void StopSFXWithFade(string sfxName, float fadeOutDuration = DEFAULT_FADE_DURATION)
    {
        if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
        {
            AudioSource sourceToStop = sfxSources.Find(source => source.clip == clip && source.isPlaying);
            if (sourceToStop != null)
            {
                // 기존 페이드가 있다면 제거
                if (activeFades.ContainsKey(sourceToStop))
                {
                    activeFades[sourceToStop].Kill();
                    activeFades.Remove(sourceToStop);
                }

                // 페이드 아웃 시작
                Tween fadeTween = sourceToStop.DOFade(0f, fadeOutDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        sourceToStop.Stop();
                        sourceToStop.volume = sfxVolume; // 볼륨 초기화
                        activeFades.Remove(sourceToStop);
                    });

                activeFades.Add(sourceToStop, fadeTween);
                Debug.Log($"효과음 페이드 아웃 시작: {sfxName}");
            }
            else
            {
                Debug.LogWarning($"정지할 수 있는 효과음이 없습니다: {sfxName}");
            }
        }
        else
        {
            Debug.LogWarning($"효과음을 찾을 수 없음: {sfxName}");
        }
    }

    public void StopAllSFXWithFade(float fadeOutDuration = DEFAULT_FADE_DURATION)
    {
        foreach (AudioSource sfxSource in sfxSources)
        {
            if (sfxSource.isPlaying)
            {
                // 기존 페이드가 있다면 제거
                if (activeFades.ContainsKey(sfxSource))
                {
                    activeFades[sfxSource].Kill();
                    activeFades.Remove(sfxSource);
                }

                // 페이드 아웃 시작
                Tween fadeTween = sfxSource.DOFade(0f, fadeOutDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        sfxSource.Stop();
                        sfxSource.volume = sfxVolume; // 볼륨 초기화
                        activeFades.Remove(sfxSource);
                    });

                activeFades.Add(sfxSource, fadeTween);
            }
        }
    }

    private void OnDestroy()
    {
        // 모든 활성 페이드 제거
        foreach (var tween in activeFades.Values)
        {
            tween.Kill();
        }

        activeFades.Clear();
    }

// 앱 종료시 설정 저장
    private void OnApplicationQuit()
    {
        SoundSaveSystem.SaveSoundSettings();
    }

// 앱 일시정지시 설정 저장 (모바일 환경 대응)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SoundSaveSystem.SaveSoundSettings();
        }
    }
}