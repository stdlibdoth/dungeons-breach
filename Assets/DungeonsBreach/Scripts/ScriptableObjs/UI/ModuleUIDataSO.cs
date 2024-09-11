using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModuleUIData", menuName = "ScriptableObjects/ModuleUIData", order = 1)]

public class ModuleUIDataSO : ScriptableObject
{
    [SerializeField] private List<ModuleUIData> m_data;


    private Dictionary<string, ModuleUIData> m_dataMap;

    private void OnEnable()
    {
        m_dataMap = new Dictionary<string, ModuleUIData>();
        foreach (var data in m_data)
        {
            m_dataMap.Add(data.mouduleName,new ModuleUIData
            {
                icon = data.icon,
                mouduleName = string.Copy(data.mouduleName),
            });
        }
    }

    public ModuleUIData GetData(string name)
    {
        if (m_dataMap.TryGetValue(name, out var data))
        {
            return data;
        }
        return null;
    }
}
