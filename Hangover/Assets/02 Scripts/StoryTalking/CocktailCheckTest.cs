using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CocktailCheckTest : MonoBehaviour
{
    public CocktailCheck cocktailCheck; // CocktailCheck ������Ʈ�� ������ ����


    // Start is called before the first frame update
    void Start()
    {
        // CocktailCheck ������Ʈ�� ������
        cocktailCheck = GetComponent<CocktailCheck>();

        if (cocktailCheck == null)
        {
            Debug.LogError("CocktailCheck ������Ʈ�� ã�� �� �����ϴ�. �ش� GameObject�� CocktailCheck ������Ʈ�� �߰��ϼ���.");
            return;
        }

        // LoadCocktailData�� ����Ǿ����� Ȯ�� �� �׽�Ʈ ����
        cocktailCheck.LoadCocktailData(); // �ʿ�� ȣ�� (public �޼���� ����� �׽�Ʈ������ ����)
        RunTest();
    }

    private void RunTest()
    {
        // 1. Ĭ���� ID�� ������ ������ ��� ������ �ϵ��ڵ��Ͽ� �׽�Ʈ
        int testCocktailId = 1;
        Dictionary<string, float> userIngredients = new Dictionary<string, float>
        {
            { "Vodka", 0.4f },
            { "OrangeJuice", 0.6f }
        };

        // ��� �ʱ�ȭ
        float cost;

        // CocktailCheck Ŭ������ CheckIngredients �Լ� ȣ��
        bool isSuccessful = cocktailCheck.CheckIngredients(testCocktailId, userIngredients, out cost);

        // ��� ���
        if (isSuccessful)
        {
            Debug.Log("�׽�Ʈ ����: Ĭ���� ���� ����! �� ���: " + cost);
        }
        else
        {
            Debug.Log("�׽�Ʈ ����: Ĭ���� ���� ����. ��ᰡ ��ġ���� �ʽ��ϴ�.");
        }

        // 2. �߰� �׽�Ʈ ���̽�
        testCocktailId = 2;
        userIngredients = new Dictionary<string, float>
        {
            { "Gin", 0.3f },
            { "Tonic", 0.6f } // �Ϻη� �߸��� ������ �Է��Ͽ� ���и� ����
        };

        isSuccessful = cocktailCheck.CheckIngredients(testCocktailId, userIngredients, out cost);

        if (isSuccessful)
        {
            Debug.Log("�׽�Ʈ ����: Ĭ���� ���� ����! �� ���: " + cost);
        }
        else
        {
            Debug.Log("�׽�Ʈ ����: Ĭ���� ���� ����. ��ᰡ ��ġ���� �ʽ��ϴ�.");
        }
    }
}
