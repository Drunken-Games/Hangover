using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CocktailCheck : MonoBehaviour
{
    private Dictionary<int, Dictionary<string, float>> cocktailData;  // Ĭ���� ID�� ��� ������ ������ ��ųʸ�

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
            Debug.LogError("CSV ������ ã�� �� �����ϴ�: " + path);
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
                    Mathf.Abs(userIngredients[ingredient.Key] - ingredient.Value) > 0.01f) // ���� �� ��� ����
                {
                    FP = 1;
                    //return false; // ���� ����
                }

                // ��� ��� ���� (��: ��� ���� * Ư�� ��� ���)
                cost += ingredient.Value * 10; // 10�� ���� ���
            }
            if (FP == 1) return false;
            return true; // ���� ����
        }
        Debug.LogError("�ش� ID�� Ĭ������ �����ϴ�: " + cocktailId);
        return false;
    }
}
