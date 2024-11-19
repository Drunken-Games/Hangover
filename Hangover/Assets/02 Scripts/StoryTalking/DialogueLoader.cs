using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

// ��� ������ ������ ����ü ����
[System.Serializable]
public struct DialogueEntry
{
    public int id;                  // ��� ID
    public int day;                 // ���� Day
    public string character;        // ĳ���� �̸�
    public string text;             // ��� �ؽ�Ʈ
    public List<int> nextDialogueIds; // ���� ��� ID ����Ʈ (���� ����)

    public DialogueEntry(int id, int day, string character, string text, List<int> nextDialogueIds)
    {
        this.id = id;
        this.day = day;
        this.character = character;
        this.text = text;
        this.nextDialogueIds = nextDialogueIds;
    }
}
public class DialogueLoader
{
    

    public static List<DialogueEntry> LoadDialogues(string filePath)
    {
        List<DialogueEntry> loadedDialogues = new List<DialogueEntry>();
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            // ���Խ� ����: ū����ǥ �ȿ� �ִ� ��ǥ�� �����ϰ� �и�
            string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";

            for (int i = 1; i < lines.Length; i++)
            {
                // ���Խ����� ���� �и�
                string[] columns = Regex.Split(lines[i], pattern);

                // ū����ǥ ���� �� Ʈ��
                for (int j = 0; j < columns.Length; j++)
                {
                    columns[j] = columns[j].Trim().Trim('"');
                }

                // �ּ� 7���� �÷� �ʿ�
                if (columns.Length >= 7)
                {
                    // ID�� Day �ʵ尡 �������� Ȯ��
                    if (int.TryParse(columns[0].Trim(), out int id) &&
                        int.TryParse(columns[1].Trim(), out int day))
                    {
                        string character = columns[2].Trim();
                        string text = columns[6].Trim();

                        // ���� ��� ID ó��
                        List<int> nextDialogueIds = new List<int>();
                        if (!string.IsNullOrWhiteSpace(columns[5]))
                        {
                            string nextDialogueField = columns[5].Trim().Replace("\"", "");
                            string[] idStrings = nextDialogueField.Split(',');

                            foreach (var idString in idStrings)
                            {
                                if (int.TryParse(idString.Trim(), out int nextId))
                                {
                                    nextDialogueIds.Add(nextId);
                                }
                                else
                                {
                                    Debug.LogWarning($"��ȿ���� ���� ���� ��� ID: '{idString.Trim()}' at line {i + 1}");
                                }
                            }
                        }

                        loadedDialogues.Add(new DialogueEntry(id, day, character, text, nextDialogueIds));
                    }
                    else
                    {
                        Debug.LogWarning($"ID �Ǵ� Day ���� ��ȿ���� �ʽ��ϴ�: '{columns[0].Trim()}', '{columns[1].Trim()}' at line {i + 1}");
                    }
                }
                else
                {
                    Debug.LogWarning($"������ ������ �ùٸ��� �ʽ��ϴ�. �ּ� 7���� �÷��� �ʿ��մϴ�. at line {i + 1}");
                }
            }
        }
        else
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�: " + filePath);
        }
        return loadedDialogues;
    }
}
