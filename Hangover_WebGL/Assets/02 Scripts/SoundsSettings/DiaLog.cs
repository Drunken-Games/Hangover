using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiaLog : MonoBehaviour
{
    [SerializeField]
    private TMP_Text LastDia;

    // Start is called before the first frame update
    void Start()
    {
        ShowLatestDialogue();
    }

    public void ShowLatestDialogue()
    {
        if (GameManager.instance.DialoguesLog != null && GameManager.instance.DialoguesLog.Count > 0)
        {
            GameManager.DialogueLog latestDialogue = GameManager.instance.DialoguesLog[GameManager.instance.DialoguesLog.Count - 1];

            // 텍스트를 설정합니다.
            LastDia.text = latestDialogue.Text;
        }
        else
        {
            Debug.LogWarning("DialoguesLog에 요소가 없거나 GameManager 인스턴스가 없습니다.");
        }
    }
}
