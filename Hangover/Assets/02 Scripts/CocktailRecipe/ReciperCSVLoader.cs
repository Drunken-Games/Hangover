using System.Collections.Generic;
using UnityEngine;

public class ReciperCSVLoader : MonoBehaviour
{
    [SerializeField] private CocktailDatabase cocktailDatabase; // ScriptableObject 참조
    [SerializeField] private GameObject recipeItemPrefab;
    [SerializeField] private Transform recipeContainer;

    void Start()
    {
        LoadRecipeData();
    }

    private void LoadRecipeData()
    {
        foreach (var cocktailData in cocktailDatabase.cocktails)
        {
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

                // ScriptableObject의 데이터를 사용하여 RecipeItem 설정
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

                // 생성 확인을 위한 로그
                // Debug.Log($"RecipeItem 생성 완료: {cocktailData.cocktailName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"RecipeItem 생성 중 오류 발생: {ex.Message}");
            }
        }
    }
}