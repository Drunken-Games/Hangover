using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ChangeNPC : MonoBehaviour
{
    public TextMeshProUGUI characterNameText; // CharacterNameText를 참조합니다.
    public Image npcImage; // NPCImage를 참조합니다.
    private string previousCharacterName = ""; // 이전 캐릭터 이름을 저장합니다.

    // Start는 첫 번째 프레임 업데이트 전에 호출됩니다
    void Start()
    {
        // CharacterNameText의 초기 텍스트 값에 따라 UpdateNPCImage가 호출되도록 이벤트를 설정합니다.
        characterNameText.text = characterNameText.text; // 초기 텍스트 설정
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

    void UpdateNPCImage()
    {
        // CharacterNameText에서 캐릭터 이름을 가져옵니다
        string characterName = characterNameText.text;

        // Assets/04 Resources/Character 폴더에서 해당 이름의 이미지를 로드합니다
        string path = Path.Combine(Application.dataPath, "04 Resources/Character/" + characterName + ".png");
        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            npcImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning("해당 이름의 캐릭터 이미지를 찾을 수 없습니다: " + characterName);
        }
    }
}
