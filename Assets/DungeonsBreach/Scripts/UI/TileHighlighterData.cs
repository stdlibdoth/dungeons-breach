using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TileHighlighterData
{
    [SerializeField] public string name;
    [SerializeField] public Sprite[] sprite;
    [SerializeField] public int sortingOrder;
    [SerializeField] public Color color;

    public static TileHighlighterData Copy(TileHighlighterData data)
    {
        return new TileHighlighterData
        {
            sprite = data.sprite,
            name = data.name,
            sortingOrder = data.sortingOrder,
            color = data.color
        };
    }
}
