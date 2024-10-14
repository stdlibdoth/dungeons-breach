using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using TMPro;



[RequireComponent(typeof(SpriteRenderer))]
public class TileHighlighter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private TextMeshPro m_TMP;
    
    private ObjectPool<TileHighlighter> m_pool;
    private int previewkey;


    public void Init(Sprite sprite, int sorting_order, Color color, string sorting_layer)
    {
        m_spriteRenderer.sprite = sprite;
        m_spriteRenderer.color = color;
        m_spriteRenderer.sortingOrder = sorting_order;
        m_spriteRenderer.sortingLayerID = SortingLayer.NameToID(sorting_layer);
    }

    public TileHighlighter SetPool(ObjectPool<TileHighlighter> pool)
    {
        m_pool = pool;
        return this;
    }

    public void SetText(string text)
    {
        if (m_TMP == null)
            return;

        m_TMP.text = text;
        m_TMP.gameObject.SetActive(true);
    }


    public void SetPreviewKey(PreviewKey key)
    {
        previewkey = key.GetHashCode();
    }

    public virtual void Release()
    {
        m_TMP?.gameObject.SetActive(false);
        m_pool.Release(this);
    }
}




public class TileHighlight
{
    private List<TileHighlighter> m_highlighters;
    

    public TileHighlight(TileHighlighterFactory factory, IsoGridCoord[] coords, string key, IsoGridDirection dir = IsoGridDirection.SE)
    {
        m_highlighters= new List<TileHighlighter>();
        m_highlighters = new List<TileHighlighter>(factory.GetHighlighters(coords.Length, key, dir));
        for (int i = 0; i < coords.Length; i++)
        {
            m_highlighters[i].transform.position = coords[i].ToWorldPosition(GridManager.ActivePathGrid);
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
