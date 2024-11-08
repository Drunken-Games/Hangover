using System.Collections;
using System.Collections.Generic;
using System.Linq; // System.Linq 추가
using UnityEngine;

public class CocktailCal : MonoBehaviour
{
    public static CocktailCal instance;
    public List<CocktailData> cocktails; // cocktails 리스트 정의

    // 특정 칵테일 ID로 가격 가져오기
    public int GetCocktailPrice(int cocktailId)
    {
        if (cocktailId != -1)
        {
            for (int i = 0; i < cocktails.Count; i++)
                    {
                        Debug.Log($"Index {i}: id:{cocktails[i].id}, Name: {cocktails[i].cocktailName}, Price: {cocktails[i].price}, Sweet: {cocktails[i].sweet}, Sour: {cocktails[i].sour}, Bitter: {cocktails[i].bitter}, Spice: {cocktails[i].spice}, Spirit: {cocktails[i].spirit}, AlcoholContent: {cocktails[i].alcoholContent}, Taste: {cocktails[i].taste}, Description: {cocktails[i].description}, Method: {cocktails[i].method}");
                        if (cocktails[i].id == cocktailId)
                        {
                            return cocktails[i].price;
                        }
                    }
        }
        else
        {
            int[] cocktailParameters = GameManager.instance.GetCocktailParameters();
            int costma=CalculateMaterialCost(cocktailParameters);
            return costma + 30;        
        }
        
        return 0;
    }

    // 재료 비용 계산 메서드
    public int CalculateMaterialCost(int[] cocktailParameters)
    {
        int materialCost = 0;

        // 재료마다 10의 비용으로 가정하고 계산 (예시)
        for (int i = 0; i < cocktailParameters.Length - 1; i++) // 마지막 항목은 제조방법이므로 제외
        {
            if (i == 0)
            {
                materialCost += 1 * cocktailParameters[i];
            }
            else if (i == 1)
            {
                materialCost += 2 * cocktailParameters[i];
            }
            else if (i == 2)
            {
                materialCost += 3 * cocktailParameters[i];
            }
            else if (i == 3)
            {
                materialCost += 4 * cocktailParameters[i];
            }
            else
            {
                materialCost += 2 * cocktailParameters[i];
            }
        }

        return materialCost;
    }
}
