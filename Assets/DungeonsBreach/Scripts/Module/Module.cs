using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Module : MonoBehaviour
{
    [SerializeField] protected string m_id;
    [SerializeField] protected UnitStatus m_unitParamBase;

    public string Id {  get { return m_id; } }

    public virtual UnitStatus Modified(UnitStatus other)
    {
        return other + m_unitParamBase;
    }
}
