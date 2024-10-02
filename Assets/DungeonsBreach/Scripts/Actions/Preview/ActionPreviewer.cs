using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPreviewer : MonoBehaviour
{
    [SerializeField] private TileHighlighterFactory m_factory;


      
    private Dictionary<PreviewKey,List<ActionPreviewerData>> m_data = new Dictionary<PreviewKey, List<ActionPreviewerData>>();
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
        // if(PreviewKey.IsNull(key))
        //     key = m_globalKey;
            
        if(!m_data.ContainsKey(key))
        {
            m_data[key] = new List<ActionPreviewerData>();
            m_highlighters[key] = new List<TileHighlighter>();
        }
        m_data[key].Add(data);
        Debug.Log("add   " + key.GetHashCode());
        GeneratePreviewElement(data, key);
    }


    public void ClearPreview(PreviewKey key)
    {
        if(!m_initialized)
            return;
        Debug.Log("clear   " + key.GetHashCode() + "  " + m_data.ContainsKey(key));
        if(!m_data.ContainsKey(key))
            return;
        
        foreach (var item in m_highlighters[key])
        {
            Debug.Log("release  " + key.GetHashCode());
            item.Release();
        }
        m_highlighters.Remove(key);
        m_data.Remove(key);
        Debug.Log(m_data.Count);
    }

    private void GeneratePreviewElement(ActionPreviewerData data, PreviewKey key)
    {
        Debug.Log(data.highlighterKey);
        var hl = m_factory.GetHighlighter(data.highlighterKey,data.dir);
        hl.SetPreviewKey(key);
        hl.transform.position = data.coord.ToWorldPosition(GridManager.ActiveTileGrid);
        m_highlighters[key].Add(hl);
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