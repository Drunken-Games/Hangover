using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecipeItem : MonoBehaviour
{
    [Header("Cocktail Info Settings")]
    [SerializeField] private Image CocktailImage;
    [SerializeField] private TMP_Text CoctailNameText;
    [SerializeField] private TMP_Text CoctailMethodText;
    [SerializeField] private string CocktailName;
    [SerializeField] private string CocktailAlcoholContent;
    [SerializeField] private string CocktailTaste;
    [SerializeField] private string CocktailDescription;
    
    // 게이지 프리팹 및 컨테이너
    [Header("Cocktail Guage & Description Prefabs")]
    [SerializeField] private GameObject CocktailDesctiptionPrefab;
    [SerializeField] private GameObject OrangeGageItemPrefab;
    [SerializeField] private Transform OrangeGageContainer;
    [SerializeField] private GameObject LightGreenGageItemPrefab;
    [SerializeField] private Transform LightGreenGageContainer;
    [SerializeField] private GameObject GreenGageItemPrefab;
    [SerializeField] private Transform GreenGageContainer;
    [SerializeField] private GameObject PinkGageItemPrefab;
    [SerializeField] private Transform PinkGageContainer;
    [SerializeField] private GameObject BlueGageItemPrefab;
    [SerializeField] private Transform BlueGageContainer;
    

    public void SetData(Sprite iconSprite, string name, string sweet, string sour, string bitter, string spice, string spirit, string alcoholContent, string taste, string description, string  method)
    {
        List<string> MethodList = new List<string> { "빌드", "젓기", "쉐이킹", "블랜딩" };

        
        
        CocktailImage.sprite = iconSprite;
        CoctailNameText.text = $"{name} ({alcoholContent}%)";
        CoctailMethodText.text = MethodList[int.Parse(method)];

        CocktailName = name;
        CocktailAlcoholContent = alcoholContent;
        CocktailTaste = taste;
        CocktailDescription = description;

        ClearContainer(OrangeGageContainer);
        ClearContainer(LightGreenGageContainer);
        ClearContainer(GreenGageContainer);
        ClearContainer(PinkGageContainer);
        ClearContainer(BlueGageContainer);

        StartCoroutine(InstantiateGagesWithDelay(OrangeGageItemPrefab, OrangeGageContainer, sweet));
        StartCoroutine(InstantiateGagesWithDelay(LightGreenGageItemPrefab, LightGreenGageContainer, sour));
        StartCoroutine(InstantiateGagesWithDelay(GreenGageItemPrefab, GreenGageContainer, bitter));
        StartCoroutine(InstantiateGagesWithDelay(PinkGageItemPrefab, PinkGageContainer, spice));
        StartCoroutine(InstantiateGagesWithDelay(BlueGageItemPrefab, BlueGageContainer, spirit));
    }

    public void OpenDescription()
    {
        GameObject rootParent = GetRootParent(this.gameObject);
        Debug.Log("Root Parent: " + rootParent.name);
        
        GameObject CocktailDetailInstance = Instantiate(CocktailDesctiptionPrefab, rootParent.transform);
        RecipeBookDetail recipeBookDetail = CocktailDetailInstance.GetComponent<RecipeBookDetail>();

        recipeBookDetail.RecipeDetailSetData(CocktailName, CocktailAlcoholContent, CocktailTaste, CocktailDescription);
        // 생성 확인을 위한 로그
        Debug.Log($"RecipeItem 생성 완료: {CocktailName}");

    }

    private IEnumerator InstantiateGagesWithDelay(GameObject prefab, Transform container, string value)
    {
        if (int.TryParse(value, out int count))
        {
            for (int i = 0; i < count; i++)
            {
                Instantiate(prefab, container);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            Debug.LogWarning($"Invalid value for gauge: {value}");
        }
    }
    
    GameObject GetRootParent(GameObject child)
    {
        Transform currentParent = child.transform;

        while (currentParent.parent != null)
        {
            if (currentParent.name == "RecipeUICanvas")
            {
                return currentParent.gameObject;
            }
            currentParent = currentParent.parent;
        }

        return currentParent.gameObject;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
