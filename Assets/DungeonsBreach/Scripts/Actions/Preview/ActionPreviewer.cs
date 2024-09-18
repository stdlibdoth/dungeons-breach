using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPreviewer : MonoBehaviour
{
    [SerializeField] private TileHighlighterFactory m_factory;
      
    private List<ActionPreviewerData> m_data = new List<ActionPreviewerData>();
    private List<TileHighlighter> m_highlighters = new List<TileHighlighter>();

    private bool m_initialized;

    public void InitPreview()
    {
        m_data = new List<ActionPreviewerData>();
        m_highlighters = new List<TileHighlighter>();
        m_initialized =true;
    }


    public void RegistorPreview(ActionPreviewerData data)
    {
        if(!m_initialized)
        {
            throw new System.Exception("Previewer not initialized");
        }
        m_data.Add(data);
        GeneratePreviewElement(data);
    }

    public void ClearPreview()
    {
        foreach (var item in m_highlighters)
        {
            item.Release();
        }
        m_highlighters.Clear();
        m_initialized = false;
    }


    private void GeneratePreviewElement(ActionPreviewerData data)
    {
        Debug.Log(data.highlighterKey);
        var hl = m_factory.GetHighlighter(data.highlighterKey,data.dir);
        hl.transform.position = data.coord.ToWorldPosition(GridManager.ActiveTileGrid);
        m_highlighters.Add(hl);
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