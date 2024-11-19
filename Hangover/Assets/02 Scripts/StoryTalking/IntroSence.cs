using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class IntroSence : MonoBehaviour
{
    public Text messageText;
    //public List<string> messages;
    public string csvFilePath = "Assets/04 Resources/messages.csv";
    public float textDisplayDuration = 2.0f;
    private List<string> messages = new List<string>();
    private int currentIndex = 0;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        LoadMessagesFromCSV();

        if (messages.Count > 0)
        {
            messageText.text = messages[currentIndex];
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= textDisplayDuration)
        {
            currentIndex++;
            timer = 0f;

            if(currentIndex < messages.Count)
            {
                messageText.text = messages[currentIndex];
            }
            else
            {
                SceneManager.LoadScene("MainSecne");
            }
        }
    }

    void LoadMessagesFromCSV()
    {
        if(File.Exists(csvFilePath))
        {
            string[] lines = File.ReadAllLines(csvFilePath);
            foreach(string line in lines)
            {
                if(!string.IsNullOrWhiteSpace(line))
                {
                    messages.Add(line.Trim());
                }
            }
        }
        else
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + csvFilePath);
        }
    }
}
