using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;

[System.Serializable]
public struct IngredientColorData
{
    public IngredientType type;
    public Color color;
}

public class BuildManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxContainerHeight = 500f;
    [SerializeField] private float initialContainerHeight = 100f;
    [SerializeField] private float itemMinHeight = 50f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private int progressiveGrowthLimit = 6;
    [SerializeField] private IngredientColorData[] ingredientColorData;
    
    [Header("References")]
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private VerticalLayoutGroup verticalLayout;
    [SerializeField] private IngredientButton[] ingredientButtons;
    
    [Header("Sound Settings")]
    [SerializeField] private float soundFadeInDuration = 0.2f;
    [SerializeField] private float soundFadeOutDuration = 0.3f;
    
    private Dictionary<IngredientType, Color> ingredientColors;
    private List<RectTransform> activeItems = new List<RectTransform>();
    private ObjectPool objectPool;
    private bool isProcessing = false;
    private float currentContainerHeight;
    private int currentMethod;
    private bool isPlayingSound = false;
    
    public event System.Action<int> OnIngredientsChanged;
    
    private void Awake()
    {
        objectPool = GetComponent<ObjectPool>();
        currentContainerHeight = initialContainerHeight;
        InitializeContainer();
        InitializeIngredientColors();
    }

    private void OnDisable()
    {
        StopPouringSound();
    }

    private void OnDestroy()
    {
        StopPouringSound();
    }
    
    private void InitializeContainer()
    {
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, initialContainerHeight);
        verticalLayout.childControlHeight = true;
        verticalLayout.childForceExpandHeight = true;
        verticalLayout.spacing = 0;
        
        if (TryGetComponent<ScrollRect>(out var scrollRect))
        {
            scrollRect.enabled = false;
        }
    }
    
    private void InitializeIngredientColors()
    {
        ingredientColors = new Dictionary<IngredientType, Color>();
        foreach (var data in ingredientColorData)
        {
            if (!ingredientColors.ContainsKey(data.type))
            {
                ingredientColors.Add(data.type, data.color);
            }
        }
    }       
    
    public bool AddIngredient(IngredientType ingredient)
    {
        if (isProcessing || objectPool == null) return false;
        
        isProcessing = true;
        PlayPouringSound();
        
        GameObject newItem = objectPool.GetPooledObject();
        if (newItem == null) return false;

        RectTransform itemRect = newItem.GetComponent<RectTransform>();
        IngredientLayer ingredientLayer = newItem.GetComponent<IngredientLayer>();
        
        ingredientLayer.SetIngredientType(ingredient);
        
        newItem.SetActive(false);
        newItem.transform.SetParent(containerRect);
        newItem.transform.localScale = Vector3.one;
        
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(0, currentContainerHeight / (activeItems.Count + 1));
        
        newItem.SetActive(true);
        activeItems.Add(itemRect);
        
        UpdateContainerSize();
        
        if (ingredientColors.TryGetValue(ingredient, out Color color))
        {
            ingredientLayer.SetColor(color);
        }
        
        AnimateNewItem(itemRect);
        
        DOVirtual.DelayedCall(animationDuration, () => {
            isProcessing = false;
            StopPouringSound();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            OnIngredientsChanged?.Invoke(activeItems.Count);
        });
        
        return true;
    }

    private void PlayPouringSound()
    {
        if (!isPlayingSound && SoundsManager.instance != null)
        {
            SoundsManager.instance.PlaySFXWithFade("pouring", soundFadeInDuration);
            isPlayingSound = true;
        }
    }

    private void StopPouringSound()
    {
        if (isPlayingSound && SoundsManager.instance != null)
        {
            SoundsManager.instance.StopSFXWithFade("pouring", soundFadeOutDuration);
            isPlayingSound = false;
        }
    }
    
    private void UpdateContainerSize()
    {
        if (activeItems.Count <= progressiveGrowthLimit)
        {
            float targetHeight = Mathf.Lerp(
                initialContainerHeight, 
                maxContainerHeight, 
                (float)activeItems.Count / progressiveGrowthLimit
            );
            
            containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, currentContainerHeight);
            containerRect.DOSizeDelta(
                new Vector2(containerRect.sizeDelta.x, targetHeight),
                animationDuration
            ).SetEase(Ease.OutQuad);
            
            currentContainerHeight = targetHeight;
        }
    }
    
    private void AnimateNewItem(RectTransform itemRect)
    {
        itemRect.localScale = Vector3.zero;
        
        itemRect.DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack);
    }
    
    public void ResetContainer()
    {
        if (isProcessing) return;
        isProcessing = true;

        DOTween.Kill(containerRect);
        foreach (var item in activeItems)
        {
            if (item != null)
            {
                DOTween.Kill(item);
            }
        }
        
        containerRect.DOSizeDelta(
            new Vector2(containerRect.sizeDelta.x, initialContainerHeight),
            animationDuration
        ).SetEase(Ease.InQuad);
        
        foreach (var itemRect in activeItems.ToArray())
        {
            if (itemRect != null && itemRect.gameObject.activeInHierarchy)
            {
                var ingredientLayer = itemRect.GetComponent<IngredientLayer>();
                if (ingredientLayer != null)
                {
                    ingredientLayer.PlayRemoveAnimation();
                }
            }
        }
        
        foreach (var button in ingredientButtons)
        {
            if (button != null)
            {
                button.ResetButton();
            }
        }

        activeItems.Clear();
        currentContainerHeight = initialContainerHeight;
        
        DOVirtual.DelayedCall(animationDuration + 0.1f, () => {
            foreach (Transform child in containerRect)
            {
                child.gameObject.SetActive(false);
            }
            
            objectPool.ResetAllObjects();
            isProcessing = false;
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            OnIngredientsChanged?.Invoke(0);
        });
    }
    
    public Color GetIngredientColor(IngredientType type)
    {
        if (ingredientColors != null && ingredientColors.TryGetValue(type, out Color color))
        {
            return color;
        }
        return Color.white;
    }

    public void SetMakingMethod(string value)
    {
        if (value == "Build")
            currentMethod = 0;
        else if (value == "Stir")
            currentMethod = 1;
        else if (value == "Shake")
            currentMethod = 2;
        else if (value == "Blend")
            currentMethod = 3;
    }

    public int[] GetParameters()
    {
        int[] parameters = new int[6];
        
        foreach (var itemRect in activeItems)
        {
            if (itemRect != null && itemRect.gameObject.activeInHierarchy)
            {
                IngredientLayer ingredientLayer = itemRect.GetComponent<IngredientLayer>();
                if (ingredientLayer != null)
                {
                    int typeIndex = (int)ingredientLayer.GetIngredientType();
                    if (typeIndex >= 0 && typeIndex < 5)
                    {
                        parameters[typeIndex]++;
                    }
                }
            }
        }
        
        parameters[5] = (int)currentMethod;
        
        return parameters;
    }
}