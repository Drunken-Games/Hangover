[System.Serializable]
public class SoundSaveData
{
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public int currentBGMIndex = 0;
    
    public bool isMasterMuted = false;
    public bool isBGMMuted = false;
    public bool isSFXMuted = false;
    
    public float lastMasterVolume = 1f;
    public float lastBGMVolume = 1f;
    public float lastSFXVolume = 1f;
    
    public string saveDateTime;
}