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


    public TileHighlighter RegistorPreview(ActionPreviewerData data,PreviewKey key)
    {
        if(!m_initialized)
        {
            throw new System.Exception("Previewer not initialized");
        }
        if(!m_highlighters.ContainsKey(key))
        {
            m_highlighters[key] = new List<TileHighlighter>();
        }
        return GeneratePreviewElement(data, key);
    }


    public TileHighlighter RegistorPreview(string highlighter_key, Vector3 position,PreviewKey key)
    {
        if(!m_initialized)
        {
            throw new System.Exception("Previewer not initialized");
        }
        if(!m_highlighters.ContainsKey(key))
        {
            m_highlighters[key] = new List<TileHighlighter>();
        }
        return GeneratePreviewElement(highlighter_key, position, key);
    }


    public void ClearPreview(PreviewKey key)
    {
        if(!m_initialized)
            return;
        if (PreviewKey.IsNull(key))
            key = PreviewKey.GlobalKey;
        if (!m_highlighters.ContainsKey(key))
            return;
        
        foreach (var item in m_highlighters[key])
        {
            item.Release();
        }
        m_highlighters.Remove(key);
    }

    private TileHighlighter GeneratePreviewElement(ActionPreviewerData data, PreviewKey key)
    {
        //Debug.Log(data.highlighterKey);
        var hl = m_factory.GetHighlighter(data.highlighterKey,data.dir);
        hl.SetPreviewKey(key);
        hl.transform.position = data.coord.ToWorldPosition(GridManager.ActiveTileGrid);
        m_highlighters[key].Add(hl);
        return hl;
    }

    private TileHighlighter GeneratePreviewElement(string highlighter_key, Vector3 position, PreviewKey preview_key)
    {
        var hl = m_factory.GetHighlighter(highlighter_key,IsoGridDirection.SE);
        hl.SetPreviewKey(preview_key);
        hl.transform.position = position;
        m_highlighters[preview_key].Add(hl);
        return hl;
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