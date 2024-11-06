using System.Collections.Generic;
using System.Linq; // System.Linq 추가
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

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

    private List<CocktailData> cocktails; // cocktails 리스트 정의


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
        // Canvas 하위의 DialogueImage에서 UI 요소 찾기
        Transform dialogueImageTransform = canvas.transform.Find("DialogueImage");
        if (dialogueImageTransform == null)
        {
            Debug.LogError("DialogueImage를 찾을 수 없습니다.");
            return;
        }
        // Canvas 안의 UI 요소 찾기
        dialogueText = dialogueImageTransform.transform.Find("DialogueText")?.GetComponent<TextMeshProUGUI>();
        characterNameText = dialogueImageTransform.transform.Find("CharacterNameText")?.GetComponent<TextMeshProUGUI>();
        dayText = canvas.transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
        nextButton = dialogueImageTransform.transform.Find("NextButton")?.GetComponent<Button>();

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


        // 시작 시 비활성화
        nameInputPanel.SetActive(false);
        // submitButton 클릭 시 SavePlayerName 메서드 호출
        submitButton.onClick.AddListener(SavePlayerName);

        // GameManager의 대화 데이터를 불러오기
        dialogues = GameManager.instance.dialogues;

        // GameManager의 currentDialogueIndex로 첫 번째 대화 표시
        DisplayDialogue(GetCurrentDialogueEntry());

        // 대화 인덱스 확인하여 이름 입력 패널 표시
        GameManager.instance.CheckDialogueForNameInput();
        LoadCocktailData(); // cocktails 데이터 로드
    }


    // 이름 저장 및 패널 비활성화 메서드
    private void SavePlayerName()
    {
        // nameInputField의 텍스트를 GameManager의 playerName에 저장
        GameManager.instance.playerName = nameInputField.text;
        if (nameInputField.text!=null)
        {
            Debug.Log($"Player name saved: {GameManager.instance.playerName}");
            nameInputPanel.SetActive(false);
            // 이름 입력 패널을 제거
            Destroy(nameInputPanel);
            // nextButton 활성화를 위한 코루틴 실행
            StartCoroutine(WaitForNextButtonActivation());
        }
        
    }
    // nextButton이 활성화될 때까지 기다리는 코루틴
    private System.Collections.IEnumerator WaitForNextButtonActivation()
    {
        // nextButton의 SetActive 상태가 true가 될 때까지 반복
        while (!nextButton.gameObject.activeSelf)
        {
            nextButton.gameObject.SetActive(true);
            yield return null;  // 한 프레임을 기다립니다
        }

        // nextButton이 활성화되면 다음 대화로 진행
        ProceedToNextDialogue();
    }


    // GameManager의 currentDialogueIndex를 기반으로 현재 대화 항목 가져오기
    DialogueEntry GetCurrentDialogueEntry()
    {
        int currentDialogueIndex = GameManager.instance.currentDialogueIndex;
        return dialogues[currentDialogueIndex];
    }
    // Resources 폴더에서 모든 CocktailData ScriptableObject 로드
    private void LoadCocktailData()
    {
        cocktails = Resources.LoadAll<CocktailData>("DialogueDataAssets/CocktailDatabase").ToList();
        for (int i = 0; i < cocktails.Count; i++)
        {
            // 칵테일 데이터의 각 속성을 출력
            Debug.Log($"Index {i}: id:{cocktails[i].id}, Name: {cocktails[i].cocktailName}, Price: {cocktails[i].price}, Sweet: {cocktails[i].sweet}, Sour: {cocktails[i].sour}, Bitter: {cocktails[i].bitter}, Spice: {cocktails[i].spice}, Spirit: {cocktails[i].spirit}, AlcoholContent: {cocktails[i].alcoholContent}, Taste: {cocktails[i].taste}, Description: {cocktails[i].description}, Method: {cocktails[i].method}");
        }
        Debug.Log("칵테일 데이터 로드 완료");
    }

    // 특정 칵테일 ID로 가격 가져오기
    private int GetCocktailPrice(int cocktailId)
    {
        for (int i = 0; i < cocktails.Count; i++)
        {
            // 칵테일 데이터의 각 속성을 출력
            Debug.Log($"Index {i}: id:{cocktails[i].id}, Name: {cocktails[i].cocktailName}, Price: {cocktails[i].price}, Sweet: {cocktails[i].sweet}, Sour: {cocktails[i].sour}, Bitter: {cocktails[i].bitter}, Spice: {cocktails[i].spice}, Spirit: {cocktails[i].spirit}, AlcoholContent: {cocktails[i].alcoholContent}, Taste: {cocktails[i].taste}, Description: {cocktails[i].description}, Method: {cocktails[i].method}");
            if (cocktails[i].id == cocktailId)
            {

                return cocktails[i].price;
            }
        }
        return 0;
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
            //
            //price=Assets/04 Resources/CocktailData/CocktailDatabase 에서 currentDialogue.cocktailId와 일치하는 칵테일 Id를 찾아서 그 것의 pirce를 넣어둔다
            // price 값 설정
            int price = GetCocktailPrice(currentDialogue.cocktailId);
           
            if (currentDialogue.nextDialogueIds.Count > 1)
            {
                if (currentDialogue.nextDialogueIds.Count == 2)
                {
                    int firstNextDialogueId = currentDialogue.nextDialogueIds[0];
                    int secondNextDialogueId = currentDialogue.nextDialogueIds[1];

                    // 현재 대사의 칵테일 ID와 현재 선택된 칵테일 ID가 일치하는지 확인
                    if (currentRecipeId != currentDialogue.cocktailId && currentRecipeId!=0)
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

                            // 성공 시 Total Profit에 특정 가격 추가
                            GameManager.instance.dayResultData.totalProfit += price; // 예시로 100 추가
                            Debug.Log("성공하여 Total Profit이 증가했습니다.");
                        }
                    }
                }
                else if (currentDialogue.nextDialogueIds.Count == 3)
                {
                    if(currentDialogue.cocktailId ==9)
                    {
                        GameManager.instance.currentDialogueIndex = 82;
                        GameManager.instance.dayResultData.totalProfit += price; // 예시로 100 추가
                        Debug.Log("성공하여 Total Profit이 증가했습니다.");
                    }
                    else if (currentDialogue.cocktailId == 1 || currentDialogue.cocktailId == 15)
                    {
                        GameManager.instance.currentDialogueIndex = 79;
                        GameManager.instance.dayResultData.totalProfit += price; // 예시로 100 추가
                        Debug.Log("성공하여 Total Profit이 증가했습니다.");
                    }
                    else
                    {
                        GameManager.instance.currentDialogueIndex = 78;
                    }
                }
                // 돌아왔을 때 해당 대화 ID와 칵테일 ID 비교하여, 성공여부 판정, 성공시 Total Profit 에 특정 가격을 올리기
                // 성공 여부와 관계없이 materials에 재료 비용 추가
                int materialCost = CalculateMaterialCost();
                GameManager.instance.dayResultData.materials += materialCost;
                Debug.Log($"재료 비용 {materialCost}이 materials에 추가되었습니다.{currentRecipeId}{currentDialogue.cocktailId}");
                DisplayDialogue(GetCurrentDialogueEntry());
            }
        }
    }
    // 특수 분기 처리를 위한 메서드
    void HandleSpecialBranch(int branchId)
    {
        switch (branchId)
        {
            case 1:
                // 예시: 분기 1에 대한 처리
                Debug.Log("특수 분기 1 실행 중...");
                // 추가 로직...
                break;
            case 2:
                // 예시: 분기 2에 대한 처리
                Debug.Log("특수 분기 2 실행 중...");
                // 추가 로직...
                break;
                // 다른 분기들 추가 가능
        }
    }

    // 텍스트의 모든 플레이스홀더를 처리하고 제거하는 메서드
    string ProcessDialogueText(string text)
    {
        string processedText = text;

        if (processedText.Contains("{playerName}"))
            processedText = processedText.Replace("{playerName}", GameManager.instance.playerName);
        if (processedText.Contains("{name"))
            processedText = processedText.Replace("{name}", HandleOpenName());
        if (processedText.Contains("{openRecipe}"))
            processedText = processedText.Replace("{openRecipe}", HandleOpenRecipeBook());

        if (processedText.Contains("{closeRecipe}"))
            processedText = processedText.Replace("{closeRecipe}", HandleCloseRecipeBook());

        if (processedText.Contains("{Build}"))
            processedText = processedText.Replace("{Build}", HandleOpenDrinkBuildScene());

        if (processedText.Contains("{fireCount}"))
            processedText = processedText.Replace("{fireCount}", HandleFireCount());

        if (processedText.Contains("{robotCount}"))
            processedText = processedText.Replace("{robotCount}", HandleRobotCount());

        if (processedText.Contains("{checkRobot}"))
            processedText = processedText.Replace("{checkRobot}", HandleCheckRobotCount());

        if (processedText.Contains("{select}"))
        {
            processedText = processedText.Replace("{select}", HandleSelectPanel());
            processedText = "";
        }
            

        if (processedText.Contains("{branchIdx}"))
            processedText = processedText.Replace("{branchIdx}", HandleBranchIdx());

        if (processedText.Contains("{checkTrigger1}"))
            processedText = processedText.Replace("{checkTrigger1}", HandleCheckTrigger1Dialogue());

        if (processedText.Contains("{checkTrigger2}"))
            processedText = processedText.Replace("{checkTrigger2}", HandleCheckTrigger2Dialogue());

        // {openEnding} 뒤에 숫자가 있을 경우 처리
        Match match = Regex.Match(processedText, @"\{openEnding\s+(\d+)\}");
        if (match.Success)
        {
            int endingNumber = int.Parse(match.Groups[1].Value);
            processedText = processedText.Replace(match.Value, HandleOpenEndingScene(endingNumber));
        }


        Debug.Log(processedText);
        return processedText;
    }

    // HandleOpenName 메서드
    string HandleOpenName()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            nameInputField.interactable = true;
            submitButton.gameObject.SetActive(true);
            
            nextButton.gameObject.SetActive(false);
        }
        return "";  // 필요 시 반환 값을 변경할 수 있음
    }
    // 개별 플레이스홀더 처리 메서드들 (예제)
    string HandleOpenRecipeBook()
    {
        // "GameUISet" 오브젝트를 찾은 후 그 하위에서 "RecipeUICanvas" 오브젝트를 찾음
        GameObject gameUISet = GameObject.Find("GameUISet");

        // "RecipeUICanvas" 하위 오브젝트를 찾음
        GameObject recipeUICanvas = gameUISet.transform.Find("RecipeUICanvas")?.gameObject;

        recipeUICanvas.SetActive(true); // RecipeUICanvas 활성화

        return "";
    }

    string HandleCloseRecipeBook()
    {
        // "GameUISet" 오브젝트를 찾은 후 그 하위에서 "RecipeUICanvas" 오브젝트를 찾음
        GameObject gameUISet = GameObject.Find("GameUISet");

        // "RecipeUICanvas" 하위 오브젝트를 찾음
        GameObject recipeUICanvas = gameUISet.transform.Find("RecipeUICanvas")?.gameObject;

        recipeUICanvas.SetActive(false); // RecipeUICanvas 비활성화


        return "";
    }


    string HandleOpenDrinkBuildScene()
    {
        ProceedToNextDialogue();
        ///SceneManager.LoadScene("BuildScene");
        GameManager.instance.tempSpecialBranch = 1;
        return "";
    }

    string HandleFireCount()
    {
        GameManager.instance.fireCount++;
        return "";
    }

    string HandleRobotCount()
    {
        GameManager.instance.robotCount++;
        return "";
    }

    string HandleBranchIdx()
    {
        GameManager.instance.branchIdx++;
        return "";
    }

    string HandleCheckRobotCount()
    {
        int dialogueId = GameManager.instance.robotCount == 2 ? 196 :
                         GameManager.instance.robotCount == 1 ? 201 : 199;
        GameManager.instance.currentDialogueIndex = dialogueId;
        return dialogueId.ToString();
    }

    string HandleSelectPanel()
    {
        ShowSelectPanel();

        // NotificationPanel에서 PrimaryActionButton 및 SecondaryActionButton을 가져옴
        Button primaryActionButton = notificationPanel.transform.Find("PrimaryActionButton")?.GetComponent<Button>();
        Button secondaryActionButton = notificationPanel.transform.Find("SecondaryActionButton")?.GetComponent<Button>();

        if (primaryActionButton != null && secondaryActionButton != null)
        {
            // PrimaryActionButton 클릭 시 endingTrigger를 0으로 설정하고 패널을 숨긴 후 다음 대화로 이동
            primaryActionButton.onClick.AddListener(() =>
            {
                GameManager.instance.endingTrigger = 0;
                notificationPanel.SetActive(false);
                ProceedToNextDialogue();
            });

            // SecondaryActionButton 클릭 시 endingTrigger를 1로 설정하고 패널을 숨긴 후 다음 대화로 이동
            secondaryActionButton.onClick.AddListener(() =>
            {
                GameManager.instance.endingTrigger = 1;
                notificationPanel.SetActive(false);
                ProceedToNextDialogue();
            });
        }
        else
        {
            Debug.LogError("NotificationPanel 내의 PrimaryActionButton 또는 SecondaryActionButton을 찾을 수 없습니다.");
        }

        return "";
    }

    string HandleCheckTrigger1Dialogue()
    {
        int dialogueId = GameManager.instance.endingTrigger == 0 ? 147 : 173;
        GameManager.instance.currentDialogueIndex = dialogueId;
        return dialogueId.ToString();
    }

    string HandleCheckTrigger2Dialogue()
    {
        int dialogueId = GameManager.instance.endingTrigger == 0 ? 204 : 223;
        GameManager.instance.currentDialogueIndex = dialogueId;
        return dialogueId.ToString();
    }

    // HandleOpenEndingScene 메서드
    string HandleOpenEndingScene(int endingNumber)
    {
        GameManager.instance.endingNumber = endingNumber;
        SceneManager.LoadScene("EndingScene");
        return "";
    }

    void ShowSelectPanel()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }
    }

    void DisplayDialogue(DialogueEntry entry)
    {
        string processedText = ProcessDialogueText(entry.text);
        dialogueText.text = processedText;
        characterNameText.text = entry.character;
        dayText.text = "Day " + entry.day;
    }
    // 다음 대화로 이동하기
    void ProceedToNextDialogue()
    {
        DialogueEntry currentDialogue = GetCurrentDialogueEntry();

        if (GameManager.instance.tempSpecialBranch != -1)
        {
            HandleSpecialBranch(GameManager.instance.tempSpecialBranch);
            GameManager.instance.ClearSpecialBranch(); // 분기 처리 후 초기화
            return;
        }

        //string processedText = ProcessDialogueText(currentDialogue.text);
        //dialogueText.text = processedText;

        int currentRecipeId = GameManager.instance.GetRecipeId(); // 현재 레시피 ID 가져오기
        int nextDialogueId = currentDialogue.nextDialogueIds[0];
        if (currentDialogue.nextDialogueIds.Count > 1)
        {
            if (!GameManager.instance.GameSceneNeedsProceed)
            {
                //GameManager.instance.GameSceneNeedsProceed = true; // 플래그 초기화
                SceneManager.LoadScene("BuildScene");
            }

        }
        else if (currentDialogue.nextDialogueIds.Count == 1 && nextDialogueId != -1)
        {
            // 다음 대화 ID가 하나인 경우 바로 해당 ID로 이동
            //int nextDialogueId = currentDialogue.nextDialogueIds[0];
            int nextDialogueIndex = dialogues.FindIndex(d => d.id == nextDialogueId);
            if (nextDialogueIndex != -1)
            {
                GameManager.instance.currentDialogueIndex = nextDialogueIndex;
                DisplayDialogue(GetCurrentDialogueEntry());
            }
        }
        else if (GameManager.instance.DayResultProceed == false)
        {
            // 다음 대화 ID가 없는 경우 DayResultScene으로 이동
            Debug.Log("대화가 종료되었습니다. DayResultScene으로 이동합니다.");
            // 다음 대화 ID가 없는 경우
            // DayResultScene으로 이동 후 돌아온 후에
            // 그 다음 MoveToDialogueOrNextDay(); 실행
            //MoveToDialogueOrNextDay();

            // 현재 대사의 day 값을 GameManager의 DayNum에 저장
            GameManager.instance.dayResultData.dayNum = currentDialogue.day;
            Debug.LogError("{currentDialogue.day}");
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


    private int CalculateMaterialCost()
    {
        int[] cocktailParameters = GameManager.instance.GetCocktailParameters();
        int materialCost = 0;

        // 재료마다 10의 비용으로 가정하고 계산 (예시)
        for (int i = 0; i < cocktailParameters.Length - 1; i++) // 마지막 항목은 제조방법이므로 제외
        {
            if (cocktailParameters[i] == 0)
            {
                materialCost += 1;
            }
            else if(cocktailParameters[i] == 1)
            {
                materialCost += 2;
            }
            else if(cocktailParameters[i] == 2)
            {
                materialCost += 3;
            }
            else if(cocktailParameters[i]==3)
            {
                materialCost += 4;
            }
            else
            {
                materialCost += 2;
            }
            
        }

        return materialCost;
    }


}
//void ShowChoiceDialogue(string dialogue)
//{
//    if (choiceHandler == null)
//    {
//        Debug.LogError("NotificationPanel에 ChoiceHandler 스크립트가 없습니다.");
//        return;
//    }
//    // select 뒤의 대사와 , 구분자를 기준으로 선택지 파싱
//    string[] splitDialogue = dialogue.Substring("select".Length).Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);

//    if(splitDialogue.Length>=2)
//    {
//        string choice1 = splitDialogue[0].Trim().Trim('"');
//        string choice2 = splitDialogue[1].Trim().Trim('"');

//        choiceHandler.ShowChoices(choice1, choice2, choiceIndex =>
//        {
//            if (choiceIndex == 1 && GetCurrentDialogueEntry().nextDialogueIds.Count > 0)
//            {
//                GameManager.instance.currentDialogueIndex = GetCurrentDialogueEntry().nextDialogueIds[0];
//            }
//            else if (choiceIndex == 2 && GetCurrentDialogueEntry().nextDialogueIds.Count > 1)
//            {
//                GameManager.instance.currentDialogueIndex = GetCurrentDialogueEntry().nextDialogueIds[1];
//            }

//            // 선택에 따라 다음 대사를 표시
//            DisplayDialogue(GetCurrentDialogueEntry());
//        });
//    }

//}
