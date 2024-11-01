using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameSence : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;       // 대화 텍스트 UI
    public TextMeshProUGUI characterNameText;  // 캐릭터 이름 텍스트 UI
    public TextMeshProUGUI dayText;            // 현재 Day 텍스트 UI
    public Button nextButton;                  // 다음 버튼 UI

    private List<DialogueEntry> dialogues;     // 대화 내용 데이터를 담을 리스트

    void Start()
    {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다. Canvas가 존재하는지 확인해 주세요.");
            return;
        }

        // Canvas 안의 UI 요소 찾기
        dialogueText = canvas.transform.Find("DialogueText")?.GetComponent<TextMeshProUGUI>();
        characterNameText = canvas.transform.Find("CharacterNameText")?.GetComponent<TextMeshProUGUI>();
        dayText = canvas.transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
        nextButton = canvas.transform.Find("NextButton")?.GetComponent<Button>();

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ProceedToNextDialogue);
        }

        // GameManager의 대화 데이터를 불러오기
        dialogues = GameManager.instance.dialogues;

        // GameManager의 currentDialogueIndex로 첫 번째 대화 표시
        DisplayDialogue(GetCurrentDialogueEntry());
    }

    // GameManager의 currentDialogueIndex를 기반으로 현재 대화 항목 가져오기
    DialogueEntry GetCurrentDialogueEntry()
    {
        int currentDialogueIndex = GameManager.instance.currentDialogueIndex;
        return dialogues[currentDialogueIndex];
    }
    void Update()
    {
        // BuildScene에서 돌아왔을 때 특정 조건에 따라 다음 대사로 이동
        if (GameManager.instance.GameSceneNeedsProceed)
        {
            // 플래그를 false로 설정하여 반복 실행 방지
            GameManager.instance.GameSceneNeedsProceed = false;

            // ProceedToNextDialogue의 특정 부분 실행
            DialogueEntry currentDialogue = GetCurrentDialogueEntry();
            int currentRecipeId = GameManager.instance.GetRecipeId();

            if (currentDialogue.nextDialogueIds.Count > 1)
            {
                int firstNextDialogueId = currentDialogue.nextDialogueIds[0];
                int secondNextDialogueId = currentDialogue.nextDialogueIds[1];

                // 현재 대사의 칵테일 ID와 현재 선택된 칵테일 ID가 일치하는지 확인
                if (currentRecipeId == currentDialogue.id)
                {
                    int nextDialogueIndex = dialogues.FindIndex(d => d.id == firstNextDialogueId);
                    if (nextDialogueIndex != -1)
                    {
                        GameManager.instance.currentDialogueIndex = nextDialogueIndex;
                        Debug.Log($"첫 번째 다음 대사로 이동합니다. 다음 대사 ID: {firstNextDialogueId}");
                    }
                }
                else
                {
                    int nextDialogueIndex = dialogues.FindIndex(d => d.id == secondNextDialogueId);
                    if (nextDialogueIndex != -1)
                    {
                        GameManager.instance.currentDialogueIndex = nextDialogueIndex;
                        Debug.Log($"두 번째 다음 대사로 이동합니다. 다음 대사 ID: {secondNextDialogueId}");
                    }
                }
                DisplayDialogue(GetCurrentDialogueEntry());
            }
        }
    }

    // 다음 대화로 이동하기
    void ProceedToNextDialogue()
    {
        DialogueEntry currentDialogue = GetCurrentDialogueEntry();
        int currentRecipeId = GameManager.instance.GetRecipeId(); // 현재 레시피 ID 가져오기
        
        if (currentDialogue.nextDialogueIds.Count > 1)
        {
            if (!GameManager.instance.GameSceneNeedsProceed)
            {
                //GameManager.instance.GameSceneNeedsProceed = true; // 플래그 초기화
                SceneManager.LoadScene("BuildScene");
            }
            /*else
            {
                int firstNextDialogueId = currentDialogue.nextDialogueIds[0];
                int secondNextDialogueId = currentDialogue.nextDialogueIds[1];

                // 현재 대사의 칵테일 ID와 현재 선택된 칵테일 ID가 일치하는지 확인
                if (currentRecipeId == currentDialogue.id)
                {
                    int nextDialogueIndex = dialogues.FindIndex(d => d.id == firstNextDialogueId);
                    if (nextDialogueIndex != -1)
                    {
                        GameManager.instance.currentDialogueIndex = nextDialogueIndex;
                        Debug.Log($"첫 번째 다음 대사로 이동합니다. 다음 대사 ID: {firstNextDialogueId}");
                    }
                }
                else
                {
                    int nextDialogueIndex = dialogues.FindIndex(d => d.id == secondNextDialogueId);
                    if (nextDialogueIndex != -1)
                    {
                        GameManager.instance.currentDialogueIndex = nextDialogueIndex;
                        Debug.Log($"두 번째 다음 대사로 이동합니다. 다음 대사 ID: {secondNextDialogueId}");
                    }
                }
                DisplayDialogue(GetCurrentDialogueEntry());
                GameManager.instance.GameSceneNeedsProceed = false;
            }
            */
        }
        else if (currentDialogue.nextDialogueIds.Count == 1)
        {
            // 다음 대화 ID가 하나인 경우 바로 해당 ID로 이동
            int nextDialogueId = currentDialogue.nextDialogueIds[0];
            int nextDialogueIndex = dialogues.FindIndex(d => d.id == nextDialogueId);
            if (nextDialogueIndex != -1)
            {
                GameManager.instance.currentDialogueIndex = nextDialogueIndex;
                DisplayDialogue(GetCurrentDialogueEntry());
            }
        }
        else
        {
            // 다음 대화 ID가 없는 경우 다음 Day로 이동
            MoveToDialogueOrNextDay();
        }
    }

    // 다음 대화로 이동하거나 다음 Day의 첫 번째 대화로 이동하는 메서드
    void MoveToDialogueOrNextDay()
    {
        int currentDialogueIndex = GameManager.instance.currentDialogueIndex;
        int nextDay = dialogues[currentDialogueIndex].day + 1;

        int nextDayDialogueIndex = dialogues.FindIndex(d => d.day == nextDay);
        if (nextDayDialogueIndex != -1)
        {
            GameManager.instance.currentDialogueIndex = nextDayDialogueIndex;
            DisplayDialogue(GetCurrentDialogueEntry());
        }
        else
        {
            dialogueText.text = "더 이상 대화가 없습니다."; // 대화가 없을 때 메시지 표시
            nextButton.interactable = false;               // 다음 버튼 비활성화
        }
    }

    // UI에 현재 대화 정보를 표시하는 메서드
    void DisplayDialogue(DialogueEntry entry)
    {
        if (dialogueText == null || characterNameText == null || dayText == null)
        {
            Debug.LogError("UI 요소가 초기화되지 않았습니다.");
            return;
        }
        dialogueText.text = entry.text;              // 대화 텍스트 설정
        characterNameText.text = entry.character;    // 캐릭터 이름 설정
        dayText.text = "Day " + entry.day;           // Day 텍스트 설정
    }
}
