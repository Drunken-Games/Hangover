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

    private GameObject nameInputPanel;  // NameInputPanel 오브젝트
    private TMP_InputField nameInputField;  // NameInputField 오브젝트
    private Button submitButton;  // 이름 제출 버튼

    public ChoiceHandler choiceHandler;
    private GameObject notificationPanel; // NotificationPanel 오브젝트
    void Start()
    {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다. Canvas가 존재하는지 확인해 주세요.");
            return;
        }
        // Canvas 하위의 ChoiceHandler 찾기
        notificationPanel = canvas.transform.Find("NotificationPanel")?.gameObject;
        choiceHandler = notificationPanel?.GetComponent<ChoiceHandler>();
        if (choiceHandler == null || notificationPanel == null)
        {
            Debug.LogError("NotificationPanel 또는 ChoiceHandler 스크립트를 찾을 수 없습니다.");
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

        // NameInputPanel 및 하위 요소들 참조
        nameInputPanel = canvas.transform.Find("NameInputPanel")?.gameObject;
        nameInputField = nameInputPanel?.transform.Find("NameInputField")?.GetComponent<TMP_InputField>();
        submitButton = nameInputPanel?.transform.Find("SubmitButton")?.GetComponent<Button>();

        if (nameInputPanel == null || nameInputField == null || submitButton == null)
        {
            Debug.LogError("NameInputPanel 또는 그 하위 요소들을 찾을 수 없습니다. 이름을 다시 확인해 주세요.");
            return;
        }

        // submitButton 클릭 시 SavePlayerName 메서드 호출
        submitButton.onClick.AddListener(SavePlayerName);

        // GameManager의 대화 데이터를 불러오기
        dialogues = GameManager.instance.dialogues;

        // GameManager의 currentDialogueIndex로 첫 번째 대화 표시
        DisplayDialogue(GetCurrentDialogueEntry());

        // 대화 인덱스 확인하여 이름 입력 패널 표시
        GameManager.instance.CheckDialogueForNameInput();
    }


    // 이름 저장 및 패널 비활성화 메서드
    private void SavePlayerName()
    {
        // nameInputField의 텍스트를 GameManager의 playerName에 저장
        GameManager.instance.playerName = nameInputField.text;
        Debug.Log($"Player name saved: {GameManager.instance.playerName}");
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

                        // 성공 시 Total Profit에 특정 가격 추가
                        GameManager.instance.dayResultData.totalProfit += 100; // 예시로 100 추가
                        Debug.Log("성공하여 Total Profit이 증가했습니다.");
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
                // 돌아왔을 때 해당 대화 ID와 칵테일 ID 비교하여, 성공여부 판정, 성공시 Total Profit 에 특정 가격을 올리기
                // 성공 여부와 관계없이 materials에 재료 비용 추가
                int materialCost = CalculateMaterialCost();
                GameManager.instance.dayResultData.materials += materialCost;
                Debug.Log($"재료 비용 {materialCost}이 materials에 추가되었습니다.");
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
        else if(GameManager.instance.DayResultProceed == false)
        {
            // 다음 대화 ID가 없는 경우 DayResultScene으로 이동
            Debug.Log("대화가 종료되었습니다. DayResultScene으로 이동합니다.");
            // 다음 대화 ID가 없는 경우
            // DayResultScene으로 이동 후 돌아온 후에
            // 그 다음 MoveToDialogueOrNextDay(); 실행
            //MoveToDialogueOrNextDay();
            // 다음 대화로 이동하기 위해 플래그 설정
            GameManager.instance.DayResultProceed = true;
            SceneManager.LoadScene("DayResultScene");
        }
        else
        {
            GameManager.instance.DayResultProceed = false;
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
        // 대사 텍스트에 {playerName}을 GameManager의 playerName으로 대체
        string processedText = entry.text.Replace("{playerName}", GameManager.instance.playerName);

        if (processedText.StartsWith("select"))
        {
            notificationPanel.SetActive(true);  // select를 만나면 NotificationPanel 활성화
            ShowChoiceDialogue(processedText);
        }
        else
        {

            notificationPanel.SetActive(false);  // select가 없으면 NotificationPanel 비활성화
            dialogueText.text = processedText;              // 대화 텍스트 설정
            characterNameText.text = entry.character;    // 캐릭터 이름 설정
            dayText.text = "Day " + entry.day;           // Day 텍스트 설정


            // currentDialogueIndex가 2일 때 NameInputPanel과 그 안의 요소들을 활성화
            if (GameManager.instance.currentDialogueIndex == 2)
            {
                nameInputPanel.SetActive(true);  // NameInputPanel 활성화
                nameInputField.interactable = true;  // 입력 필드 활성화
                submitButton.gameObject.SetActive(true);  // 제출 버튼 활성화
            }
            else
            {
                nameInputPanel.SetActive(false);  // NameInputPanel 비활성화
                nameInputField.interactable = false;  // 입력 필드 비활성화
                submitButton.gameObject.SetActive(false);  // 제출 버튼 비활성화
            }
        }
    }

    void ShowChoiceDialogue(string dialogue)
    {
        if (choiceHandler == null)
        {
            Debug.LogError("NotificationPanel에 ChoiceHandler 스크립트가 없습니다.");
            return;
        }
        // select 뒤의 대사와 , 구분자를 기준으로 선택지 파싱
        string[] splitDialogue = dialogue.Substring("select".Length).Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
        
        if(splitDialogue.Length>=2)
        {
            string choice1 = splitDialogue[0].Trim().Trim('"');
            string choice2 = splitDialogue[1].Trim().Trim('"');

            choiceHandler.ShowChoices(choice1, choice2, choiceIndex =>
            {
                if (choiceIndex == 1 && GetCurrentDialogueEntry().nextDialogueIds.Count > 0)
                {
                    GameManager.instance.currentDialogueIndex = GetCurrentDialogueEntry().nextDialogueIds[0];
                }
                else if (choiceIndex == 2 && GetCurrentDialogueEntry().nextDialogueIds.Count > 1)
                {
                    GameManager.instance.currentDialogueIndex = GetCurrentDialogueEntry().nextDialogueIds[1];
                }

                // 선택에 따라 다음 대사를 표시
                DisplayDialogue(GetCurrentDialogueEntry());
            });
        }
    
    }

    private int CalculateMaterialCost()
    {
        int[] cocktailParameters = GameManager.instance.GetCocktailParameters();
        int materialCost = 0;

        // 재료마다 10의 비용으로 가정하고 계산 (예시)
        for (int i = 0; i < cocktailParameters.Length - 1; i++) // 마지막 항목은 제조방법이므로 제외
        {
            materialCost += cocktailParameters[i] * 10;
        }

        return materialCost;
    }


}
