using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [SerializeField] private Transform m_HPHolder;
    [SerializeField] private PlayerHealthPoint m_HPPrefab;

    private PlayerStatus m_status;
    private List<PlayerHealthPoint> m_HP = new List<PlayerHealthPoint>();


    public void SetHealthBar(PlayerStatus status)
    {
        if(status.maxHP != m_status.maxHP)
        {
            ClearHP();
            for (int i = 0; i < status.maxHP; i++)
            {
                m_HP.Add(Instantiate(m_HPPrefab,m_HPHolder));
            }
        }

        for (int i = 1; i <= status.maxHP; i++)
        {
            m_HP[i-1].SetHP(i<=status.hp);
        }
        m_status = status;
    }



    private void ClearHP()
    {
        foreach (var item in m_HP)
        {
            DestroyImmediate(item);
        }
        m_HP = new List<PlayerHealthPoint>();
    }
}
