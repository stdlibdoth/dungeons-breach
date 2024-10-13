using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPreviewer : MonoBehaviour
{
    [SerializeField] private TileHighlighterFactory m_factory;


    private Dictionary<PreviewKey,List<TileHighlighter>> m_highlighters = new Dictionary<PreviewKey, List<TileHighlighter>>();

    private bool m_initialized;

    public void InitPreview()
    {
        m_initialized =true;
    }


    public void RegistorPreview(ActionPreviewerData data,PreviewKey key)
    {
        if(!m_initialized)
        {
            throw new System.Exception("Previewer not initialized");
        }
        if(!m_highlighters.ContainsKey(key))
        {
            m_highlighters[key] = new List<TileHighlighter>();
        }
        GeneratePreviewElement(data, key);
    }


    public void RegistorPreview(string highlighter_key, Vector3 position,PreviewKey key)
    {
        if(!m_initialized)
        {
            throw new System.Exception("Previewer not initialized");
        }
        if(!m_highlighters.ContainsKey(key))
        {
            m_highlighters[key] = new List<TileHighlighter>();
        }
        GeneratePreviewElement(highlighter_key,position, key);
    }


    public void ClearPreview(PreviewKey key)
    {
        if(!m_initialized)
            return;
        if(!m_highlighters.ContainsKey(key))
            return;
        
        foreach (var item in m_highlighters[key])
        {
            item.Release();
        }
        m_highlighters.Remove(key);
    }

    private void GeneratePreviewElement(ActionPreviewerData data, PreviewKey key)
    {
        //Debug.Log(data.highlighterKey);
        var hl = m_factory.GetHighlighter(data.highlighterKey,data.dir);
        hl.SetPreviewKey(key);
        hl.transform.position = data.coord.ToWorldPosition(GridManager.ActiveTileGrid);
        m_highlighters[key].Add(hl);
    }

    private void GeneratePreviewElement(string highlighter_key, Vector3 position, PreviewKey preview_key)
    {
        var hl = m_factory.GetHighlighter(highlighter_key,IsoGridDirection.SE);
        hl.SetPreviewKey(preview_key);
        hl.transform.position = position;
        m_highlighters[preview_key].Add(hl);
    }

}


[System.Serializable]
public class ActionPreviewerData
{
    public string highlighterKey;
    public IsoGridDirection dir;
    public IsoGridCoord coord;

    public ActionPreviewerData(string key, IsoGridDirection direction, IsoGridCoord coord)
    {
        highlighterKey = key;
        dir = direction;
        this.coord = coord;
    }
}