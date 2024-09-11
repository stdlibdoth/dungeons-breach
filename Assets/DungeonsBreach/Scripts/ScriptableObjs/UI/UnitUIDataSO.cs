using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitUIData", menuName = "ScriptableObjects/UnitUIData", order = 1)]

public class UnitUIDataSO : ScriptableObject
{
    [SerializeField] private List<UnitUIData> m_data;


    private Dictionary<string, UnitUIData> m_dataMap;

    private void OnEnable()
    {
        m_dataMap = new Dictionary<string, UnitUIData>();
        foreach (var data in m_data)
        {
            m_dataMap.Add(data.unitName,new UnitUIData
            {
                portrait = data.portrait,
                unitName = string.Copy(data.unitName),
            });
        }

    }

    public UnitUIData GetData(string name)
    {
        if(m_dataMap.TryGetValue(name, out var data))
        {
            return data;
        }
        return null;
    }
}
