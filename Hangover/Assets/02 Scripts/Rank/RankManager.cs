using UnityEngine;
using UnityEngine.Networking; // HTTP 요청을 위한 네임스페이스
using UnityEngine.UI; // UI 관련 네임스페이스 추가
using System.Collections; // 코루틴을 사용하기 위한 네임스페이스
using System.Text; // 문자열 인코딩을 위한 네임스페이스
using TMPro; // TextMesh Pro 관련 네임스페이스 추가

public class RankManager : MonoBehaviour
{
    private const string baseUrl = "https://k11c205.p.ssafy.io/hangover/api/v1/rank"; // API 기본 URL

    [SerializeField] private TMP_InputField nicknameInput; // 닉네임 입력 필드
    [SerializeField] private TMP_Text rankText; // Unity에서 설정할 Text UI 요소
    [SerializeField] private GameObject textPrefab; // UI 텍스트 Prefab
    [SerializeField] private Transform content; // Grid Layout Group의 Content

    private void Start() 
    {
        StartCoroutine(GetRanks());
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
        int finalMoney = 1000; // 예시 값, 실제 값으로 대체 필요
        int finalDay = 5; // 예시 값, 실제 값으로 대체 필요
        Debug.Log("요청" + userNickname + ", " + finalMoney + ", " + finalDay);
        StartCoroutine(PostRank(userNickname, finalMoney, finalDay));
        Debug.Log("성공" + userNickname + ", " + finalMoney + ", " + finalDay);
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
        // 기존 텍스트 제거
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 헤더 추가
        CreateText("닉네임");
        CreateText("최종 금액");
        CreateText("최종 일수");

        // 데이터 추가
        foreach (RankDto rank in userRanks)
        {
            CreateText(rank.userNickname);
            CreateText(rank.finalMoney.ToString());
            CreateText(rank.finalDay.ToString());
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


