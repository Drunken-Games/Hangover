using System.Collections;
using System.Collections.Generic; // List<T> ����� ���� ���ӽ����̽� �߰�
using System.IO; // ���� �б� �� ���� ����� ���� ���ӽ����̽� �߰�
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
//using System.Text.RegularExpressions;

public class GameSence : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;       // ��� �ؽ�Ʈ UI
    public TextMeshProUGUI characterNameText;  // ĳ���� �̸� �ؽ�Ʈ UI
    public TextMeshProUGUI dayText;            // ���� Day �ؽ�Ʈ UI
    public Button nextButton;                  // ���� ��ư UI

    private List<DialogueEntry> dialogues;     // ��� ��� �����͸� ��� ����Ʈ
    private int currentDialogueIndex;          // ���� ����� �ε���
    public int? nextDialogueId;                // ���� ��� ID (null�� ��� ���� Day�� �̵�)

    void Start()
    {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas�� ã�� �� �����ϴ�. Canvas�� �����ϴ��� Ȯ���ϼ���.");
            return;
        }

        // Canvas ������ UI ��Ҹ� ã�� ����
        dialogueText = canvas.transform.Find("DialogueText")?.GetComponent<TextMeshProUGUI>();
        if (dialogueText == null) Debug.LogError("DialogueText�� ã�� �� �����ϴ�.");

        characterNameText = canvas.transform.Find("CharacterNameText")?.GetComponent<TextMeshProUGUI>();
        if (characterNameText == null) Debug.LogError("CharacterNameText�� ã�� �� �����ϴ�.");

        dayText = canvas.transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
        if (dayText == null) Debug.LogError("DayText�� ã�� �� �����ϴ�.");

        nextButton = canvas.transform.Find("NextButton")?.GetComponent<Button>();
        if (nextButton == null) Debug.LogError("NextButton�� ã�� �� �����ϴ�.");
        else
        {
            nextButton.onClick.AddListener(ProceedToNextDialogue);
        }
        // ��� �����͸� �ҷ��ɴϴ�.
        dialogues = GameManager.instance.dialogues;
        //LoadDialogues(Application.dataPath + "/04 Resources/Dialogues.csv");
        //new List<DialogueEntry>();

        // ���̺� �����Ϳ��� ������ ��� ID �ε� (������ ó������)
        int savedDialogueId = PlayerPrefs.GetInt("SavedDialogueId", 1);
        currentDialogueIndex = savedDialogueId != -1
            ? dialogues.FindIndex(d => d.id == savedDialogueId)
            : 0;

        // ù ��° ��� ǥ��
        if (currentDialogueIndex >= 0 && currentDialogueIndex < dialogues.Count)
            DisplayDialogue(dialogues[currentDialogueIndex]);
       
    }
    void ProceedToNextDialogue()
    {
        DialogueEntry currentDialogue = dialogues[currentDialogueIndex];

        if (currentDialogue.nextDialogueIds.Count > 1)
        {
            // ������ �ʿ��� ��� Ĭ���� Scene���� �̵�
            SceneManager.LoadScene("CocktailScene");

            // ���� ����� PlayerPrefs ���� ���ؼ� `CocktailScene`���� ���õ� ID�� ��ȯ�޾ƾ� �մϴ�.
            // ���� `SetNextDialogueIdByResult`���� ����� ó���Ͽ� ���� ���� �̵��մϴ�.
        }
        else if (currentDialogue.nextDialogueIds.Count == 1)
        {
            // ���� ��� ID�� �ϳ��� ��� �ٷ� �ش� ID�� �̵�
            nextDialogueId = currentDialogue.nextDialogueIds[0];
            MoveToDialogueOrNextDay();
        }
        else
        {
            // ���� ��� ID�� ���� ��� ���� Day�� �̵�
            nextDialogueId = null;
            MoveToDialogueOrNextDay();
        }
    }

    // ���� ����� ���� ���� ��� ID�� �����ϴ� �޼���
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
            Debug.LogError("��ȿ���� ���� ���� ����Դϴ�.");
        }
    }


    // ���� ���� �̵��ϰų� ���� Day�� ù ���� �̵��ϴ� �޼���
    void MoveToDialogueOrNextDay()
    {
        if (nextDialogueId.HasValue)
        {
            // nextDialogueId�� ���� ��� �ش� ID�� ��� �ε����� �̵�
            currentDialogueIndex = dialogues.FindIndex(d => d.id == nextDialogueId.Value);
        }
        else
        {
            // nextDialogueId�� ���� ��� ���� Day�� ù ��° ���� �̵�
            int nextDay = dialogues[currentDialogueIndex].day + 1;
            currentDialogueIndex = dialogues.FindIndex(d => d.day == nextDay);
        }

        // ���� �ε����� ��ȿ���� Ȯ�� ��, ��� ǥ�� �Ǵ� ���� �޽��� ���
        if (currentDialogueIndex >= 0 && currentDialogueIndex < dialogues.Count)
        {
            DisplayDialogue(dialogues[currentDialogueIndex]);
        }
        else
        {
            dialogueText.text = "�� �̻� ��簡 �����ϴ�."; // ��簡 ��� ������ �� �޽��� ǥ��
            nextButton.interactable = false;               // ���� ��ư ��Ȱ��ȭ
        }
    }

    // UI�� ���� ĳ����, Day ������ ǥ���ϴ� �޼���
    void DisplayDialogue(DialogueEntry entry)
    {
        if (dialogueText == null || characterNameText == null || dayText == null)
        {
            Debug.LogError("UI ��Ұ� �������� �ʾҽ��ϴ�.");
            return;
        }
        dialogueText.text = entry.text;              // ��� �ؽ�Ʈ ����
        characterNameText.text = entry.character;    // ĳ���� �̸� ����
        dayText.text = "Day " + entry.day;           // Day �ؽ�Ʈ ����
    }

}
//// Ư�� ��翡 ����, ����, Ư�� ��Ȳ�� ���� ID�� ��� �����ϴ��� Ȯ��
//bool HasMultipleNextIds(DialogueEntry dialogue)
//{
//    return GetFailNextId(dialogue.id) != null && GetSuccessNextId(dialogue.id) != null && GetSpecialNextId(dialogue.id) != null;
//}
//// ���� �� ���� ��� ID ��ȯ (�ʿ��� ��� ����)
//int? GetFailNextId(int dialogueId) { /* ���� �� ���� ID ���� */ return null; }

//// ���� �� ���� ��� ID ��ȯ (�ʿ��� ��� ����)
//int? GetSuccessNextId(int dialogueId) { /* ���� �� ���� ID ���� */ return null; }

//// Ư�� ��Ȳ �� ���� ��� ID ��ȯ (�ʿ��� ��� ����)
//int? GetSpecialNextId(int dialogueId) { /* Ư�� ��Ȳ �� ���� ID ���� */ return null; }