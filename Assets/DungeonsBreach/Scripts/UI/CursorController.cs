using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField]private CursorDataSO m_cursorDataSO;
    private Dictionary<string,CursorData> m_cursorData = new Dictionary<string, CursorData>();


    private void Awake()
    {
        if(m_cursorData == null)
            throw new System.ArgumentException("cursor data is empty");
        foreach (var item in m_cursorDataSO.m_data)
        {
            m_cursorData[item.key] = item;
        }
    }


    public void SetCursor(string key)
    {
        if(!m_cursorData.ContainsKey(key))
            return;
        
        var data = m_cursorData[key];
        Vector2 hotspot = Vector2.one/2;
        Cursor.SetCursor(data.texture,hotspot,CursorMode.Auto);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null,Vector2.zero,CursorMode.Auto);
    }


}


[System.Serializable]
public class CursorData
{
    public string key;
    public Texture2D texture;
}
