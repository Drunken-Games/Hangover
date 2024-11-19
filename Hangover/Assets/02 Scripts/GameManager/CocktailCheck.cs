using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CocktailCheck : MonoBehaviour
{
    private Dictionary<int, Dictionary<string, float>> cocktailData;

    void Start()
    {
        LoadCocktailData();
    }

    // CSV 파일에서 칵테일 데이터를 불러오는 메서드
    public void LoadCocktailData()
    {
        cocktailData = new Dictionary<int, Dictionary<string, float>>();
        string path = "Assets/04 Resources/CocktailData/CocktailList.csv";

        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] fields = line.Split(',');
                int cocktailId = int.Parse(fields[0]);
                string ingredient = fields[1];
                float ratio = float.Parse(fields[2]);

                if (!cocktailData.ContainsKey(cocktailId))
                    cocktailData[cocktailId] = new Dictionary<string, float>();

                cocktailData[cocktailId][ingredient] = ratio;
            }
        }
        else
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + path);
        }
    }

    public int FindMatchingCocktail(int[] cocktailParameters)
    {
        foreach (var cocktail in cocktailData)
        {
            int cocktailId = cocktail.Key;
            Dictionary<string, float> ingredients = cocktail.Value;

            bool match = true;
            int[] parameters = new int[ingredients.Count];
            int i = 0;
            foreach (var ingredient in ingredients)
            {
                parameters[i] = (int)ingredient.Value;
                if (cocktailParameters.Length > i && parameters[i] != cocktailParameters[i])
                {
                    match = false;
                    break;
                }
                i++;
            }

            if (match)
            {
                return cocktailId;
            }
        }
        return -1; // 일치하는 칵테일을 찾을 수 없는 경우
    }

}
