using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "Scriptable Object/Dialogue Database", order = int.MaxValue)]
public class DialogueDatabase : ScriptableObject
{
    [SerializeField]
    public List<DialogueData1> dialogues = new List<DialogueData1>();
}
