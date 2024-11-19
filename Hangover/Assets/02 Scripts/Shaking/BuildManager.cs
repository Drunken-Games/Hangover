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
    [SerializeField] private IngredientButton[] ingredientButtons; // 버튼 배열 추가
    
    private Dictionary<IngredientType, Color> ingredientColors;
    private List<RectTransform> activeItems = new List<RectTransform>();
    private ObjectPool objectPool;
    private bool isProcessing = false;
    private float currentContainerHeight;
    private int currentMethod;
    
    private void Awake()
    {
        objectPool = GetComponent<ObjectPool>();
        currentContainerHeight = initialContainerHeight;
        InitializeContainer();
        InitializeIngredientColors();
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
        
        GameObject newItem = objectPool.GetPooledObject();
        if (newItem == null) return false;

        RectTransform itemRect = newItem.GetComponent<RectTransform>();
        IngredientLayer ingredientLayer = newItem.GetComponent<IngredientLayer>();
        
        // 재료 타입 설정 추가
        ingredientLayer.SetIngredientType(ingredient);
        
        // 아이템을 비활성화된 상태로 부모 설정
        newItem.SetActive(false);
        newItem.transform.SetParent(containerRect);
        newItem.transform.localScale = Vector3.one;
        
        // 기본 위치와 크기 설정
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(0, currentContainerHeight / (activeItems.Count + 1));
        
        // 이제 활성화
        newItem.SetActive(true);
        activeItems.Add(itemRect);
        
        // 컨테이너 크기 조정
        UpdateContainerSize();
        
        // 색상 설정 및 애니메이션
        if (ingredientColors.TryGetValue(ingredient, out Color color))
        {
            ingredientLayer.SetColor(color);
        }
        
        AnimateNewItem(itemRect);
        
        DOVirtual.DelayedCall(animationDuration, () => {
            isProcessing = false;
            // 레이아웃 갱신
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        });
        
        return true;
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
            
            // 컨테이너 크기 즉시 설정 후 애니메이션
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
        // 시작 상태 설정
        itemRect.localScale = Vector3.zero;
        
        // 스케일 애니메이션
        itemRect.DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack);
    }
    
    public void ResetContainer()
    {
        if (isProcessing) return;
        isProcessing = true;

        // 진행 중인 모든 애니메이션 중지
        DOTween.Kill(containerRect);
        foreach (var item in activeItems)
        {
            if (item != null)
            {
                DOTween.Kill(item);
            }
        }
        
        // 컨테이너 크기 초기화
        containerRect.DOSizeDelta(
            new Vector2(containerRect.sizeDelta.x, initialContainerHeight),
            animationDuration
        ).SetEase(Ease.InQuad);
        
        // 아이템 제거
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
        
        // 모든 버튼 리셋
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
        });
    }
    
    public Color GetIngredientColor(IngredientType type)
    {
        if (ingredientColors != null && ingredientColors.TryGetValue(type, out Color color))
        {
            return color;
        }
        return Color.white; // 기본 색상
    }

    // 제조 방법 설정
    public void SetMakingMethod(string value)
    {
        if(value == "Build")
            currentMethod = 0;
        else if (value == "Stir")
            currentMethod = 1;
        else if(value == "Shake")
            currentMethod = 2;
    }

    // 파라미터 가져오기
    public int[] GetParameters()
    {
        int[] parameters = new int[6]; // 5개 재료 + 1개 제조방법
        
        // 재료 카운트
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
        
        // 제조 방법 저장
        parameters[5] = (int)currentMethod;
        
        return parameters;
    }
}