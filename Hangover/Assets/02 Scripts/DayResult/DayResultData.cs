[System.Serializable]
public class DayResultData
{
    public int dayNum; // 완료 일차
    public int beforeMoney; // 이전 자산
    public int totalProfit; // 총 수익
    public int tip; // 팁
    public int refund; // 환불
    public int materials; // 총 재료값
    public int netProfit; // 순 수익
    public int afterMoney; // 일차 후  총 자산
    public string playerName; // 플레이어 이름
    public int endingTrigger; // 분기 인덱스

    public int fireCount; // 해고 엔딩을 위한 int, 초기값 0
    public int robotCount; // SNS 엔딩을 위한 int, 초기값 0
}