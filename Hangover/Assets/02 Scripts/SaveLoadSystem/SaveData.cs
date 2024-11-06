[System.Serializable]
public class SaveData
{
    //public int dayNum;       // 현재 일차
    public int nowMoney;   // 현재 금액
    //public string playerName; // 플레이어 이름
    //public int fireCount; // 해고 엔딩을 위한 int, 초기값 0
    //public int robotCount; // SNS 엔딩을 위한 int, 초기값 0
    //public int endingTrigger; // 분기 인덱스

    public int dayNum;
    public int beforeMoney;
    public int totalProfit;
    public int tip;
    public int refund;
    public int materials;
    public int netProfit;
    public int afterMoney;
    public string playerName; // 플레이어 이름을 문자열로 설정
    public int endingTrigger;
    public int fireCount;
    public int robotCount;


    public string saveDateTime; // 저장일시
}
