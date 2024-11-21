using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 추가
using TMPro;
using System;
using System.Collections.Generic;


public class ChoiceHandler : MonoBehaviour
{
    public GameObject notificationPanel;
    public Button primaryActionButton;
    public Button secondaryActionButton;
    public TextMeshProUGUI primaryButtonText;
    public TextMeshProUGUI secondaryButtonText;

    private Action<int> onChoiceSelected;

    private void Start()
    {
        if (notificationPanel == null) Debug.LogError("ChoiceHandler: notificationPanel이 연결되지 않았습니다.");
        if (primaryActionButton == null) Debug.LogError("ChoiceHandler: primaryActionButton이 연결되지 않았습니다.");
        if (secondaryActionButton == null) Debug.LogError("ChoiceHandler: secondaryActionButton이 연결되지 않았습니다.");
        if (primaryButtonText == null) Debug.LogError("ChoiceHandler: primaryButtonText가 연결되지 않았습니다.");
        if (secondaryButtonText == null) Debug.LogError("ChoiceHandler: secondaryButtonText가 연결되지 않았습니다.");
        if (notificationPanel == null || primaryActionButton == null || secondaryActionButton == null || primaryButtonText == null || secondaryButtonText == null)
        {
            Debug.LogError("ChoiceHandler: UI 요소가 연결되지 않았습니다. Inspector에서 모든 UI 요소가 연결되어 있는지 확인하세요.");
            return;
        }
        // 버튼 클릭 이벤트 설정
        

        //처음에는 패널을 비활성화
        notificationPanel.SetActive(false);
    }

    // 선택지를 표시하는 메서드
    public void ShowChoices(string choice1, string choice2, Action<int> choiceCallback)
    {
        if (primaryButtonText == null || secondaryButtonText == null || notificationPanel == null)
        {
            Debug.LogError("ChoiceHandler: ShowChoices 메서드에서 UI 요소가 null입니다.");
            return;
        }
        primaryButtonText.text = choice1;
        secondaryButtonText.text = choice2;
        onChoiceSelected = choiceCallback;


        // 버튼 클릭 이벤트 설정
        primaryActionButton.onClick.RemoveAllListeners();
        primaryActionButton.onClick.AddListener(() => OnChoiceSelected(1));
        secondaryActionButton.onClick.RemoveAllListeners();
        secondaryActionButton.onClick.AddListener(() => OnChoiceSelected(2));
        // 패널 활성화
        notificationPanel.SetActive(true);
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        // 선택된 결과 전달
        onChoiceSelected?.Invoke(choiceIndex);

        // 패널 비활성화
        notificationPanel.SetActive(false);
    }
}
