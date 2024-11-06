#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;

public class DialogueDataImporter : EditorWindow
{
    private string filePath;

    [MenuItem("Tools/Dialogue Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<DialogueDataImporter>("Dialogue Data Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV 파일을 불러와 DialogueData1 생성", EditorStyles.boldLabel);
        filePath = EditorGUILayout.TextField("CSV 파일 경로", filePath);

        if (GUILayout.Button("Import CSV"))
        {
            ImportCsvToDialogueData(filePath);
        }
    }

    private static void ImportCsvToDialogueData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + filePath);
            return;
        }

        // DialogueDatabase 컨테이너 생성
        DialogueDatabase database = ScriptableObject.CreateInstance<DialogueDatabase>();

        // Database 전체를 하나의 에셋 파일로 먼저 생성
        string assetPath = "Assets/DialogueDataAssets/DialogueDatabase.asset";
        AssetDatabase.CreateAsset(database, assetPath);
        AssetDatabase.SaveAssets();

        string[] lines = File.ReadAllLines(filePath);
        string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = Regex.Split(lines[i], pattern);
            for (int j = 0; j < columns.Length; j++)
            {
                columns[j] = columns[j].Trim().Trim('"');
            }

            // 각 필드에 대한 기본값 설정
            int id = int.TryParse(columns[0], out int parsedId) ? parsedId : -1;
            int day = int.TryParse(columns[1], out int parsedDay) ? parsedDay : -1;
            string character = !string.IsNullOrWhiteSpace(columns[2]) ? columns[2] : "Unknown";
            string text = !string.IsNullOrWhiteSpace(columns[6]) ? columns[6] : "내용 없음";
            int cocktailId = int.TryParse(columns[7], out int parsedCocktailId) ? parsedCocktailId : -1;

            // 다음 대화 ID 리스트 설정, 비어있으면 -1 추가
            List<int> nextDialogueIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(columns[5]))
            {
                string[] nextIds = columns[5].Split(',');
                foreach (string idStr in nextIds)
                {
                    if (int.TryParse(idStr.Trim(), out int nextId))
                    {
                        nextDialogueIds.Add(nextId);
                    }
                    else
                    {
                        nextDialogueIds.Add(-1);
                    }
                }
            }
            else
            {
                nextDialogueIds.Add(-1);
            }

            // DialogueData 생성 및 Database에 추가
            DialogueData1 dialogueData = CreateDialogueData(id, day, character, text, cocktailId, nextDialogueIds);
            database.dialogues.Add(dialogueData);

            // Database 에셋에 개별 DialogueData 추가
            AssetDatabase.AddObjectToAsset(dialogueData, database);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("하나의 대화 데이터베이스 에셋 파일이 생성되었습니다.");
    }


    private static DialogueData1 CreateDialogueData(int id, int day, string character, string text, int cocktailId, List<int> nextDialogueIds)
    {
        DialogueData1 dialogueData = ScriptableObject.CreateInstance<DialogueData1>();
        dialogueData.name = $"Dialogue_{id}";

        // Reflection을 사용하여 private 필드에 값 할당
        typeof(DialogueData1).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dialogueData, id);
        typeof(DialogueData1).GetField("day", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dialogueData, day);
        typeof(DialogueData1).GetField("character", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dialogueData, character);
        typeof(DialogueData1).GetField("text", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dialogueData, text);
        typeof(DialogueData1).GetField("cocktailId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dialogueData, cocktailId);
        typeof(DialogueData1).GetField("nextDialogueIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dialogueData, nextDialogueIds);

        return dialogueData;
    }
}
//1