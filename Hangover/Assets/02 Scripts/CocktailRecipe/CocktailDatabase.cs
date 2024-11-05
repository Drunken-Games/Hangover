using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CocktailDatabase", menuName = "ScriptableObjects/CocktailDatabase")]
public class CocktailDatabase : ScriptableObject
{
    public List<CocktailData> cocktails = new List<CocktailData>();
}