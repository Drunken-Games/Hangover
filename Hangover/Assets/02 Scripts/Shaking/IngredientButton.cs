using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class IngredientButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private IngredientType ingredientType;
    [SerializeField] private int maxClicks = 5;
    
    [Header("UI References")]
    [SerializeField] private List<Image> counterImages;
    [SerializeField] private float colorChangeDuration = 0.3f;
    
    private Button button;
    private BuildManager buildManager;
    private int remainingClicks;
    private Color defaultColor;
    private Color ingredientColor;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        remainingClicks = maxClicks;
        
        // A5A5A5 색상 설정 (알파값 1 유지)
        defaultColor = new Color(0.647f, 0.647f, 0.647f, 1f);
        
        if (counterImages.Count != maxClicks)
        {
            Debug.LogWarning($"Counter images count ({counterImages.Count}) doesn't match maxClicks ({maxClicks})");
        }
        
        // 모든 카운터 이미지를 기본 색상으로 초기화
        foreach (var image in counterImages)
        {
            if (image != null)
            {
                image.color = defaultColor;
            }
        }
    }
    
    private void Start()
    {
        buildManager = FindObjectOfType<BuildManager>();
        button.onClick.AddListener(OnClick);
        
        // 재료 색상 가져오기 (알파값 1로 설정)
        Color baseColor = buildManager.GetIngredientColor(ingredientType);
        ingredientColor = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        
        UpdateButtonState();
    }
    
    private void OnClick()
    {
        if (remainingClicks <= 0 || buildManager == null) return;
        
        if (buildManager.AddIngredient(ingredientType))
        {
            int usedCount = maxClicks - remainingClicks;
            if (usedCount < counterImages.Count && counterImages[usedCount] != null)
            {
                // 알파값을 1로 유지하면서 색상 변경
                Image targetImage = counterImages[usedCount];
                Color currentColor = targetImage.color;
                
                targetImage.DOColor(new Color(
                    ingredientColor.r,
                    ingredientColor.g,
                    ingredientColor.b,
                    currentColor.a  // 현재 알파값 유지
                ), colorChangeDuration).SetEase(Ease.OutQuad);
            }
            
            remainingClicks--;
            UpdateButtonState();
        }
    }
    
    private void UpdateButtonState()
    {
        button.interactable = remainingClicks > 0;
    }
    
    private void ResetCounterColors()
    {
        foreach (var image in counterImages)
        {
            if (image != null)
            {
                // 진행 중인 애니메이션 중지
                DOTween.Kill(image);
                
                // 알파값을 유지하면서 기본 색상으로 변경
                Color currentColor = image.color;
                image.DOColor(new Color(
                    defaultColor.r,
                    defaultColor.g,
                    defaultColor.b,
                    currentColor.a  // 현재 알파값 유지
                ), colorChangeDuration).SetEase(Ease.OutQuad);
            }
        }
    }
    
    public void ResetButton()
    {
        remainingClicks = maxClicks;
        ResetCounterColors();
        UpdateButtonState();
    }
}