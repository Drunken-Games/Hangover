using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ChangeNPC : MonoBehaviour
{
    public TextMeshProUGUI characterNameText; // CharacterNameText를 참조합니다.
    public GameObject characterCanvas; // CharacterCanvas 오브젝트 참조
    //public Image npcImage; // NPCImage를 참조합니다.
    private string previousCharacterName = ""; // 이전 캐릭터 이름을 저장합니다.

    // 캐릭터 이름과 이미지 매핑을 위한 Dictionary
    private Dictionary<string, GameObject> characterImageObjects = new Dictionary<string, GameObject>();

    // 캐릭터 이름 목록 및 이미지 오브젝트 연결
   // public GameObject[] characterImages; // Canvas 안의 각 캐릭터 이미지 (비활성화 상태로 초기 설정)

    // Start는 첫 번째 프레임 업데이트 전에 호출됩니다
    void Start()
    {
        // CharacterNameText가 설정되어 있는지 확인
        if (characterNameText == null)
        {
            Debug.LogError("characterNameText가 null입니다. Inspector에서 설정해 주세요.");
            return;
        }

        // CharacterNameText의 초기 텍스트 값에 따라 UpdateNPCImage가 호출되도록 이벤트를 설정합니다.
        characterNameText.text = characterNameText.text; // 초기 텍스트 설정

        //LoadCharacterImages();
        //npcImage.gameObject.SetActive(false); // 처음에는 비활성화 상태로 시작
        // 캐릭터 이미지 오브젝트를 Dictionary에 저장
        InitializeCharacterImages();
        UpdateNPCImage();
    }

    void LateUpdate()
    {
        // CharacterNameText의 텍스트가 변경된 경우 이미지를 업데이트합니다
        if (characterNameText.text != previousCharacterName)
        {
            UpdateNPCImage();
            previousCharacterName = characterNameText.text;
        }
    }

    // 각 캐릭터 이미지 오브젝트를 Dictionary에 저장
    void InitializeCharacterImages()
    {
        // Canvas 하위의 모든 Image 컴포넌트를 찾아서 Dictionary에 저장
        Image[] images = characterCanvas.GetComponentsInChildren<Image>(true); // CharacterCanvas 하위의 모든 Image 컴포넌트를 포함
        foreach (Image image in images)
        {
            string characterName = image.gameObject.name;
            image.gameObject.SetActive(false); // 모든 캐릭터 이미지를 초기에는 비활성화
            characterImageObjects[characterName] = image.gameObject;
        }
    }

    // 캐릭터 이름에 따라 이미지 활성화/비활성화 업데이트
    void UpdateNPCImage()
    {
        string characterName = characterNameText.text;

        // 모든 캐릭터 이미지를 비활성화
        foreach (var characterImage in characterImageObjects.Values)
        {
            characterImage.SetActive(false);
        }

        // Dictionary에 해당 캐릭터 이름이 존재할 경우, 이미지 활성화
        if (characterImageObjects.ContainsKey(characterName))
        {
            characterImageObjects[characterName].SetActive(true);
        }
        else if (characterName == GameManager.instance.dayResultData.playerName)
        {
            characterImageObjects["주인공"].SetActive(true);
        }
        else
        {
            Debug.LogWarning("해당 이름의 캐릭터 이미지를 찾을 수 없습니다: " + characterName);
        }
    }
}
