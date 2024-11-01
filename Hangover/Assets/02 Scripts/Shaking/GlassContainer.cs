using UnityEngine;

public class GlassContainer : MonoBehaviour
{
    [Header("Glass Settings")]
    [SerializeField] private float glassWidth = 3f;     // 컵의 너비
    [SerializeField] private float glassHeight = 5f;    // 컵의 높이
    [SerializeField] private float borderThickness = 0.2f; // 테두리 두께
    [SerializeField] private Color borderColor = new Color(1f, 1f, 1f, 0.8f); // 테두리 색상
    
    private SpriteRenderer glassRenderer;
    private SpriteRenderer borderRenderer;

    private void Awake()
    {
        SetupGlass();
        SetupBorder();
    }

    private void SetupGlass()
    {
        // 메인 글라스용 게임오브젝트
        GameObject glassObj = new GameObject("GlassBackground");
        glassObj.transform.SetParent(transform);
        glassObj.transform.localPosition = Vector3.zero;
        
        glassRenderer = glassObj.AddComponent<SpriteRenderer>();
        glassRenderer.sprite = CreateGlassSprite();
        glassRenderer.sortingOrder = -2;
        glassObj.transform.localScale = new Vector3(glassWidth, glassHeight, 1f);
    }

    private void SetupBorder()
    {
        // 테두리용 게임오브젝트
        GameObject borderObj = new GameObject("GlassBorder");
        borderObj.transform.SetParent(transform);
        borderObj.transform.localPosition = Vector3.zero;
        
        borderRenderer = borderObj.AddComponent<SpriteRenderer>();
        borderRenderer.sprite = CreateBorderSprite();
        borderRenderer.sortingOrder = 1; // 레이어들보다 앞에 표시
        borderObj.transform.localScale = new Vector3(glassWidth + borderThickness, glassHeight + borderThickness, 1f);
    }

    private Sprite CreateGlassSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(1f, 1f, 1f, 0.1f); // 약간 반투명한 흰색
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateBorderSprite()
    {
        int width = 32;
        int height = 32;
        Texture2D texture = new Texture2D(width, height);
        
        // 전체를 투명하게 초기화
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 테두리에 해당하는 픽셀인지 확인
                bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                texture.SetPixel(x, y, isBorder ? borderColor : new Color(1f, 1f, 1f, 0f));
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    public Vector2 GetGlassSize()
    {
        return new Vector2(glassWidth, glassHeight);
    }

    public float GetBorderThickness()
    {
        return borderThickness;
    }
}