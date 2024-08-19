using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ThemeBase:MonoBehaviour
{
    [SerializeField] protected string m_themeName;

    public string Name
    {
        get { return m_themeName; }
    }

    public virtual void Init(string name)
    {
        m_themeName = name;
    }
}
