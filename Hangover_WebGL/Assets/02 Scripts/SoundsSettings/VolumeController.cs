using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Master Control")]
    public Toggle masterVolumeToggle;
    public Slider masterVolumeSlider;
    
    [Header("BGM Control")]
    public Toggle bgmToggle;
    public Slider bgmSlider;

    [Header("SFX Control")]
    public Toggle sfxToggle;
    public Slider sfxSlider;
    
    private Color toggleOnColor;
    private Color toggleOffColor;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#00B757", out toggleOnColor);
        ColorUtility.TryParseHtmlString("#778899", out toggleOffColor);
    }
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }

    private void InitializeUI()
    {
        if (SoundsManager.instance == null) return;

        // Master 설정
        masterVolumeToggle.isOn = !SoundsManager.instance.IsMasterMuted();
        masterVolumeSlider.value = SoundsManager.instance.GetLastMasterVolume();
        UpdateSliderInteractable(masterVolumeSlider, masterVolumeToggle.isOn);
        
        // BGM 설정
        bgmToggle.isOn = !SoundsManager.instance.IsBGMMuted();
        bgmSlider.value = SoundsManager.instance.GetLastBGMVolume();
        UpdateSliderInteractable(bgmSlider, bgmToggle.isOn && masterVolumeToggle.isOn);
        
        // SFX 설정
        sfxToggle.isOn = !SoundsManager.instance.IsSFXMuted();
        sfxSlider.value = SoundsManager.instance.GetLastSFXVolume();
        UpdateSliderInteractable(sfxSlider, sfxToggle.isOn && masterVolumeToggle.isOn);

        // 토글 이미지 초기화
        UpdateToggleVisuals(masterVolumeToggle, masterVolumeToggle.isOn);
        UpdateToggleVisuals(bgmToggle, bgmToggle.isOn);
        UpdateToggleVisuals(sfxToggle, sfxToggle.isOn);
    }

    private void SetupEventListeners()
    {
        masterVolumeToggle.onValueChanged.AddListener(OnMasterToggleChanged);
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        bgmToggle.onValueChanged.AddListener(OnBGMToggleChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        
        sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void UpdateSliderInteractable(Slider slider, bool interactable)
    {
        slider.interactable = interactable;
        
        // 핸들 이미지만 비활성화 상태일 때 색상 변경
        Image handleImage = slider.handleRect.GetComponent<Image>();
        handleImage.color = interactable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    private void UpdateToggleVisuals(Toggle toggle, bool isOn)
    {
        Image background = toggle.GetComponentInChildren<Image>();
        if (background != null)
        {
            background.color = isOn ? toggleOnColor : toggleOffColor;
        }
    }

    private void OnMasterToggleChanged(bool isOn)
    {
        SoundsManager.instance.SetMasterMute(!isOn);
        
        UpdateSliderInteractable(masterVolumeSlider, isOn);
        UpdateSliderInteractable(bgmSlider, bgmToggle.isOn && isOn);
        UpdateSliderInteractable(sfxSlider, sfxToggle.isOn && isOn);
        
        UpdateToggleVisuals(masterVolumeToggle, isOn);
    }

    private void OnBGMToggleChanged(bool isOn)
    {
        SoundsManager.instance.SetBGMMute(!isOn);
        
        UpdateSliderInteractable(bgmSlider, isOn && masterVolumeToggle.isOn);
        UpdateToggleVisuals(bgmToggle, isOn);
    }

    private void OnSFXToggleChanged(bool isOn)
    {
        SoundsManager.instance.SetSFXMute(!isOn);
        
        UpdateSliderInteractable(sfxSlider, isOn && masterVolumeToggle.isOn);
        UpdateToggleVisuals(sfxToggle, isOn);
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (masterVolumeToggle.isOn)
        {
            SoundsManager.instance.SetMasterVolume(value / 100f);
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (bgmToggle.isOn && masterVolumeToggle.isOn)
        {
            SoundsManager.instance.SetBGMVolume(value / 100f);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (sfxToggle.isOn && masterVolumeToggle.isOn)
        {
            SoundsManager.instance.SetSFXVolume(value / 100f);
        }
    }

    private void OnDestroy()
    {
        masterVolumeToggle.onValueChanged.RemoveListener(OnMasterToggleChanged);
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmToggle.onValueChanged.RemoveListener(OnBGMToggleChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        sfxToggle.onValueChanged.RemoveListener(OnSFXToggleChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}