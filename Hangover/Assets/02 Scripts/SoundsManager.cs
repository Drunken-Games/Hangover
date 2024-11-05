using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager instance { get; private set; }

    private AudioSource bgmSource;
    private List<AudioSource> sfxSources;

    [SerializeField] private Dictionary<string, AudioClip> bgmClips;
    [SerializeField] private Dictionary<string, AudioClip> sfxClips;
    
    private const string VolumeKey = "AudioVolume"; // 저장할 키
    public AudioListener audioListener;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float masterVolume;

    private const int MAX_SFX_SOURCES = 5;
    private List<AudioClip> bgmList; // BGM 리스트
    private int currentBGMIndex = 0; // 현재 재생 중인 BGM 인덱스
    
    // 페이드 관련 설정
    private const float DEFAULT_FADE_DURATION = 0.1f;
    private Dictionary<AudioSource, Tween> activeFades = new Dictionary<AudioSource, Tween>();

    private void Awake()
    {
        masterVolume = AudioListener.volume;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundManager();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    public float GetMasterVolume()
    {
        return masterVolume * 100f; // 현재 BGM 볼륨 반환
    }
    
    public float GetBGMVolume()
    {
        return bgmVolume * 100f; // 현재 BGM 볼륨 반환
    }

    public float GetSFXVolume()
    {
        return sfxVolume * 100f; // 현재 SFX 볼륨 반환
    }


    private void InitializeSoundManager()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = false; // BGM이 끝나면 자동으로 루프하지 않도록 설정

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
        bgmList = new List<AudioClip>(bgmClips.Values); // BGM 클립 목록 초기화
        PlayNextBGM(); // 첫 번째 BGM 재생
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


    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        AudioListener.volume = masterVolume;
        bgmSource.volume = bgmVolume;
        foreach (AudioSource sfxSource in sfxSources)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void PlayNextBGM()
    {
        if (bgmList.Count > 0)
        {
            // 현재 인덱스에 해당하는 BGM 클립 재생
            bgmSource.clip = bgmList[currentBGMIndex];
            bgmSource.Play();
            Debug.Log($"BGM 재생 시작: {bgmList[currentBGMIndex].name}");

            // 인덱스를 증가시키고 리스트의 길이로 모듈로 연산
            currentBGMIndex = (currentBGMIndex + 1) % bgmList.Count;

            // BGM이 끝날 때 다음 BGM을 재생하는 코루틴 호출
            StartCoroutine(WaitForBGMToEnd());
        }
        else
        {
            Debug.LogWarning("BGM 목록이 비어 있습니다.");
        }
    }

    private System.Collections.IEnumerator WaitForBGMToEnd()
    {
        // 현재 BGM이 재생 중인지 확인
        yield return new WaitUntil(() => !bgmSource.isPlaying);
        PlayNextBGM(); // 다음 BGM 재생
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
                Debug.Log($"효과음 재생: {sfxName}");
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
    
    public void StopSFX(string sfxName)
    {
        if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
        {
            AudioSource sourceToStop = sfxSources.Find(source => source.clip == clip && source.isPlaying);
            if (sourceToStop != null)
            {
                sourceToStop.Stop();
                Debug.Log($"효과음 정지: {sfxName}");
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
                    .OnComplete(() => {
                        activeFades.Remove(availableSource);
                    });
                
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
                    .OnComplete(() => {
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
                    .OnComplete(() => {
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
}
