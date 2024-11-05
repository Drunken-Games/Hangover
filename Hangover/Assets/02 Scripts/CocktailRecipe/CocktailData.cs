using UnityEngine;

[CreateAssetMenu(fileName = "CocktailData", menuName = "ScriptableObjects/CocktailData")]
public class CocktailData : ScriptableObject
{
    public Sprite icon;
    public string cocktailName;
    public int sweet;
    public int sour;
    public int bitter;
    public int spice;
    public int spirit;
    public string alcoholContent;
    public string taste;
    public string description;
    public int method;
}