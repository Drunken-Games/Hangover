using System.Collections.Generic;
using System.Collections;
using System.Linq; // System.Linq 추가
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using Febucci.UI.Core;

public static class AsyncOperationExtensions
{
    public static Task AsTask(this AsyncOperation asyncOperation)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOperation.completed += _ => tcs.SetResult(null);
        return tcs.Task;
    }
}

public class GameSence : MonoBehaviour, IPointerDownHandler
{
    private TextMeshProUGUI checkText;
    public TypewriterCore dialogueText;       // 대화 텍스트 UI
    public TextMeshProUGUI characterNameText;  // 캐릭터 이름 텍스트 UI
    public TextMeshProUGUI dayText;            // 현재 Day 텍스트 UI
    public Button nextButton;                  // 다음 버튼 UI

    private List<DialogueEntry> dialogues;     // 대화 내용 데이터를 담을 리스트

    private GameObject nameInputPanel;  // NameInputPanel 오브젝트
    private TMP_InputField nameInputField;  // NameInputField 오브젝트
    private Button submitButton;  // 이름 제출 버튼
    private TouchScreenKeyboard keyboard; // 모바일 키보드 변수

    public ChoiceHandler choiceHandler;
    private GameObject notificationPanel; // NotificationPanel 오브젝트

    [SerializeField] private CocktailCal cocktailCal; // CocktailCal 인스턴스
    private bool isUpdateComplete = false;

    private Coroutine currentCoroutine;

    private bool check_build = false;
    private bool is_RCOPEN = false;
    private int nextid = -1;

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
        dialogueText = dialogueImageTransform.transform.Find("DialogueText")?.GetComponent<TypewriterCore>();
        checkText= dialogueImageTransform.transform.Find("DialogueText").GetComponent<TextMeshProUGUI>();
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

        if (cocktailCal == null)
        {
            Debug.Log("GameSence.cs - Start() - cocktailCal 못찾음");
        }

        if (nameInputPanel == null || nameInputField == null || submitButton == null)
        {
            Debug.LogError("NameInputPanel 또는 그 하위 요소들을 찾을 수 없습니다. 이름을 다시 확인해 주세요.");
            return;
        }

        cocktailCal = GameObject.FindObjectOfType<CocktailCal>();
        if (cocktailCal == null)
        {
            Debug.LogError("CocktailCal을 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log("CocktailCal을 찾았습니다.");
        }


        // 시작 시 비활성화
        nameInputPanel.SetActive(false);
        // submitButton 클릭 시 SavePlayerName 메서드 호출
        submitButton.onClick.AddListener(SavePlayerName);

        // GameManager의 대화 데이터를 불러오기
        dialogues = GameManager.instance.dialogues;
        // GameManager의 currentDialogueIndex로 첫 번째 대화 표시
        SoundsManager.instance.StopAllSFX();
        SoundsManager.instance.PlaySFX(GameManager.instance.currentDialogueIndex.ToString());
        Debug.Log("Start");
        DisplayDialogue(GetCurrentDialogueEntry());

        // 대화 인덱스 확인하여 이름 입력 패널 표시
        GameManager.instance.CheckDialogueForNameInput();

        // TMP_InputField가 활성화될 때 포커스를 자동으로 설정하도록 리스너 추가
        nameInputField.onSelect.AddListener(delegate { ActivateKeyboard(); });

         // 입력이 끝났을 때 SavePlayerName 메서드 호출
        nameInputField.onEndEdit.AddListener(delegate { SavePlayerName(); });
    }

    void Update()
    {
        // BuildScene에서 돌아왔을 때 특정 조건에 따라 다음 대사로 이동
        if (GameManager.instance.GameSceneNeedsProceed)
        {
            // 추가 로그 확인
            if (cocktailCal.cocktails == null || cocktailCal.cocktails.Count == 0)
            {
                Debug.LogError("GameSence에서 cocktailCal의 cocktails 리스트가 null이거나 비어 있습니다.");
            }
            else
            {
                Debug.Log("GameSence에서 cocktailCal의 cocktails 리스트가 정상적으로 참조되었습니다.");
            }
            // 플래그를 false로 설정하여 반복 실행 방지
            GameManager.instance.GameSceneNeedsProceed = false;

            // ProceedToNextDialogue의 특정 부분 실행
            DialogueEntry currentDialogue = GetCurrentDialogueEntry();
            int currentRecipeId = GameManager.instance.GetRecipeId();
            //
            //price=Assets/04 Resources/CocktailData/CocktailDatabase 에서 currentDialogue.cocktailId와 일치하는 칵테일 Id를 찾아서 그 것의 pirce를 넣어둔다
            // price 값 설정
            int price = cocktailCal.GetCocktailPrice(currentRecipeId);

            //if (currentDialogue.nextDialogueIds.Count > 1)
            //{
            StartCoroutine(HandleDialogueProcessing());
            SoundsManager.instance.PlaySFX(GameManager.instance.currentDialogueIndex.ToString());
            dialogueText = GameObject.Find("Canvas/DialogueImage/DialogueText")?.GetComponent<TypewriterCore>();
            //while (dialogueText.text != GetCurrentDialogueEntry().text)
            //{
                DisplayDialogue(GetCurrentDialogueEntry());
            //}
            //}
            //else if(currentDialogue.nextDialogueIds.Count==1)
            //{

            //}
        }

        isUpdateComplete = true;
        Debug.Log($"{GetCurrentDialogueEntry().text},{characterNameText.text},{dayText.text}");
        Debug.Log(checkText.text);
        if (GameManager.instance.DialoguesLog.LastOrDefault()?.Text != checkText.text)
        {
            GameManager.instance.DialoguesLog.Add(new GameManager.DialogueLog(GameManager.instance.currentDialogueIndex, checkText.text));
        }
        if (is_RCOPEN==false&& checkText.text != GetCurrentDialogueEntry().text)
        {
            Debug.Log("Update");
            DisplayDialogue(GetCurrentDialogueEntry());
            GameManager.instance.DialoguesLog.Add(new GameManager.DialogueLog(GameManager.instance.currentDialogueIndex, checkText.text));
            is_RCOPEN = true;
        }
        Debug.Log(isUpdateComplete);

         // TouchScreenKeyboard에서 입력한 텍스트를 TMP_InputField에 업데이트
        if (keyboard != null && keyboard.active)
        {
            nameInputField.text = keyboard.text; // 키보드의 텍스트를 InputField에 반영
        }
    }


    // 이름 저장 및 패널 비활성화 메서드
    private void SavePlayerName()
    {
        // nameInputField의 텍스트를 GameManager의 playerName에 저장
        GameManager.instance.dayResultData.playerName = nameInputField.text;
        if (nameInputField.text != null)
        {
            Debug.Log($"Player name saved: {GameManager.instance.dayResultData.playerName}");
            nameInputPanel.SetActive(false);
            // 이름 입력 패널을 제거
            Destroy(nameInputPanel);
            // nextButton 활성화를 위한 코루틴 실행
            StartCoroutine(WaitForNextButtonActivation());
        }

    }


    // 사용자가 InputField를 터치했을 때 포커스 설정
    public void OnPointerDown(PointerEventData eventData)
    {
        ActivateKeyboard(); // 키보드 활성화
    }

    private void ActivateKeyboard()
    {
        nameInputField.Select(); // InputField 선택
        nameInputField.ActivateInputField(); // 모바일 키보드 강제 활성화

        // 모바일 환경에서 키보드를 강제로 열도록 추가
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
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
        Debug.Log(currentDialogueIndex);
        // 현재 대화 인덱스를 기준으로 대화 리스트에서 해당 인덱스를 반환
        DialogueEntry dialogueEntry = dialogues.FirstOrDefault(d => d.id == currentDialogueIndex);

        if (EqualityComparer<DialogueEntry>.Default.Equals(dialogueEntry, default(DialogueEntry)))
        {
            Debug.LogError($"해당 ID {currentDialogueIndex}에 맞는 대화 항목을 찾을 수 없습니다.");
        }

        else
        {
            // 속성들이 올바르게 출력되도록 로그 추가
            Debug.Log($"ID: {dialogueEntry.id}, Day: {dialogueEntry.day}, Character: {dialogueEntry.character}, Text: {dialogueEntry.text}, Cocktail ID: {dialogueEntry.cocktailIds}");
        }
        return dialogueEntry;
    }
    IEnumerator HandleDialogueProcessing()
    {
        DialogueEntry currentDialogue = GetCurrentDialogueEntry();
        int currentRecipeId = GameManager.instance.GetRecipeId();
        int price = cocktailCal.GetCocktailPrice(currentRecipeId);
        Debug.Log("hi");
        if (currentDialogue.nextDialogueIds.Count > 1)
        {
            if (currentDialogue.text.Contains("{Build}") && currentDialogue.id != 78)
            {
                Debug.Log(1);
                int firstNextDialogueId = currentDialogue.nextDialogueIds[0];
                int secondNextDialogueId = currentDialogue.nextDialogueIds[1];
                Debug.Log($"currentRecipeId: {currentRecipeId}, currentDialogue.cocktailId: {currentDialogue.cocktailIds}");

                // 현재 대사의 칵테일 ID와 현재 선택된 칵테일 ID가 일치하는지 확인
                bool foundMatchingId = true;

                for (int i = 0; i < currentDialogue.cocktailIds.Count; i++)
                {
                    if (currentRecipeId == currentDialogue.cocktailIds[i])
                    {
                        foundMatchingId = false;
                        Debug.Log(currentRecipeId);
                        break; // 일치하는 ID를 찾으면 루프 종료
                    }
                }

                if (foundMatchingId && currentRecipeId != 0)
                {
                    int nextDialogueIndex = dialogues.FindIndex(d => d.id == firstNextDialogueId);
                    if (nextDialogueIndex != -1)
                    {
                        GameManager.instance.currentDialogueIndex = firstNextDialogueId;
                        Debug.Log($"첫 번째 다음 대사로 이동합니다. 다음 대사 ID: {firstNextDialogueId}");
                    }
                }
                else
                {
                    GameManager.instance.currentDialogueIndex = secondNextDialogueId;
                    Debug.Log($"두 번째 다음 대사로 이동합니다. 다음 대사 ID: {secondNextDialogueId}");

                    // 성공 시 Total Profit에 특정 가격 추가
                    GameManager.instance.dayResultData.totalProfit += price; // 예시로 100 추가
                    Debug.Log("성공하여 Total Profit이 증가했습니다.");
                }
            }
            else if (currentDialogue.id == 78)
            {
                if (currentRecipeId == 9)
                {
                    GameManager.instance.currentDialogueIndex = 83;
                    GameManager.instance.dayResultData.totalProfit += price; // 예시로 100 추가
                    Debug.Log("성공하여 Total Profit이 증가했습니다.");
                }
                else if (currentRecipeId == 1 || currentRecipeId == 15)
                {
                    GameManager.instance.currentDialogueIndex = 80;
                    GameManager.instance.dayResultData.totalProfit += price; // 예시로 100 추가
                    Debug.Log("성공하여 Total Profit이 증가했습니다.");
                }
                else
                {
                    GameManager.instance.currentDialogueIndex = 79;
                }
            }
        }
        else if (currentDialogue.nextDialogueIds.Count == 1)
        {
            int firstNextDialogueId = currentDialogue.nextDialogueIds[0];
            GameManager.instance.currentDialogueIndex = firstNextDialogueId;
            Debug.Log($"첫 번째 다음 대사로 이동합니다. 다음 대사 ID: {firstNextDialogueId}");
            GameManager.instance.dayResultData.totalProfit += price; // 예시로 100 추가
            Debug.Log("성공하여 Total Profit이 증가했습니다.");
        }


        // 돌아왔을 때 해당 대화 ID와 칵테일 ID 비교하여, 성공여부 판정, 성공 시 Total Profit 에 특정 가격을 올리기
        // 성공 여부와 관계없이 materials에 재료 비용 추가
        int[] cocktailParameters = GameManager.instance.GetCocktailParameters();
        int materialCost = cocktailCal.CalculateMaterialCost(cocktailParameters);
        GameManager.instance.dayResultData.materials += materialCost;
        Debug.Log($"재료 비용 {materialCost}이 materials에 추가되었습니다. {currentRecipeId} {currentDialogue.cocktailIds}");
        SoundsManager.instance.StopAllSFX();
        Debug.Log(GameManager.instance.currentDialogueIndex);
        yield break; // 비동기 메서드 종료

    }
    // 특수 분기 처리를 위한 메서드
    void HandleSpecialBranch(int branchId)
    {
        if (branchId != -1)
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
    }

    // 텍스트의 모든 플레이스홀더를 처리하고 제거하는 메서드
    IEnumerator ProcessDialogueText(string text, System.Action<string> callback)
    {

        string processedText = text;

        if (processedText.Contains("{playerName}"))
            processedText = processedText.Replace("{playerName}", GameManager.instance.dayResultData.playerName);
        if (processedText.Contains("{name"))
            processedText = processedText.Replace("{name}", HandleOpenName());
        if (processedText.Contains("{openRecipe}"))
            processedText = processedText.Replace("{openRecipe}", HandleOpenRecipeBook());

        if (processedText.Contains("{closeRecipe}"))
            processedText = processedText.Replace("{closeRecipe}", HandleCloseRecipeBook());

        if (processedText.Contains("{Build}"))
        {
            yield return StartCoroutine(HandleOpenDrinkBuildScene(processedText));
            processedText = processedText.Replace("{Build}", "");
        }


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
        Match match = Regex.Match(processedText, @"\{openEnding}\s+(\d+)");
        if (match.Success)
        {
            int endingNumber = int.Parse(match.Groups[1].Value);
            processedText = processedText.Replace(match.Value, HandleOpenEndingScene(endingNumber));
        }


        callback?.Invoke(processedText);
        yield break;
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



    IEnumerator HandleOpenDrinkBuildScene(string processedText)
    {
        if (!GameManager.instance.GameSceneNeedsProceed && processedText == "{Build}")
        {
            // BuildScene을 비동기로 로드
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BuildScene");
            while (!asyncLoad.isDone)
            {
                yield return null; // 비동기 로드가 완료될 때까지 대기
            }
            StopAllCoroutines(); // 현재 MonoBehaviour의 모든 코루틴 중지
            this.enabled = false;
            yield break; // 메서드 종료
        }
        else if (!GameManager.instance.GameSceneNeedsProceed)
        {
            check_build = true;
            yield break;
        }
        yield break;
        //else if (processedText == "{build}")
        //{
        //    ProceedToNextDialogue();
        //    return;
        //}
        //
        ///SceneManager.LoadScene("BuildScene");
        //GameManager.instance.tempSpecialBranch = 1;
    }

    string HandleFireCount()
    {
        GameManager.instance.dayResultData.fireCount++;
        return "";
    }

    string HandleRobotCount()
    {
        GameManager.instance.dayResultData.robotCount++;
        return "";
    }

    string HandleBranchIdx()
    {
        //GameManager.instance.branchIdx++;
        return "";
    }

    string HandleCheckRobotCount()
    {
        int dialogueId = GameManager.instance.dayResultData.robotCount == 2 ? 198 :
                         GameManager.instance.dayResultData.robotCount == 1 ? 201 : 203;
        nextid = dialogueId;
        return "";
    }

    private System.Collections.IEnumerator SCShowSelectPanel()
    {
        while (!notificationPanel.gameObject.activeSelf)
        {
            notificationPanel.SetActive(true);
            yield return null;
        }
    }

    string HandleSelectPanel()
    {
        StartCoroutine(SCShowSelectPanel());

        // NotificationPanel에서 PrimaryActionButton 및 SecondaryActionButton을 가져옴
        Button primaryActionButton = notificationPanel.transform.Find("PrimaryActionButton")?.GetComponent<Button>();
        Button secondaryActionButton = notificationPanel.transform.Find("SecondaryActionButton")?.GetComponent<Button>();
        
        if (primaryActionButton != null && secondaryActionButton != null)
        {
            // PrimaryActionButton 클릭 시 endingTrigger를 0으로 설정하고 패널을 숨긴 후 다음 대화로 이동
            primaryActionButton.onClick.AddListener(() =>
            {
                GameManager.instance.dayResultData.endingTrigger = 0;
                notificationPanel.SetActive(false);
                ProceedToNextDialogue();
            });

            // SecondaryActionButton 클릭 시 endingTrigger를 1로 설정하고 패널을 숨긴 후 다음 대화로 이동
            secondaryActionButton.onClick.AddListener(() =>
            {
                GameManager.instance.dayResultData.endingTrigger = 1;
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
        int dialogueId = GameManager.instance.dayResultData.endingTrigger == 0 ? 149 : 175;
        nextid = dialogueId;
        Debug.Log(dialogueId);
        return "";
    }

    string HandleCheckTrigger2Dialogue()
    {
        int dialogueId = GameManager.instance.dayResultData.endingTrigger == 0 ? 206 : 225;
        nextid = dialogueId;
        return dialogueId.ToString();
    }

    // HandleOpenEndingScene 메서드
    string HandleOpenEndingScene(int endingNumber)
    {
        GameManager.instance.endingNumber = endingNumber-1;
        SceneManager.LoadScene("EndingScene");
        return "";
    }
    void DisplayDialogue(DialogueEntry entry)
    {
        if (dialogueText == null)
        {
            dialogueText = GameObject.Find("Canvas/DialogueImage/DialogueText")?.GetComponent<TypewriterCore>();
            if (dialogueText == null) return;
        }
        Debug.Log("HI");
        StartCoroutine(ProcessDialogueText(entry.text, processedText =>
        {
            if (processedText == "") return;
            //if (entry.text.Contains("{Build}")) return;
            dialogueText.ShowText(processedText);
            // Debug.Log(dialogueText.text);
            if (!string.IsNullOrEmpty(GameManager.instance.dayResultData.playerName) && entry.character == "주인공")
            {
                characterNameText.text = GameManager.instance.dayResultData.playerName;
            }
            else
            {
                characterNameText.text = entry.character;
            }
            dayText.text = "Day " + entry.day;
        }));
        Debug.Log("HI");
        // Debug.Log($"{dialogueText.text},{characterNameText.text},{dayText.text}");
        //GameManager.instance.DialoguesLog.Add(new GameManager.DialogueLog(GameManager.instance.currentDialogueIndex, checkText.text));
        return;
    }
    // 다음 대화로 이동하기
    void ProceedToNextDialogue()
    {
        if (nextid == -1)
        {
            if (check_build == true)
            {
                dialogueText = null;
                SceneManager.LoadScene("BuildScene");
                return;
            }
            DialogueEntry currentDialogue = GetCurrentDialogueEntry();
            Debug.Log("596");
            if (GameManager.instance.tempSpecialBranch != -1)
            {
                Debug.Log(GameManager.instance.tempSpecialBranch);
                HandleSpecialBranch(GameManager.instance.tempSpecialBranch);
                GameManager.instance.ClearSpecialBranch(); // 분기 처리 후 초기화
                return;
            }

            //string processedText = ProcessDialogueText(currentDialogue.text);
            //dialogueText.text = processedText;

            int currentRecipeId = GameManager.instance.GetRecipeId(); // 현재 레시피 ID 가져오기
            int nextDialogueId = currentDialogue.nextDialogueIds[0];
            Debug.Log(nextDialogueId);
            //if (currentDialogue.nextDialogueIds.Count > 1)
            //{


            //}
            if (GameManager.instance.currentDialogueIndex==97&&nextDialogueId == 97)
            {
                Debug.Log(GameManager.instance.currentDialogueIndex);
            }
            if (currentDialogue.nextDialogueIds.Count == 1 && nextDialogueId != -1)
            {
                // 다음 대화 ID가 하나인 경우 바로 해당 ID로 이동
                //int nextDialogueId = currentDialogue.nextDialogueIds[0];
                //int nextDialogueIndex = dialogues.FindIndex(d => d.id == nextDialogueId);
                //if (nextDialogueIndex != -1)
                //{
                GameManager.instance.currentDialogueIndex = nextDialogueId;
                //nextDialogueIndex;
                Debug.Log(GameManager.instance.currentDialogueIndex);
                SoundsManager.instance.StopAllSFX();
                SoundsManager.instance.PlaySFX(GameManager.instance.currentDialogueIndex.ToString());
                Debug.Log("ProceedToNextDialogue");
                DisplayDialogue(GetCurrentDialogueEntry());
                //}
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
            else if(GameManager.instance.DayResultProceed == true&&GameManager.instance.currentDialogueIndex==41 || GameManager.instance.currentDialogueIndex==93 || GameManager.instance.currentDialogueIndex == 138 || GameManager.instance.currentDialogueIndex == 174 || GameManager.instance.currentDialogueIndex ==192)
            {
                //GameManager.instance.dayResultData.beforeMoney = 0;
                //GameManager.instance.dayResultData.totalProfit = 0;
                //GameManager.instance.dayResultData.tip = 0;
                //GameManager.instance.dayResultData.materials = 0;
                //GameManager.instance.dayResultData.netProfit = 0;
                //GameManager.instance.dayResultData.afterMoney = 0;
                GameManager.instance.DayResultProceed = false;
                Debug.Log(GameManager.instance.currentDialogueIndex);
                MoveToDialogueOrNextDay();
            }
        }
        else
        {
            GameManager.instance.currentDialogueIndex = nextid;
            Debug.Log(GameManager.instance.currentDialogueIndex);
            SoundsManager.instance.StopAllSFX();
            SoundsManager.instance.PlaySFX(GameManager.instance.currentDialogueIndex.ToString());
            Debug.Log("ProceedToNextDialogue");
            DisplayDialogue(GetCurrentDialogueEntry());
            nextid = -1;
        }
    }

    // 다음 대화로 이동하거나 다음 Day의 첫 번째 대화로 이동하는 메서드
    void MoveToDialogueOrNextDay()
    {
        int currentDialogueIndex = GameManager.instance.currentDialogueIndex;
        int nextDay = dialogues[currentDialogueIndex].day + 1;

        // nextDay에 해당하는 첫 번째 대화 항목을 찾고, id 값을 가져옵니다.
        // nextDay에 해당하는 첫 번째 대화 항목을 찾고, id 값을 가져옵니다.
        var nextDialogue = dialogues.FirstOrDefault(d => d.day == nextDay);

        // 기본값과 비교하여 nextDialogue의 존재 여부 확인
        int nextDayDialogueIndex = (nextDialogue.Equals(default(DialogueEntry))) ? -1 : nextDialogue.id;
        if (nextDayDialogueIndex == 41 || nextDayDialogueIndex == 93 || nextDayDialogueIndex == 138 || nextDayDialogueIndex == 174 || nextDayDialogueIndex == 192)
        {
            if (nextDayDialogueIndex != -1)
            {
                GameManager.instance.currentDialogueIndex = nextDayDialogueIndex;
                SoundsManager.instance.StopAllSFX();
                SoundsManager.instance.PlaySFX(GameManager.instance.currentDialogueIndex.ToString());
                Debug.Log("MoveToDialogueOrNextDay");
                Debug.Log(nextDayDialogueIndex);
                Debug.Log(currentDialogueIndex);
                DisplayDialogue(GetCurrentDialogueEntry());
            }
            else
            {
                dialogueText.ShowText("더 이상 대화가 없습니다."); // 대화가 없을 때 메시지 표시
                nextButton.interactable = false;               // 다음 버튼 비활성화
            }
        }
    }
        

}
