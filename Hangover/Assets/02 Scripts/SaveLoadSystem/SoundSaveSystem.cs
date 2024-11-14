using UnityEngine;
using System.IO;
using System;

public class SoundSaveSystem : MonoBehaviour
{
    private static string saveFilePath;

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "soundSave.json");
    }

    public static string GetSaveFilePath()
    {
        if (string.IsNullOrEmpty(saveFilePath))
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "soundSave.json");
        }
        return saveFilePath;
    }

    public static void LoadSoundSettings()
    {
        try
        {
            string filePath = GetSaveFilePath();
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                SoundSaveData loadedData = JsonUtility.FromJson<SoundSaveData>(json);

                if (loadedData != null && SoundsManager.instance != null)
                {
                    // 볼륨값 먼저 설정
                    SoundsManager.instance.SetMasterVolume(loadedData.lastMasterVolume);
                    SoundsManager.instance.SetBGMVolume(loadedData.lastBGMVolume);
                    SoundsManager.instance.SetSFXVolume(loadedData.lastSFXVolume);

                    // 음소거 상태 설정
                    SoundsManager.instance.SetMasterMute(loadedData.isMasterMuted);
                    SoundsManager.instance.SetBGMMute(loadedData.isBGMMuted);
                    SoundsManager.instance.SetSFXMute(loadedData.isSFXMuted);

                    SoundsManager.instance.currentBGMIndex = loadedData.currentBGMIndex;

                    Debug.Log($"사운드 설정 로드 완료: Master({loadedData.lastMasterVolume}, {loadedData.isMasterMuted}), " +
                            $"BGM({loadedData.lastBGMVolume}, {loadedData.isBGMMuted}), " +
                            $"SFX({loadedData.lastSFXVolume}, {loadedData.isSFXMuted})");
                }
            }
            else
            {
                Debug.Log("저장된 사운드 설정이 없습니다. 기본값을 사용합니다.");
                SaveSoundSettings();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"사운드 설정 로드 중 에러 발생: {e.Message}");
            Debug.LogError($"스택 트레이스: {e.StackTrace}");
        }
    }

    public static void SaveSoundSettings()
    {
        try
        {
            if (SoundsManager.instance == null) return;

            SoundSaveData saveData = new SoundSaveData
            {
                masterVolume = SoundsManager.instance.GetMasterVolume() / 100f,
                bgmVolume = SoundsManager.instance.GetBGMVolume() / 100f,
                sfxVolume = SoundsManager.instance.GetSFXVolume() / 100f,
                
                lastMasterVolume = SoundsManager.instance.GetLastMasterVolume() / 100f,
                lastBGMVolume = SoundsManager.instance.GetLastBGMVolume() / 100f,
                lastSFXVolume = SoundsManager.instance.GetLastSFXVolume() / 100f,
                
                isMasterMuted = SoundsManager.instance.IsMasterMuted(),
                isBGMMuted = SoundsManager.instance.IsBGMMuted(),
                isSFXMuted = SoundsManager.instance.IsSFXMuted(),
                
                currentBGMIndex = SoundsManager.instance.currentBGMIndex,
                saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(GetSaveFilePath(), json);
            
            Debug.Log($"사운드 설정 저장 완료: Master({saveData.lastMasterVolume}, {saveData.isMasterMuted}), " +
                     $"BGM({saveData.lastBGMVolume}, {saveData.isBGMMuted}), " +
                     $"SFX({saveData.lastSFXVolume}, {saveData.isSFXMuted})");
        }
        catch (Exception e)
        {
            Debug.LogError($"사운드 설정 저장 중 에러 발생: {e.Message}");
            Debug.LogError($"스택 트레이스: {e.StackTrace}");
        }
    }

    public static void DeleteSoundSettings()
    {
        try
        {
            string filePath = GetSaveFilePath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("사운드 설정 삭제 완료");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"사운드 설정 삭제 중 에러 발생: {e.Message}");
        }
    }
}