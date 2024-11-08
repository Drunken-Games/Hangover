using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class CreaftingResultManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform resultSpawnPoint;
    [SerializeField] private GameObject[] successPrefab;
    [SerializeField] private GameObject failurePrefab;
    [SerializeField] private TextMeshProUGUI cocktailName;
    [SerializeField] private TextAsset cocktailRecipeCSV;
     
    [Header("Animation Settings")]
    [SerializeField] private float spawnAnimationDuration = 1f;
    [SerializeField] private float rotationSpeed = 30f;
    
    private Dictionary<int, (string name, int[] ingredients, int mixMethod)> recipeData 
        = new Dictionary<int, (string name, int[] ingredients, int mixMethod)>();
    private GameManager gameManager;
    private GameObject currentResultObject;
    private bool isAnimating = false;

    private void Start()
    {
        Initialize();
        LoadRecipes();
        CheckResult();
    }

    private void Initialize()
    {
        gameManager = GameManager.instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }
    }

    private void LoadRecipes()
    {
        if (cocktailRecipeCSV == null)
        {
            Debug.LogError("Recipe CSV file not assigned!");
            return;
        }

        string[] lines = cocktailRecipeCSV.text.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            string[] data = line.Split(',');
            if (data.Length < 11) continue; // ID부터 제조방법까지 총 11개 컬럼 필요

            try 
            {
                int id = int.Parse(data[0]);
                string name = data[1].Trim();
                
                int[] ingredients = new int[5]
                {
                    int.Parse(data[2]), // sweet
                    int.Parse(data[3]), // sour
                    int.Parse(data[4]), // bitter
                    int.Parse(data[5]), // spicy
                    int.Parse(data[6])  // spirit
                };

                int mixMethod = int.Parse(data[10].Trim()); // 마지막 컬럼의 제조방법

                recipeData.Add(id, (name, ingredients, mixMethod));
                Debug.Log($"Loaded recipe: {id}, {name}, Method: {mixMethod}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing line: {line}\nError: {e.Message}");
                continue;
            }
        }
    }

    private void CheckResult()
    {
        if (isAnimating) return;

        int[] parameters = gameManager.GetCocktailParameters();
        LogCurrentParameters(parameters);
        
        var matchedRecipe = FindMatchingRecipe(parameters);

        if (matchedRecipe.HasValue)
        {
            SpawnResultObject(true, matchedRecipe.Value);
            Debug.Log($"Successfully created: {recipeData[matchedRecipe.Value].name}");
        }
        else
        {
            SpawnResultObject(false, -1);
            Debug.Log("No matching recipe found");
        }
    }

    private void LogCurrentParameters(int[] parameters)
    {
        Debug.Log($"Current parameters - Sweet: {parameters[0]}, Sour: {parameters[1]}, " +
                  $"Bitter: {parameters[2]}, Spicy: {parameters[3]}, Spirit: {parameters[4]}, " +
                  $"Method: {parameters[5]}");
    }

    private int? FindMatchingRecipe(int[] parameters)
    {
        foreach (var recipe in recipeData)
        {
            bool ingredientsMatch = true;
            for (int i = 0; i < 5; i++)
            {
                if (recipe.Value.ingredients[i] != parameters[i])
                {
                    ingredientsMatch = false;
                    break;
                }
            }

            if (ingredientsMatch && recipe.Value.mixMethod == parameters[5])
            {
                return recipe.Key;
            }
        }
        return null;
    }

    private void SpawnResultObject(bool isSuccess, int recipeId)
    {
        if (isAnimating) return;
        isAnimating = true;

        if (currentResultObject != null)
        {
            Destroy(currentResultObject);
        }

        // 시작 위치와 회전 설정
        Vector3 startPosition = resultSpawnPoint.position + new Vector3(0, -2, 0);
        Quaternion startRotation = Quaternion.Euler(15, -200, 0);

        if (isSuccess)
        {
            currentResultObject = Instantiate(successPrefab[recipeId-1], startPosition, startRotation);
            cocktailName.text = recipeData[recipeId].name;
            gameManager.SetRecipeId(recipeId);
        }
        else
        {
            currentResultObject = Instantiate(failurePrefab, startPosition, startRotation);
            cocktailName.text = "민트초코 실론티 지코";
            gameManager.SetRecipeId(recipeId);
        }
    
        currentResultObject.transform.SetParent(resultSpawnPoint);
    
        // 초기 스케일을 0으로 설정
        currentResultObject.transform.localScale = Vector3.zero;

        // 애니메이션 시퀀스
        Sequence spawnSequence = DOTween.Sequence();
    
        // 목표 스케일을 20으로 설정
        Vector3 targetScale = Vector3.one * 20f;
    
        spawnSequence.Append(currentResultObject.transform
            .DOScale(targetScale, spawnAnimationDuration)
            .SetEase(Ease.OutBack));

        if (rotationSpeed > 0)
        {
            // Y축 회전만 적용
            currentResultObject.transform
                .DORotate(new Vector3(15, 360, 0), rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        spawnSequence.OnComplete(() => {
            isAnimating = false;
        });
    }

    // 디버깅을 위한 메소드
    public void PrintCurrentRecipes()
    {
        foreach (var recipe in recipeData)
        {
            Debug.Log($"ID: {recipe.Key}, Name: {recipe.Value.name}, " +
                     $"Method: {recipe.Value.mixMethod}, " +
                     $"Ingredients: {string.Join(", ", recipe.Value.ingredients)}");
        }
    }

    private void OnDestroy()
    {
        if (currentResultObject != null)
        {
            DOTween.Kill(currentResultObject.transform);
        }
    }
}