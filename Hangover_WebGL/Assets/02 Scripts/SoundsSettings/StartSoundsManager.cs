using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSoundsManager : MonoBehaviour
{
    private SoundsManager soundsManager;
    
        public void Start()
        {
            soundsManager = FindObjectOfType<SoundsManager>();
            
            if (soundsManager == null)
            {
                GameObject soundsManagerObj = new GameObject("SoundsManager");
                soundsManager = soundsManagerObj.AddComponent<SoundsManager>();
    
                // DontDestroyOnLoad를 바로 호출하여 소멸되지 않도록 설정
                DontDestroyOnLoad(soundsManagerObj);
            }
        }
}
