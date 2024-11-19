using System.Collections;
using System.Collections.Generic; // List<T> 사용을 위한 네임스페이스 추가
using System.IO; // 파일 읽기 및 쓰기 기능을 위한 네임스페이스 추가
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
//using System.Text.RegularExpressions;

public class GameSence : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;       // 대사 텍스트 UI
    public TextMeshProUGUI characterNameText;  // 캐릭터 이름 텍스트 UI
    public TextMeshProUGUI dayText;            // 현재 Day 텍스트 UI
    public Button nextButton;                  // 다음 버튼 UI

    private List<DialogueEntry> dialogues;     // 모든 대사 데이터를 담는 리스트
    private int currentDialogueIndex;          // 현재 대사의 인덱스
    public int? nextDialogueId;                // 다음 대사 ID (null일 경우 다음 Day로 이동)

    void Start()
    {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다. Canvas가 존재하는지 확인하세요.");
            return;
        }

        // Canvas 내에서 UI 요소를 찾아 설정
        dialogueText = canvas.transform.Find("DialogueText")?.GetComponent<TextMeshProUGUI>();
        if (dialogueText == null) Debug.LogError("DialogueText를 찾을 수 없습니다.");

        characterNameText = canvas.transform.Find("CharacterNameText")?.GetComponent<TextMeshProUGUI>();
        if (characterNameText == null) Debug.LogError("CharacterNameText를 찾을 수 없습니다.");

        dayText = canvas.transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
        if (dayText == null) Debug.LogError("DayText를 찾을 수 없습니다.");

        nextButton = canvas.transform.Find("NextButton")?.GetComponent<Button>();
        if (nextButton == null) Debug.LogError("NextButton을 찾을 수 없습니다.");
        else
        {
            nextButton.onClick.AddListener(ProceedToNextDialogue);
        }
        // 대사 데이터를 불러옵니다.
        dialogues = GameManager.instance.dialogues;
        //LoadDialogues(Application.dataPath + "/04 Resources/Dialogues.csv");
        //new List<DialogueEntry>();

        // 세이브 데이터에서 시작할 대사 ID 로드 (없으면 처음부터)
        int savedDialogueId = PlayerPrefs.GetInt("SavedDialogueId", 1);
        currentDialogueIndex = savedDialogueId != -1
            ? dialogues.FindIndex(d => d.id == savedDialogueId)
            : 0;

        // 첫 번째 대사 표시
        if (currentDialogueIndex >= 0 && currentDialogueIndex < dialogues.Count)
            DisplayDialogue(dialogues[currentDialogueIndex]);
       
    }
    void ProceedToNextDialogue()
    {
        DialogueEntry currentDialogue = dialogues[currentDialogueIndex];

        if (currentDialogue.nextDialogueIds.Count > 1)
        {
            // 판정이 필요한 경우 칵테일 Scene으로 이동
            SceneManager.LoadScene("CocktailScene");

            // 판정 결과는 PlayerPrefs 등을 통해서 `CocktailScene`에서 선택된 ID를 반환받아야 합니다.
            // 이후 `SetNextDialogueIdByResult`에서 결과를 처리하여 다음 대사로 이동합니다.
        }
        else if (currentDialogue.nextDialogueIds.Count == 1)
        {
            // 다음 대사 ID가 하나일 경우 바로 해당 ID로 이동
            nextDialogueId = currentDialogue.nextDialogueIds[0];
            MoveToDialogueOrNextDay();
        }
        else
        {
            // 다음 대사 ID가 없을 경우 다음 Day로 이동
            nextDialogueId = null;
            MoveToDialogueOrNextDay();
        }
    }

    // 판정 결과에 따라 다음 대사 ID를 선택하는 메서드
    void SetNextDialogueIdByResult(int resultIndex)
    {
        DialogueEntry currentDialogue = dialogues[currentDialogueIndex];
        if (resultIndex >= 0 && resultIndex < currentDialogue.nextDialogueIds.Count)
        {
            nextDialogueId = currentDialogue.nextDialogueIds[resultIndex];
            MoveToDialogueOrNextDay();
        }
        else
        {
            Debug.LogError("유효하지 않은 판정 결과입니다.");
        }
    }


    // 다음 대사로 이동하거나 다음 Day의 첫 대사로 이동하는 메서드
    void MoveToDialogueOrNextDay()
    {
        if (nextDialogueId.HasValue)
        {
            // nextDialogueId가 있을 경우 해당 ID의 대사 인덱스로 이동
            currentDialogueIndex = dialogues.FindIndex(d => d.id == nextDialogueId.Value);
        }
        else
        {
            // nextDialogueId가 없을 경우 다음 Day의 첫 번째 대사로 이동
            int nextDay = dialogues[currentDialogueIndex].day + 1;
            currentDialogueIndex = dialogues.FindIndex(d => d.day == nextDay);
        }

        // 현재 인덱스가 유효한지 확인 후, 대사 표시 또는 종료 메시지 출력
        if (currentDialogueIndex >= 0 && currentDialogueIndex < dialogues.Count)
        {
            DisplayDialogue(dialogues[currentDialogueIndex]);
        }
        else
        {
            dialogueText.text = "더 이상 대사가 없습니다."; // 대사가 모두 끝났을 때 메시지 표시
            nextButton.interactable = false;               // 다음 버튼 비활성화
        }
    }

    // UI에 대사와 캐릭터, Day 정보를 표시하는 메서드
    void DisplayDialogue(DialogueEntry entry)
    {
        if (dialogueText == null || characterNameText == null || dayText == null)
        {
            Debug.LogError("UI 요소가 설정되지 않았습니다.");
            return;
        }
        dialogueText.text = entry.text;              // 대사 텍스트 설정
        characterNameText.text = entry.character;    // 캐릭터 이름 설정
        dayText.text = "Day " + entry.day;           // Day 텍스트 설정
    }

}
//// 특정 대사에 실패, 성공, 특수 상황에 따른 ID가 모두 존재하는지 확인
//bool HasMultipleNextIds(DialogueEntry dialogue)
//{
//    return GetFailNextId(dialogue.id) != null && GetSuccessNextId(dialogue.id) != null && GetSpecialNextId(dialogue.id) != null;
//}
//// 실패 시 다음 대사 ID 반환 (필요한 경우 설정)
//int? GetFailNextId(int dialogueId) { /* 실패 시 다음 ID 로직 */ return null; }

//// 성공 시 다음 대사 ID 반환 (필요한 경우 설정)
//int? GetSuccessNextId(int dialogueId) { /* 성공 시 다음 ID 로직 */ return null; }

//// 특수 상황 시 다음 대사 ID 반환 (필요한 경우 설정)
//int? GetSpecialNextId(int dialogueId) { /* 특수 상황 시 다음 ID 로직 */ return null; }