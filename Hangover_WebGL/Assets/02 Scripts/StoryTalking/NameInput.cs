using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.EventSystems;

public class NameInput : MonoBehaviour, IPointerDownHandler
{
    public TMP_InputField nameInputField; // NameInputField 참조

    void Start()
    {
        // TMP_InputField가 활성화될 때 포커스를 자동으로 설정하도록 리스너 추가
        nameInputField.onSelect.AddListener(delegate { ActivateKeyboard(); });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 사용자가 InputField를 터치했을 때 포커스 설정
        ActivateKeyboard();
    }

    private void ActivateKeyboard()
    {
        nameInputField.Select();
        nameInputField.ActivateInputField(); // 모바일 키보드 강제 활성화

        // 모바일 환경에서 키보드를 강제로 열도록 추가
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }
}
//public class NameInput : MonoBehaviour
//{

    //public TMP_InputField nameInputField;  // 이름 입력 필드
    //public Button submitButton;            // 제출 버튼

    //void Start()
    //{
    //    // 제출 버튼에 리스너 추가
    //    submitButton.onClick.AddListener(OnSubmitName);
    //}

    //// 이름 제출 처리 메서드
    //private void OnSubmitName()
    //{
    //    string inputName = nameInputField.text;  // 입력된 이름 가져오기

    //    if (!string.IsNullOrEmpty(inputName) && GameManager.instance != null)
    //    {
    //        GameManager.instance.SetPlayerName(inputName);  // GameManager에 이름 저장
    //        gameObject.SetActive(false);                    // 입력 UI 비활성화
    //    }
    //}
//}
