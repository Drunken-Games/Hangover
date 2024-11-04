using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Master Control")]
    public Toggle masterVolumeToggle;
    public Slider masterVolumeSlider;
    
    [Header("BGM Control")]
    public Toggle bgmToggle; // BGM 토글
    public Slider bgmSlider; // BGM 볼륨 슬라이더

    [Header("SFX Control")]
    public Toggle sfxToggle; // SFX 토글
    public Slider sfxSlider; // SFX 볼륨 슬라이더

    private void Start()
    {
        // master 토글 초기화
        masterVolumeToggle.isOn = SoundsManager.instance != null; // SoundsManager가 존재하면 토글을 켜짐으로 설정
        masterVolumeToggle.onValueChanged.AddListener(OnMasterToggleChanged); // 토글 상태 변경 이벤트 연결
        
        // 슬라이더의 값 변경 시 OnMasterVolumeChanged 메서드 호출
        masterVolumeSlider.value = SoundsManager.instance != null ? SoundsManager.instance.GetMasterVolume() : 1f;    
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        // BGM 토글 초기화
        bgmToggle.isOn = SoundsManager.instance != null; // SoundsManager가 존재하면 토글을 켜짐으로 설정
        bgmToggle.onValueChanged.AddListener(OnBGMToggleChanged); // 토글 상태 변경 이벤트 연결
        
        // BGM 슬라이더 초기화
        bgmSlider.value = SoundsManager.instance != null ? SoundsManager.instance.GetBGMVolume() : 1f; // 초기 볼륨 설정
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged); // 슬라이더 값 변경 이벤트 연결
        
        // SFX 토글 초기화
        sfxToggle.isOn = SoundsManager.instance != null; // SoundsManager가 존재하면 토글을 켜짐으로 설정
        sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged); // 토글 상태 변경 이벤트 연결
        
        // SFX 슬라이더 초기화
        sfxSlider.value = SoundsManager.instance != null ? SoundsManager.instance.GetSFXVolume() : 1f; // 초기 볼륨 설정
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged); // 슬라이더 값 변경 이벤트 연결
    }
    
    private void OnMasterToggleChanged(bool isOn)
    {
        if (isOn)
        {
            SoundsManager.instance.SetMasterVolume(masterVolumeSlider.value); // 재생
        }
        else
        {
            SoundsManager.instance.SetMasterVolume(0f); // 정지
        }
    }
    
    private void OnBGMToggleChanged(bool isOn)
    {
        if (isOn)
        {
            SoundsManager.instance.SetBGMVolume(bgmSlider.value); //  재생
        }
        else
        {
            SoundsManager.instance.SetBGMVolume(0f); // BGM 정지
        }
    }
    
    private void OnSFXToggleChanged(bool isOn)
    {
        if (isOn)
        {
            SoundsManager.instance.SetSFXVolume(sfxSlider.value);
        }
        else
        {
            SoundsManager.instance.SetSFXVolume(0f); // 정지
        }
    }
    

    private void OnBGMVolumeChanged(float value)
    {
        if (bgmToggle.isOn && masterVolumeToggle.isOn)
        {
            SoundsManager.instance.SetBGMVolume(value); // BGM 볼륨 설정    
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (sfxToggle.isOn && masterVolumeToggle.isOn)
        {
            SoundsManager.instance.SetSFXVolume(value); // SFX 볼륨 설정    
        }
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        if (masterVolumeToggle.isOn)
        {
            SoundsManager.instance.SetMasterVolume(value);
        }
    }
}