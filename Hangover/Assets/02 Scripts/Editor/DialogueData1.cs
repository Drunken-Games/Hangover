using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Object/Dialogue Data", order = int.MaxValue)]
public class DialogueData1 : ScriptableObject
{
    [SerializeField]
    private int id;
    public int Id { get { return id; } }

    [SerializeField]
    private int day;
    public int Day { get { return day; } }

    [SerializeField]
    private string character;
    public string Character { get { return character; } }

    [SerializeField]
    private string text;
    public string Text { get { return text; } }

    [SerializeField]
    private int cocktailId;
    public int CocktailId { get { return cocktailId; } }

    [SerializeField]
    private List<int> nextDialogueIds = new List<int>();
    public List<int> NextDialogueIds { get { return nextDialogueIds; } }
}
