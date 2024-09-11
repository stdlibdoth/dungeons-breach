using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;



[RequireComponent(typeof(SpriteRenderer))]
public class TileHighlighter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    private ObjectPool<TileHighlighter> m_pool;


    public void Init(Sprite sprite, int sorting_order, Color color)
    {
        m_spriteRenderer.sprite = sprite;
        m_spriteRenderer.color = color;
        m_spriteRenderer.sortingOrder = sorting_order;
    }

    public TileHighlighter SetPool(ObjectPool<TileHighlighter> pool)
    {
        m_pool = pool;
        return this;
    }

    public void Release()
    {
        m_pool.Release(this);
    }
}




public class TileHighlight
{
    private List<TileHighlighter> m_highlighters;
    

    public TileHighlight(TileHighlighterFactory factory, IsoGridCoord[] coords, string name, IsoGridDirection dir = IsoGridDirection.SE)
    {
        m_highlighters= new List<TileHighlighter>();
        m_highlighters = new List<TileHighlighter>(factory.GetHighlighters(coords.Length, name, dir));
        for (int i = 0; i < coords.Length; i++)
        {
            m_highlighters[i].transform.position = coords[i].ToWorldPosition(GridManager.ActivePathGrid);
        }
    }


    public void SetPosition(IsoGridCoord coord)
    {
        for (int i = 0; i < m_highlighters.Count; i++)
        {
            m_highlighters[i].transform.position = coord.ToWorldPosition(GridManager.ActivePathGrid);
        }
    }

    public void Release()
    {
        foreach (var item in m_highlighters)
        {
            item.Release();
        }
        m_highlighters.Clear();
    }
}
