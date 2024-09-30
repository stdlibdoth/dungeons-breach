using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPreviewer : MonoBehaviour
{
    [SerializeField] private TileHighlighterFactory m_factory;


      
    private Dictionary<object,List<ActionPreviewerData>> m_data;
    private Dictionary<object,List<TileHighlighter>> m_highlighters;

    private bool m_initialized;
    private object m_globalKey;


    private static object m_globalPreviewKey = new object();
    public static object GlobalPreviewKey
    {
        get {return m_globalPreviewKey;}
    }

    public void InitPreview()
    {
        m_data = new Dictionary<object, List<ActionPreviewerData>>();
        m_highlighters =new Dictionary<object, List<TileHighlighter>>();
        m_initialized =true;
        m_globalKey = new object();
    }


    public void RegistorPreview(ActionPreviewerData data,object key = null)
    {
        if(!m_initialized)
        {
            throw new System.Exception("Previewer not initialized");
        }

        if(key == null)
            key = m_globalKey;
            
        if(!m_data.ContainsKey(key))
        {
            m_data[key] = new List<ActionPreviewerData>();
            m_highlighters[key] = new List<TileHighlighter>();
        }
        m_data[key].Add(data);
        GeneratePreviewElement(data, key);
    }


    public void ClearPreview(object key)
    {
        if(!m_initialized)
            return;

        if(!m_data.ContainsKey(key))
            return;
        foreach (var item in m_highlighters[key])
        {
            item.Release();
        }
        m_highlighters.Clear();
        m_initialized = false;
    }

    // public void ClearAll()
    // {
    //     foreach (var item in m_data)
    //     {
    //         ClearPreview(item.Key);
    //     }
    // }


    private void GeneratePreviewElement(ActionPreviewerData data, object key)
    {
        Debug.Log(data.highlighterKey);
        var hl = m_factory.GetHighlighter(data.highlighterKey,data.dir);
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