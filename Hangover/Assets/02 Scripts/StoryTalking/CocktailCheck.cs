using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CocktailCheck : MonoBehaviour
{
    private Dictionary<int, Dictionary<string, float>> cocktailData;  // 칵테일 ID와 재료 비율을 저장할 딕셔너리

    // Start is called before the first frame update
    void Start()
    {
        LoadCocktailData();
    }

    // Update is called once per frame
    void Update()
    {

    }

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
    public bool CheckIngredients(int cocktailId, Dictionary<string, float> userIngredients, out float cost)
    {
        cost = 0f;

        if (cocktailData.ContainsKey(cocktailId))
        {
            var requiredIngredients = cocktailData[cocktailId];
            var FP = 0;
            foreach (var ingredient in requiredIngredients)
            {
                if (!userIngredients.ContainsKey(ingredient.Key) ||
                    Mathf.Abs(userIngredients[ingredient.Key] - ingredient.Value) > 0.01f) // 비율 비교 허용 오차
                {
                    FP = 1;
                    //return false; // 실패 판정
                }

                // 비용 계산 예시 (예: 재료 비율 * 특정 비용 상수)
                cost += ingredient.Value * 10; // 10은 예시 상수
            }
            if (FP == 1) return false;
            return true; // 성공 판정
        }
        Debug.LogError("해당 ID의 칵테일이 없습니다: " + cocktailId);
        return false;
    }
}
