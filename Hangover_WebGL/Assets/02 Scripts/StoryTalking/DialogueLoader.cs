using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

// 대화 데이터를 저장할 구조체 정의
[System.Serializable]
public struct DialogueEntry
{
    public int id;                  // 대화 ID
    public int day;                 // 현재 Day
    public string character;        // 캐릭터 이름
    public string text;             // 대화 텍스트
    public List<int> cocktailIds;   // 칵테일 ID 리스트
    public List<int> nextDialogueIds; // 다음 대화 ID 리스트 (선택지 포함)

    public DialogueEntry(int id, int day, string character, string text, List<int> cocktailIds,  List<int> nextDialogueIds)
    {
        this.id = id;
        this.day = day;
        this.character = character;
        this.text = text;
        this.cocktailIds = cocktailIds; // null 확인 후 초기화
        this.nextDialogueIds = nextDialogueIds;
    }
}

public class DialogueLoader
{
    // CSV 파일에서 대화 데이터를 불러오는 메서드
    public static List<DialogueEntry> LoadDialogues(string filePath)
    {
        List<DialogueEntry> loadedDialogues = new List<DialogueEntry>();
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            // CSV 구분자 패턴: 큰따옴표 안에 있는 콤마를 무시하고 구분
            string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";

            for (int i = 1; i < lines.Length; i++)
            {
                // 각 줄을 분할
                string[] columns = Regex.Split(lines[i], pattern);

                // 큰따옴표 제거 후 공백 제거
                for (int j = 0; j < columns.Length; j++)
                {
                    columns[j] = columns[j].Trim().Trim('"');
                }

                // 최소 7개의 열이 필요
                if (columns.Length >= 8)
                {
                    // ID와 Day 필드가 유효한지 확인
                    if (int.TryParse(columns[0].Trim(), out int id) &&
                        int.TryParse(columns[1].Trim(), out int day))
                    {
                        string character = columns[2].Trim();
                        string text = columns[6].Trim();

                        // 칵테일 ID 파싱
                        List<int> cocktailIds = new List<int>();
                        if (!string.IsNullOrEmpty(columns[7].Trim()))
                        {
                            string[] cocktailIdStrings = columns[7].Split(',');
                            foreach (var idString in cocktailIdStrings)
                            {
                                if (int.TryParse(idString.Trim(), out int parsedCocktailId))
                                {
                                    cocktailIds.Add(parsedCocktailId);
                                }
                                else
                                {
                                    Debug.LogWarning($"유효하지 않은 칵테일 ID: '{idString.Trim()}' at line {i + 1}");
                                }
                            }
                        }

                        // 다음 대화 ID 처리
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
                                    Debug.LogWarning($"유효하지 않은 다음 대화 ID: '{idString.Trim()}' at line {i + 1}");
                                }
                            }
                        }

                        loadedDialogues.Add(new DialogueEntry(id, day, character, text, cocktailIds, nextDialogueIds));
                    }
                    else
                    {
                        Debug.LogWarning($"ID 또는 Day 값이 유효하지 않습니다: '{columns[0].Trim()}', '{columns[1].Trim()}' at line {i + 1}");
                    }
                }
                else
                {
                    Debug.LogWarning($"데이터 형식이 올바르지 않습니다. 최소 8개의 열이 필요합니다. at line {i + 1}");
                }
            }
        }
        else
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + filePath);
        }

        // 로드된 대화 로그 확인용
        foreach (var dialogue in loadedDialogues)
        {
            Debug.Log($"ID: {dialogue.id}, Day: {dialogue.day}, Character: {dialogue.character}, Text: {dialogue.text}, Cocktail IDs: {string.Join(", ", dialogue.cocktailIds)}, Next IDs: {string.Join(", ", dialogue.nextDialogueIds)}");
        }

        return loadedDialogues;
    }
}
