using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SoundsManager : MonoBehaviour
{
    // 토글 키 등록
    public Toggle BGMVolumeToggle;
    public Toggle SFXVolumeToggle;
    
    // 싱글톤 인스턴스
    public static SoundsManager instance { get; private set; }

    // 오디오 소스 컴포넌트들
    private AudioSource bgmSource;
    private List<AudioSource> sfxSources;
    
    // 오디오 클립들을 저장할 딕셔너리
    [SerializeField] private Dictionary<string, AudioClip> bgmClips;
    [SerializeField] private Dictionary<string, AudioClip> sfxClips;

    // 볼륨 설정
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    private const int MAX_SFX_SOURCES = 5;

    private void Awake()
    {
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

    private void InitializeSoundManager()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

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

        // 각 토글에 이벤트 리스너 등록
        BGMVolumeToggle.onValueChanged.AddListener(OnBGMToggleChanged);
        SFXVolumeToggle.onValueChanged.AddListener(OnSFXToggleChanged);

        // 초기 토글 상태 반영
        OnBGMToggleChanged(BGMVolumeToggle.isOn);
        OnSFXToggleChanged(SFXVolumeToggle.isOn);

        Debug.Log("SoundsManager 초기화 완료");
    }

    private void LoadAudioClips()
    {
        AudioClip[] loadedBGMs = Resources.LoadAll<AudioClip>("Sounds/BGM");
        foreach (AudioClip clip in loadedBGMs)
        {
            bgmClips.Add(clip.name, clip);
            Debug.Log($"BGM 로드됨: {clip.name}");
        }

        AudioClip[] loadedSFXs = Resources.LoadAll<AudioClip>("Sounds/SFX");
        foreach (AudioClip clip in loadedSFXs)
        {
            sfxClips.Add(clip.name, clip);
            Debug.Log($"SFX 로드됨: {clip.name}");
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        Debug.Log($"BGM Volume 설정: {bgmVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        Debug.Log($"SFX Volume 설정: {sfxVolume}");
    }

    private void UpdateVolumes()
    {
        bgmSource.volume = bgmVolume;
        foreach (AudioSource sfxSource in sfxSources)
        {
            sfxSource.volume = sfxVolume;
        }
        Debug.Log("볼륨 업데이트 완료");
    }

    public void PlayBGM(string bgmName)
    {
        if (bgmClips.TryGetValue(bgmName, out AudioClip clip))
        {
            bgmSource.clip = clip;
            bgmSource.Play();
            Debug.Log($"BGM 재생 시작: {bgmName}");
        }
        else
        {
            Debug.LogWarning($"BGM을 찾을 수 없음: {bgmName}");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        Debug.Log("BGM 정지");
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

    public void StopAllSFX()
    {
        foreach (AudioSource sfxSource in sfxSources)
        {
            sfxSource.Stop();
        }
        Debug.Log("모든 효과음 정지");
    }

    public void StopAllSounds()
    {
        StopBGM();
        StopAllSFX();
        Debug.Log("모든 사운드 정지");
    }

    private void OnBGMToggleChanged(bool isOn)
    {
        if (isOn)
        {
            PlayBGM("배경음악_클립_이름");  // 실제 BGM 클립 이름으로 대체 필요
            Debug.Log("BGM Toggle On - 배경음악 재생 시작");
        }
        else
        {
            StopBGM();
            Debug.Log("BGM Toggle Off - 배경음악 정지");
        }
    }

    private void OnSFXToggleChanged(bool isOn)
    {
        if (isOn)
        {
            SetSFXVolume(1f);
            Debug.Log("SFX Toggle On - 효과음 볼륨 활성화");
        }
        else
        {
            SetSFXVolume(0f);
            Debug.Log("SFX Toggle Off - 효과음 볼륨 비활성화");
        }
    }
}
