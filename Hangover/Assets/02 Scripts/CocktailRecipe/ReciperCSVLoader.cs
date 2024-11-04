using System.Collections.Generic;
using UnityEngine;

public class ReciperCSVLoader : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList = new List<Sprite>(); // 스프라이트 목록을 저장할 리스트
    [SerializeField] private TextAsset csvFile; // Resources에 있는 CSV 파일을 할당
    [SerializeField] private GameObject recipeItemPrefab; // RecipeItem 프리팹
    [SerializeField] private Transform recipeContainer; // RecipeItem 프리팹을 담을 부모 오브젝트
    

    void Start()
    {
        LoadSprites();
        LoadCSVData();
    }
    
    // Resources/Sprites 폴더의 스프라이트를 리스트에 로드
    private void LoadSprites()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Icons");
        spriteList.AddRange(sprites);
        Debug.Log("Loaded " + spriteList.Count + " sprites.");
    }
    
    private void LoadCSVData()
    {
        string[] rows = csvFile.text.Split('\n');

        for (int i = 0; i < rows.Length; i++) // 0부터 시작하여 헤더 건너뜀
        {
            string row = rows[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            string[] columns = row.Split(',');

            if (columns.Length < 10)
            {
                Debug.LogWarning($"행 {i + 1}의 데이터가 누락되었습니다.");
                continue;
            }

            // CSV 데이터 구성에 맞게 데이터 추출
            string iconIdx = columns[0];
            Sprite IconSprite = spriteList[int.Parse(iconIdx)-1];
            string name = columns[1];
            string sweet = columns[2];
            string sour = columns[3];
            string bitter = columns[4];
            string spice = columns[5];
            string spirit = columns[6];
            string alcoholContent = columns[7];
            string taste = columns[8];
            string description = columns[9];
            string method = columns[10];

            try
            {
                // RecipeItem 인스턴스 생성 및 설정
                GameObject recipeInstance = Instantiate(recipeItemPrefab, recipeContainer);
                RecipeItem recipeItem = recipeInstance.GetComponent<RecipeItem>();

                if (recipeItem == null)
                {
                    Debug.LogError("RecipeItem 컴포넌트를 찾을 수 없습니다.");
                    continue;
                }

                recipeItem.SetData(IconSprite, name, sweet, sour, bitter, spice, spirit, alcoholContent, taste, description, method);

                // 생성 확인을 위한 로그
                // Debug.Log($"RecipeItem 생성 완료: {name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"RecipeItem 생성 중 오류 발생 - 행 {i + 1}: {ex.Message}");
            }
        }
    }
    
}
