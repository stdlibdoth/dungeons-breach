using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Mathematics;


public class TileHighlighterFactory : MonoBehaviour
{
    [Header("HighlighterPrefab")]
    [SerializeField] private TileHighlighter m_highlighterPreafab;

    [Header("HighlighterData")]
    [SerializeField] private List<TileHighlighterDataSO> m_highlighterData;



    private ObjectPool<TileHighlighter> m_highlighterPool;
    private Dictionary<string, TileHighlighterData> m_data = new Dictionary<string, TileHighlighterData>();


    private void Awake()
    {
        m_highlighterPool = new ObjectPool<TileHighlighter>(CreateFunction, OnObjectGet, OnObjectRelease, DestroyFuntion);
    }

    private void Start()
    {
        for (int i = 0; i < m_highlighterData.Count; i++)
        {
            m_data.Add(m_highlighterData[i].data.name, TileHighlighterData.Copy(m_highlighterData[i].data));
        }
    }


    public TileHighlighter GetHighlighter(string key,IsoGridDirection dir = IsoGridDirection.SE)
    {
        if(!m_data.ContainsKey(key))
            return null;


        var l = m_highlighterPool.Get();
        var data = m_data[key];
        int index = math.clamp((int)dir, 0, data.sprite.Length-1);
        l.Init(data.sprite[index],data.sortingOrder,data.color,data.sortingLayer);
        return l;
    }


    public TileHighlighter[] GetHighlighters(int number, string key,IsoGridDirection dir = IsoGridDirection.SE)
    {
        if (!m_data.ContainsKey(key))
            return null;

        var l = new TileHighlighter[number];
        var data = m_data[key];
        int index = math.clamp((int)dir, 0, data.sprite.Length-1);
        for (int i = 0; i < l.Length; i++)
        {
            l[i] = m_highlighterPool.Get();
            l[i].Init(data.sprite[index], data.sortingOrder, data.color,data.sortingLayer);
        }
        return l;
    }
    

    #region pool

    private TileHighlighter CreateFunction()
    {
        return Instantiate(m_highlighterPreafab).SetPool(m_highlighterPool);
    }


    private void OnObjectGet(TileHighlighter highlighter)
    {
        highlighter.gameObject.SetActive(true);
    }


    public void OnObjectRelease(TileHighlighter highlighter)
    {
        highlighter.gameObject.SetActive(false);
    }


    public void DestroyFuntion(TileHighlighter highlighter)
    {
        Destroy(highlighter.gameObject);
    }

    #endregion
}
