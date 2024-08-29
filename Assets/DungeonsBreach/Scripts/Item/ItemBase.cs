using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemBase : MonoBehaviour
{
    [SerializeField] private List<ModifierBase> m_modifiers;


    public T Modified<T>(T param) where T: UnitStatusBase
    {
        for (int i = 0; i < m_modifiers.Count; i++)
        {
            param = m_modifiers[i].Modify(param);
        }
        return param;
    }
}
