using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // TextMeshPro 사용을 위해 추가
using UnityEngine.SceneManagement;  // 씬 관리를 위해 추가

public class ArcadeDialogue : MonoBehaviour
{
    // 변수 선언
    public int Npc_ID;  // NPC 스프라이트 이미지 매칭
    public int Order_ID;  // 주문 대사
    public int Reaction_ID;  // 반응 대사
    public List<int> Correct_ID; // 정답 술 리스트를 저장하기 위한 배열

    public TextMeshProUGUI orderText;  // 주문 대사를 표시할 TextMeshProUGUI 변수
    public TextMeshProUGUI npcNameText;  // NPC 이름을 표시할 TextMeshProUGUI 변수

    // 캐릭터 이미지를 참조하기 위한 리스트
    public List<GameObject> characterImages; // 캐릭터 UI 이미지들을 담을 리스트

    private CocktailCal cocktailCal; // CocktailCal 인스턴스
    private bool timeOut = false;
    private bool waitingForTouchAfterTimeout = false; // 시간 초과 후 대기 상태
    [SerializeField] private GameObject popupPrefab; // 띄울 팝업 prefab
    private GameObject currentPopup; // 현재 띄워진 팝업

    
    #region 대사집
    public List<string> Order_List = new List<string>
    {
        "{랜덤 음료} \n 주세요!",                               // Order_ID 0
        "{랜덤 음료}로 할게요. \n 기대됩니다!",                  // Order_ID 1
        "오늘은 {랜덤 음료}가 \n 딱일 것 같아요. \n 부탁드려요.", // Order_ID 2
        "{랜덤 음료} \n 하나 주세요. \n 궁금하네요",            // Order_ID 3
        "머리가 깨질정도로 \n 달콤한거 주세요",                 // Order_ID 4
        "달달한 음료 \n 한 잔 주세요. \n 오늘은 달달함이 \n 필요해요", // Order_ID 5
        "저는 단 음료는 \n 별로 안 좋아해요",                    // Order_ID 6
        "시큼한 맛이 \n 나는 걸로 부탁드려요",                  // Order_ID 7
        "좀 덜 신 걸로 \n 부탁해요. \n 너무 자극적이면 힘들어요", // Order_ID 8
        "쓴맛이 제대로 \n 느껴지는 음료로 주세요",              // Order_ID 9
        "저는 쓴맛이 싫어요",                                    // Order_ID 10
        "매운 맛이 느껴지는 \n 술로 부탁해요",                   // Order_ID 11
        "매운 건 별로에요",                                      // Order_ID 12
        "여기서 가장 독한 술로 \n 주세요. \n 빨리 기억을 잊어야 해요!", // Order_ID 13
        "독한 걸로 한 잔 \n 부탁드립니다. \n 가끔은 강한 게 좋잖아요", // Order_ID 14
        "독하지 않은 걸로 \n 부탁드려요, \n 오늘은 천천히 즐기고 싶어요", // Order_ID 15
        "뭐든 괜찮아요. \n 한 잔 부탁드립니다",                   // Order_ID 16
        "민트초코실론티지코 \n 주세요"                          // Order_ID 17
    };  // 주문 단계 대사 리스트

    public List<string> Success_Dialogues = new List<string>
    {
        "이 칵테일은 이븐하게 맛있네요",
        "와우, 이거 진짜 괜찮은데요",
        "부드럽고 기분 좋은 맛이네요. 마음에 들어요",
        "한 모금 마시니 기분이 확 좋아지네요!",
        "좋았어! 내가 딱 원했던 그 맛이야",
        "별이 다섯개 !",
        "**엄지척**",
        "완벽해요! 와!",
        "좋았어! 내가 주문한 대로군요",
        "세상에서 가장 위대한 칵테일이네요!"
    };  // 반응 단계 성공 대사 리스트

    public List<string> Fail_Dialogues = new List<string>
    {
        "이거 제 칵테일 아닌 것 같은데요? 저는 이거 안 시켰어요",
        "칵테일을 발로 만드셨나요...",
        "우웩... 정말 못 마시겠네요",
        "아 이건 좀...",
        "에 퉤퉤퉤!!!",
        "끙.....이게 무슨 맛이죠?",
        "하... 저 이거 주문 안 했는데요?",
        "우리 집 강아지도 이건 안 먹을 것 같아요",
        "지금 나랑 장난해??",
        "이게 뭐야! 소보원에 신고하겠어!!"
    };  // 반응 단계 실패 대사 리스트

    public string gameOverDialogue = "자넨 해고야!!";  // 게임 오버 대사
    public string timeOverDialogue = "마감 했습니다~~!!";  // 타임 오버 대사

    // NPC 이름 리스트 추가
    private List<string> Npc_Names = new List<string> { "날두호", "스벤", "아미트", "안토넬라", "알파사란", "웨이청", "암스트롱", "카밀라", "흑곰" }; 

    #endregion

    // 단계 초기화 메서드
    public void InitializePhase()
    {
        if (GameManager.instance != null && GameManager.instance.isReactionPhase)
        {
            InitializeReactionPhase();  // 반응 단계 초기화
        }
        else
        {
            InitializeOrderPhase();  // 주문 단계 초기화
        }
    }

    // 주문 단계 초기화 메서드
    public void InitializeOrderPhase()
    {
        // Correct_ID 리스트 초기화
        Correct_ID = new List<int>();

        // CocktailCal 인스턴스 찾기
        cocktailCal = FindObjectOfType<CocktailCal>();
        if (cocktailCal == null)
        {
            Debug.LogError("CocktailCal 인스턴스를 찾을 수 없습니다.");
        }

        do
        {
            Npc_ID = Random.Range(0, Npc_Names.Count);
        } while (Npc_ID == GameManager.instance.NPC_ID);

        // 캐릭터 이미지 활성화
        ActivateCharacterImage(Npc_ID);

        // 랜덤으로 Order ID 할당
        Order_ID = Random.Range(0, Order_List.Count);
        
        // NPC 이름 표시
        if (npcNameText != null)
        {
            npcNameText.text = Npc_Names[Npc_ID];
        }

        #region 주문 대사 표시 로직
        
        if (orderText != null)
        {
            string selectedOrder = Order_List[Order_ID];

            if (Order_ID >= 0 && Order_ID <= 3)
            {
                // 칵테일 리스트에서 랜덤으로 칵테일 선택
                if (cocktailCal != null && cocktailCal.cocktails != null && cocktailCal.cocktails.Count > 0)
                {
                    int randomIndex = Random.Range(0, cocktailCal.cocktails.Count);
                    CocktailData randomCocktail = cocktailCal.cocktails[randomIndex];

                    // 주문 대사에 칵테일 이름 대체
                    selectedOrder = selectedOrder.Replace("{랜덤 음료}", randomCocktail.cocktailName);

                    // Correct_ID에 해당 칵테일의 ID를 추가
                    Correct_ID.Add(randomCocktail.id);
                }
                else
                {
                    // 칵테일 리스트가 없을 경우
                    selectedOrder = selectedOrder.Replace("{랜덤 음료}", "아무거나");
                }
            }
             else if (Order_ID == 4)
            {
                // "머리가 깨질정도로 달콤한거 주세요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.sweet >= 4)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 5)
            {
                // "달달한 음료 한 잔 주세요. 오늘은 달달함이 필요해요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.sweet >= 3)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 6)
            {
                // "저는 단 음료는 별로 안 좋아해요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.sweet <= 2)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 7)
            {
                // "시큼한 맛이 나는 걸로 부탁드려요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.sour >= 3)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 8)
            {
                // "좀 덜 신 걸로 부탁해요. 너무 자극적이면 힘들어요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.sour <= 2)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 9)
            {
                // "쓴맛이 제대로 느껴지는 음료로 주세요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.bitter >= 3)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 10)
            {
                // "저는 쓴맛이 싫어요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.bitter <= 2)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 11)
            {
                // "매운 맛이 느껴지는 술로 부탁해요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.spice >= 3)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 12)
            {
                // "매운 건 별로에요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.spice <= 2)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 13)
            {
                // "여기서 가장 독한 술로 주세요. 빨리 기억을 잊어야 해요!"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.spirit >= 5)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 14)
            {
                // "독한 걸로 한 잔 부탁드립니다. 가끔은 강한 게 좋잖아요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.spirit >= 3)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 15)
            {
                // "독하지 않은 걸로 부탁드려요, 오늘은 천천히 즐기고 싶어요"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        if (cocktail.spirit <= 2)
                        {
                            Correct_ID.Add(cocktail.id);
                        }
                    }
                }
            }
            else if (Order_ID == 16)
            {
                // "뭐든 괜찮아요. 한 잔 부탁드립니다"
                if (cocktailCal != null && cocktailCal.cocktails != null)
                {
                    foreach (var cocktail in cocktailCal.cocktails)
                    {
                        Correct_ID.Add(cocktail.id);
                    }
                }
            }
            else if (Order_ID == 17)
            {
                Correct_ID.Add(-1);
            }
            
            // 주문 대사 텍스트 설정
            orderText.text = selectedOrder;
        }
        #endregion
        
        if (GameManager.instance != null)
        {
            GameManager.instance.NPC_ID = Npc_ID;
            GameManager.instance.Correct_ID = new List<int>(Correct_ID);
           
            Debug.Log("GameManager의 isReactionPhase가 true로 설정되었습니다.");
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    public void InitializeReactionPhase()
    {
        Reaction_ID = 0;
        if (orderText != null)
        {
            Reaction_ID = Random.Range(0, 10);
            Npc_ID = GameManager.instance.NPC_ID;
            ActivateCharacterImage(Npc_ID);
            npcNameText.text = Npc_Names[Npc_ID];
            
            // 칵테일 비용 가져오기
            int materialCost = 0;
            materialCost = cocktailCal.CalculateMaterialCost(GameManager.instance.cocktailParameters);
            
            // GameManager 인스턴스가 존재하는지 확인
            if (GameManager.instance != null)
            {
                // Correct_ID 리스트 출력
                Debug.Log("Correct_ID 리스트: " + string.Join(", ", GameManager.instance.Correct_ID));

                // 사용자가 만든 술 ID 출력
                Debug.Log("사용자가 만든 completedRecipeId: " + GameManager.instance.completedRecipeId);
                
                FindObjectOfType<ArcadeManager>().DeductMaterialCost(materialCost);
                
                // completedRecipeId를 Correct_ID 리스트에서 확인
                if (GameManager.instance.Correct_ID.Contains(GameManager.instance.completedRecipeId))
                {
                    Debug.Log("Correct_ID 리스트에 completedRecipeId가 포함되어 있습니다.");
                    orderText.text = Success_Dialogues[Reaction_ID];
                    int price = cocktailCal != null ? cocktailCal.GetCocktailPrice(GameManager.instance.completedRecipeId) : 0;

                    FindObjectOfType<ArcadeManager>().AddGold(price); // 골드 증가 함수 호출
                }
                else
                {
                    Debug.Log("Correct_ID 리스트에 completedRecipeId가 포함되어 있지 않습니다.");
                    orderText.text = Fail_Dialogues[Reaction_ID];
                    --GameManager.instance.life;
                    Debug.Log($"목숨 : {GameManager.instance.life}");
                }
            }
            else
            {
                Debug.LogWarning("GameManager 인스턴스가 없습니다.");
            }

            Debug.Log("반응 단계가 시작되었습니다.");
            GameManager.instance.isReactionPhase = true;  // 반응 단계임을 표시
        }
        else
        {
            Debug.LogError("orderText 참조가 설정되지 않았습니다.");
        }
    }


    // 캐릭터 이미지를 활성화하는 메서드
    private void ActivateCharacterImage(int npcId)
    {
        // 모든 이미지를 비활성화
        foreach (GameObject image in characterImages)
        {
            image.SetActive(false);
        }

        // npcId가 characterImages의 범위 내에 있는지 확인
        if (npcId >= 0 && npcId < characterImages.Count)
        {
            characterImages[npcId].SetActive(true);
        }
        else
        {
            Debug.LogError("NPC_ID가 캐릭터 이미지 리스트의 범위를 벗어났습니다.");
        }
    }

    // 버튼 클릭 시 호출될 메서드
    public void OnButtonClick()
    {
        if (GameManager.instance != null)
        {



            if (GameManager.instance.life <= 0)
            {
                if (!waitingForTouchAfterTimeout)
                {
                    // 모든 이미지를 비활성화
                    foreach (GameObject image in characterImages)
                    {
                        image.SetActive(false);
                    }

                    npcNameText.text = "점장";
                    
                    orderText.text = gameOverDialogue;
                    waitingForTouchAfterTimeout = true; // 첫 번째 클릭 후 대기 상태로 전환
                }
                else
                {
                    // SceneManager.LoadScene("NicknameScene"); // 두 번째 클릭 시 종료 씬으로 이동
                    ShowPopup();
                }
            }
            // 시간 초과 시 종료 대사 표시 후 대기 상태로 전환
            else if (timeOut)
            {
                
                if (!waitingForTouchAfterTimeout)
                {
                    // 모든 이미지를 비활성화
                    foreach (GameObject image in characterImages)
                    {
                        image.SetActive(false);
                    }

                    npcNameText.text = "점장";
                    
                    orderText.text = timeOverDialogue;
                    waitingForTouchAfterTimeout = true; // 첫 번째 클릭 후 대기 상태로 전환
                }
                else
                {
                    // SceneManager.LoadScene("NicknameScene"); // 두 번째 클릭 시 종료 씬으로 이동
                    ShowPopup();
                }
                return;
            }

            // 반응 단계 처리
            else if (GameManager.instance.isReactionPhase)
            {
                if (GameManager.instance.GetRemainingArcadeTime() <= 0) // 타이머 시간이 0 이하면
                {
                    timeOut = true; // 시간 초과 상태 설정
                    return;
                }
                else
                {
                    GameManager.instance.isReactionPhase = false;
                    InitializeOrderPhase();
                }
            }
            else
            {
                // 주문 단계에서 빌드 씬으로 이동
                GameManager.instance.isReactionPhase = true;
                LoadBuildScene();
            }
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
        }
    }
    // 씬 전환 메서드
    public void LoadBuildScene()
    {
        SceneManager.LoadScene("BuildScene");
    }

    
    // Start 메서드
    void Start()
    {
        
        cocktailCal = FindObjectOfType<CocktailCal>();
        // InitializePhase();  // 단계 초기화 메서드 호출
    }

    // Update 메서드
    void Update()
    {
        // 필요 시 업데이트 로직 추가
    }

    
    // 팝업을 닫는 메소드
    public void ClosePopup() 
    {
        Object popup = GameObject.Find("RankingNickNamePopUp(Clone)");
        // 이미 팝업이 열려 있다면 이전 팝업을 삭제
        if (popup != null)
        {
            Destroy(popup);
        }
    }

    // 팝업을 띄우는 메소드
    // 버튼 클릭 시 호출되는 메소드
    private void ShowPopup()
    {
        // 이미 팝업이 열려 있다면 이전 팝업을 삭제
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }

        // 팝업 prefab을 인스턴스화
        currentPopup = Instantiate(popupPrefab, Vector3.zero, Quaternion.identity);

        // Canvas의 자식으로 설정
        currentPopup.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // 중앙에 배치하기
        RectTransform rectTransform = currentPopup.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero; // 중앙 위치로 설정
    }
    
}