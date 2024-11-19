using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CocktailCheckTest : MonoBehaviour
{
    public CocktailCheck cocktailCheck; // CocktailCheck 컴포넌트를 참조할 변수


    // Start is called before the first frame update
    void Start()
    {
        // CocktailCheck 컴포넌트를 가져옴
        cocktailCheck = GetComponent<CocktailCheck>();

        if (cocktailCheck == null)
        {
            Debug.LogError("CocktailCheck 컴포넌트를 찾을 수 없습니다. 해당 GameObject에 CocktailCheck 컴포넌트를 추가하세요.");
            return;
        }

        // LoadCocktailData가 실행되었는지 확인 후 테스트 실행
        cocktailCheck.LoadCocktailData(); // 필요시 호출 (public 메서드로 만들어 테스트용으로 접근)
        RunTest();
    }

    private void RunTest()
    {
        // 1. 칵테일 ID와 유저가 선택한 재료 비율을 하드코딩하여 테스트
        int testCocktailId = 1;
        Dictionary<string, float> userIngredients = new Dictionary<string, float>
        {
            { "Vodka", 0.4f },
            { "OrangeJuice", 0.6f }
        };

        // 비용 초기화
        float cost;

        // CocktailCheck 클래스의 CheckIngredients 함수 호출
        bool isSuccessful = cocktailCheck.CheckIngredients(testCocktailId, userIngredients, out cost);

        // 결과 출력
        if (isSuccessful)
        {
            Debug.Log("테스트 성공: 칵테일 제작 성공! 총 비용: " + cost);
        }
        else
        {
            Debug.Log("테스트 실패: 칵테일 제작 실패. 재료가 일치하지 않습니다.");
        }

        // 2. 추가 테스트 케이스
        testCocktailId = 2;
        userIngredients = new Dictionary<string, float>
        {
            { "Gin", 0.3f },
            { "Tonic", 0.6f } // 일부러 잘못된 비율을 입력하여 실패를 유도
        };

        isSuccessful = cocktailCheck.CheckIngredients(testCocktailId, userIngredients, out cost);

        if (isSuccessful)
        {
            Debug.Log("테스트 성공: 칵테일 제작 성공! 총 비용: " + cost);
        }
        else
        {
            Debug.Log("테스트 실패: 칵테일 제작 실패. 재료가 일치하지 않습니다.");
        }
    }
}
