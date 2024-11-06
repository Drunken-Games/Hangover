using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ButtonSoundPlay : MonoBehaviour
{
    
    // 버튼 클릭 시 호출할 메서드
    public void PlayClickSound(string soundName)
    {
        if (SoundsManager.instance != null)
        {
            SoundsManager.instance.PlaySFX(soundName);
        }
    }
}
