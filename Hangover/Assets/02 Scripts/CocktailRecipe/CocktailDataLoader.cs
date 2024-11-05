using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CocktailDataLoader : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList = new List<Sprite>();
    [SerializeField] private TextAsset csvFile;
    [SerializeField] private GameObject recipeItemPrefab;
    [SerializeField] private Transform recipeContainer;
    [SerializeField] private CocktailDatabase cocktailDatabase; // 레퍼런스 추가

    void Start()
    {
        LoadSprites();
        InstantiateCocktailItems();
    }

    private void LoadSprites()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Icons");
        spriteList.AddRange(sprites);
        Debug.Log("Loaded " + spriteList.Count + " sprites.");
    }

    private void InstantiateCocktailItems()
    {
        foreach (var cocktailData in cocktailDatabase.cocktails)
        {
            try
            {
                GameObject recipeInstance = Instantiate(recipeItemPrefab, recipeContainer);
                RecipeItem recipeItem = recipeInstance.GetComponent<RecipeItem>();

                if (recipeItem == null)
                {
                    Debug.LogError("RecipeItem 컴포넌트를 찾을 수 없습니다.");
                    continue;
                }

                recipeItem.SetData(
                    cocktailData.icon,
                    cocktailData.cocktailName,
                    cocktailData.sweet.ToString(),
                    cocktailData.sour.ToString(),
                    cocktailData.bitter.ToString(),
                    cocktailData.spice.ToString(),
                    cocktailData.spirit.ToString(),
                    cocktailData.alcoholContent,
                    cocktailData.taste,
                    cocktailData.description,
                    cocktailData.method.ToString()
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"RecipeItem 생성 중 오류 발생 - {cocktailData.cocktailName}: {ex.Message}");
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Cocktail Database From CSV")]
    private void GenerateCocktailDatabase()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일이 할당되지 않았습니다.");
            return;
        }

        CocktailDatabase database = ScriptableObject.CreateInstance<CocktailDatabase>();
        string[] rows = csvFile.text.Split('\n');

        foreach (string row in rows)
        {
            string trimmedRow = row.Trim();
            if (string.IsNullOrEmpty(trimmedRow)) continue;

            string[] columns = trimmedRow.Split(',');
            if (columns.Length < 11) continue;

            CocktailData cocktailData = ScriptableObject.CreateInstance<CocktailData>();
            
            cocktailData.icon = spriteList[int.Parse(columns[0])-1];
            cocktailData.cocktailName = columns[1];
            cocktailData.sweet = int.Parse(columns[2]);
            cocktailData.sour = int.Parse(columns[3]);
            cocktailData.bitter = int.Parse(columns[4]);
            cocktailData.spice = int.Parse(columns[5]);
            cocktailData.spirit = int.Parse(columns[6]);
            cocktailData.alcoholContent = columns[7];
            cocktailData.taste = columns[8];
            cocktailData.description = columns[9];
            cocktailData.method = int.Parse(columns[10]);

            database.cocktails.Add(cocktailData);
        }

        AssetDatabase.CreateAsset(database, "Assets/04 Resources/CocktailData/CocktailDatabase.asset");
        
        // 개별 CocktailData 에셋들도 저장
        foreach (var cocktail in database.cocktails)
        {
            AssetDatabase.AddObjectToAsset(cocktail, database);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("칵테일 데이터베이스가 생성되었습니다.");
    }
#endif
}