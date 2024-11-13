using UnityEngine;
using UnityEngine.Networking; // HTTP 요청을 위한 네임스페이스
using UnityEngine.UI; // UI 관련 네임스페이스 추가
using System.Collections;
using System.Collections.Generic; // 코루틴을 사용하기 위한 네임스페이스
using System.Text; // 문자열 인코딩을 위한 네임스페이스
using TMPro; // TextMesh Pro 관련 네임스페이스 추가
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Febucci.UI.Core;

public class RankManager : MonoBehaviour, IPointerDownHandler
{
    private const string baseUrl = "https://k11c205.p.ssafy.io/hangover/api/v1/rank"; // API 기본 URL

    [SerializeField] private TMP_InputField nicknameInput; // 닉네임 입력 필드
    [SerializeField] private TMP_Text rankText; // Unity에서 설정할 Text UI 요소
    [SerializeField] private GameObject textPrefab; // UI 텍스트 Prefab
    [SerializeField] private Transform content; // Grid Layout Group의 Content
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private TMP_Text resultTextObject;

    // public TypewriterCore resultText;  
    private TouchScreenKeyboard keyboard; // 모바일 키보드 변수

    private void Start() 
    {
        StartCoroutine(GetRanks());
         // TMP_InputField가 활성화될 때 포커스를 자동으로 설정하도록 리스너 추가
        nicknameInput.onSelect.AddListener(delegate { ActivateKeyboard(); });

         // 입력이 끝났을 때 RegisterRank 메서드 호출
        nicknameInput.onEndEdit.AddListener(delegate { RegisterRank(); });

        resultTextObject.text = "<wave>" + (GameManager.instance.ArcadeGold * 100).ToString() + "</wave>";
        // resultText.ShowText((GameManager.instance.ArcadeGold * 100).ToString());
    }

    private void Update() 
    {
        // TouchScreenKeyboard에서 입력한 텍스트를 TMP_InputField에 업데이트
        if (keyboard != null && keyboard.active)
        {
            nicknameInput.text = keyboard.text; // 키보드의 텍스트를 InputField에 반영
        }
    } 

    // 사용자가 InputField를 터치했을 때 포커스 설정
    public void OnPointerDown(PointerEventData eventData)
    {
        ActivateKeyboard(); // 키보드 활성화
    }

    private void ActivateKeyboard()
    {
        nicknameInput.Select(); // InputField 선택
        nicknameInput.ActivateInputField(); // 모바일 키보드 강제 활성화

        // 모바일 환경에서 키보드를 강제로 열도록 추가
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }



    private RankDto[] ParseJsonArray(string json)
    {
        // JSON 배열에서 대괄호 제거
        json = json.TrimStart('[').TrimEnd(']');

        // 각 항목을 쉼표로 분리
        string[] jsonEntries = json.Split(new[] { "},{" }, System.StringSplitOptions.RemoveEmptyEntries);
        RankDto[] ranks = new RankDto[jsonEntries.Length];

        for (int i = 0; i < jsonEntries.Length; i++)
        {
            // 각 항목의 대괄호 제거 및 JSON 객체로 변환
            string jsonEntry = jsonEntries[i].TrimStart('{').TrimEnd('}');
            jsonEntry = jsonEntry.Replace("},{", "},{"); // JSON 객체로 변환하기 위해 변환
            ranks[i] = JsonUtility.FromJson<RankDto>("{" + jsonEntry + "}");
        }

        return ranks;
    }

    public void OpenKeyboard()
    {
        nicknameInput.ActivateInputField(); // InputField에 포커스를 줍니다.
        TouchScreenKeyboard.Open(""); // 키보드를 열도록 호출합니다.
    }

    // 랭킹 정보를 등록하는 메서드 1
    public void RegisterRank()
    {
        string userNickname = nicknameInput.text; // 입력된 닉네임 가져오기
        int finalMoney = GameManager.instance.ArcadeGold * 100; // 예시 값, 실제 값으로 대체 필요
        int finalDay = 1; // 예시 값, 실제 값으로 대체 필요
        Debug.Log("요청" + userNickname + ", " + finalMoney + ", " + finalDay);
        StartCoroutine(PostRank(userNickname, finalMoney, finalDay));
        Debug.Log("성공" + userNickname + ", " + finalMoney + ", " + finalDay);
        
        GameManager.instance.NPC_ID = 0; // 아케이드 모드 NPC id
        GameManager.instance.Correct_ID = new List<int>(); // 아케이드 모드 주문 술
        GameManager.instance.isReactionPhase = false; // 아케이드 모드 단계
        GameManager.instance.ArcadeGold = 0; // 아케이드 모드 골드 
        // 타이머 초기화 및 정지
        GameManager.instance.StopArcadeTimer(); // 타이머 정지
        GameManager.instance.arcadeTimer = 0; // 타이머 초기화
        GameManager.instance.hasTimerEnded = false; // 타이머 종료 상태 확인 변수
        GameManager.instance.ArcadeStory = false;
        GameManager.instance.life = 3;
        SceneManager.LoadScene("RankScene");

    }

    // 랭킹 정보를 등록하는 메서드 2
    public void RegisterRank(string userNickname, int finalMoney, int finalDay)
    {
        StartCoroutine(PostRank(userNickname, finalMoney, finalDay));
    }

    private IEnumerator PostRank(string userNickname, int finalMoney, int finalRound)
    {
        // RankRequestDto 객체를 JSON 형식으로 변환
        string jsonData = JsonUtility.ToJson(new RankRequestDto(userNickname, finalMoney, finalRound));

        Debug.Log(jsonData);

        // UnityWebRequest를 사용하여 POST 요청 설정
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(baseUrl + "/update", jsonData))
        {
            www.method = UnityWebRequest.kHttpVerbPOST; // POST 메서드 설정
            www.SetRequestHeader("Content-Type", "application/json"); // 요청 헤더 설정
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData)); // JSON 데이터 전송

            // 요청 전송
            yield return www.SendWebRequest();

            // 요청 결과 확인
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error); // 에러 발생 시 로그 출력
                Debug.LogError("Response: " + www.downloadHandler.text); // 추가 응답 로그
            }
            else
            {
                Debug.Log("랭킹 등록 성공: " + www.downloadHandler.text); // 성공 시 응답 출력
            }
        }
    }

    // 랭킹 정보를 조회하는 메서드
    public void FetchRanks()
    {
        StartCoroutine(GetRanks());
    }

    // 랭킹 목록을 GET 요청으로 조회하는 코루틴
    private IEnumerator GetRanks() 
    {
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/list"))
        {
            // 요청 전송
            yield return www.SendWebRequest();

            // 요청 결과 확인
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error); // 에러 발생 시 로그 출력
            }
            else
            {
                // 응답 JSON 데이터를 파싱
                RankDto[] rankList = ParseJsonArray(www.downloadHandler.text);
                DisplayRanks(rankList);
            }
        }
    }

    private void DisplayRanks(RankDto[] userRanks)
    {
        // 기존 아이템 제거
        foreach (Transform child in content)
        {
            Destroy(child.gameObject); // Content의 자식 요소를 모두 제거하여 초기화
        }

        // 데이터 추가
        for(int i = 0; i < userRanks.Length; i++)
        {
            RankDto rank = userRanks[i];

            // Item prefab을 클론하여 새로운 아이템 생성
            GameObject item = Instantiate(itemPrefab, content); // content의 자식으로 설정하면서 클론 생성

            // 아이템 내의 텍스트 설정
             TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>(); // 아이템 내의 모든 Text 컴포넌트 가져오기
             
            // 각각의 텍스트에 값 설정
            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString(); // 등수
                texts[1].text = rank.userNickname; // 닉네임 설정
                texts[2].text = rank.finalMoney.ToString(); // 최종 금액 설정
                // texts[3].text = rank.finalDay.ToString(); // 최종 일수 설정
            }
        }
    }

    private void CreateText(string content)
    {
        GameObject newText = Instantiate(textPrefab, this.content);
        newText.GetComponent<TMP_Text>().text = content;
    }
}

// 랭킹 등록 요청을 위한 DTO 클래스
[System.Serializable]
public class RankRequestDto
{
    public string userNickname; // 닉네임
    public int finalMoney; // 최종 획득 자금
    public int finalDay; // 최종 일차

    // 생성자
    public RankRequestDto(string userNickname, int finalMoney, int finalDay)
    {
        this.userNickname = userNickname;
        this.finalMoney = finalMoney;
        this.finalDay = finalDay;
    }
}

// 랭킹 DTO 클래스
[System.Serializable]
public class RankDto
{
    public long id; //id
    public string userNickname; // 닉네임
    public int finalMoney; // 최종 획득 자금
    public int finalDay; // 최종 일차

    // 생성자
    public RankDto(long id, string userNickname, int finalMoney, int finalDay)
    {
        this.id = id;
        this.userNickname = userNickname;
        this.finalMoney = finalMoney;
        this.finalDay = finalDay;
    }
}


