using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IngredientLayer : MonoBehaviour
{
    private Image image;
    private RectTransform rectTransform;
    private IngredientType ingredientType; // 재료 타입 저장용 변수 추가
    
    private void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
        }
        
        rectTransform = GetComponent<RectTransform>();
        InitializeImage();
    }
    
    private void InitializeImage()
    {
        image.color = Color.white;
        
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = Vector2.zero;
    }

    // 재료 타입 설정 메서드 추가
    public void SetIngredientType(IngredientType type)
    {
        ingredientType = type;
    }

    // 재료 타입 가져오는 메서드 추가
    public IngredientType GetIngredientType()
    {
        return ingredientType;
    }
    
    public void SetColor(Color newColor)
    {
        if (image != null)
        {
            image.color = new Color(newColor.r, newColor.g, newColor.b, 0);
            image.DOFade(1f, 0.5f);
        }
    }
    
    public void SetPosition(Vector2 position)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position + Vector2.down * 100f;
            rectTransform.DOAnchorPos(position, 0.5f).SetEase(Ease.OutBack);
        }
    }
    
    public void SetScale(Vector2 scale)
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.zero;
            rectTransform.DOScale(new Vector3(1, 1, 1), 0.5f).SetEase(Ease.OutBounce);
            rectTransform.sizeDelta = scale;
        }
    }
    
    public void PlayRemoveAnimation()
    {
        if (rectTransform != null)
        {
            rectTransform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360);
            
            Sequence sequence = DOTween.Sequence();
            sequence.Join(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y - 100f, 0.5f).SetEase(Ease.InBack));
            sequence.Join(image.DOFade(0f, 0.5f));
            sequence.OnComplete(() => gameObject.SetActive(false));
        }
    }
}