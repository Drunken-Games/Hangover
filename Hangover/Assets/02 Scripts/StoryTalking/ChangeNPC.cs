using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
public class ChangeNPC : MonoBehaviour
{
    public TextMeshProUGUI characterNameText; // CharacterNameText�� �����մϴ�.
    public Image npcImage; // NPCImage�� �����մϴ�.
    private string previousCharacterName = ""; // ���� ĳ���� �̸��� �����մϴ�.


    // Start is called before the first frame update
    void Start()
    {
        // CharacterNameText�� ���� ����� ������ UpdateNPCImage�� ȣ��ǵ��� �̺�Ʈ�� �����մϴ�.
        characterNameText.text = characterNameText.text; // �ʱ� �ؽ�Ʈ ����
        UpdateNPCImage();
    }

    void LateUpdate()
    {
        // CharacterNameText�� �ؽ�Ʈ�� ����Ǿ��� ���� �̹��� ������Ʈ
        if (characterNameText.text != previousCharacterName)
        {
            UpdateNPCImage();
            previousCharacterName = characterNameText.text;
        }
    }

    void UpdateNPCImage()
    {
        // CharacterNameText���� ĳ���� �̸��� �����ɴϴ�.
        string characterName = characterNameText.text;

        // Assets/04 Resources/Character ��ο��� �ش� �̸��� �̹����� �ε��մϴ�.
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
            Debug.LogWarning("Character image not found for name: " + characterName);
        }
    }
}
