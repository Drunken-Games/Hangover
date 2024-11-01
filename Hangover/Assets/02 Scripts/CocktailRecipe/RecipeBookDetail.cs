using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecipeBookDetail : MonoBehaviour
{
    [SerializeField] private TMP_Text CocktailNameText;
    [SerializeField] private TMP_Text CocktailAlcoholContentText;
    [SerializeField] private TMP_Text CocktailTasteText;
    [SerializeField] private TMP_Text CocktailDescriptionText;
    


    public void RecipeDetailSetData(string CocktailName, string CoctailAlcoholContent, string CoctailTaste, string CocktailDescription)
    {
        CocktailNameText.text = CocktailName;
        CocktailAlcoholContentText.text = $"도수: {CoctailAlcoholContent}";
        CocktailTasteText.text = $"맛: {CoctailTaste}";
        CocktailDescriptionText.text = $"설명: {CocktailDescription}";
    }
}
