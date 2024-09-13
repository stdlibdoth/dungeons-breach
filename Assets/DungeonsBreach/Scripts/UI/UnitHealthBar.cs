using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] private float m_width;
    [SerializeField] private float m_sidePadding;
    
    [Space]
    [SerializeField] private SpriteRenderer m_frameSprite;
    [SerializeField] private SpriteRenderer m_healthSprite;
    [SerializeField] private Transform m_blockersHolder;
    [SerializeField] private GameObject m_blockerPrefab;

    private float m_unitWidth;
    private List<GameObject> m_blockers = new List<GameObject>();
    private int m_maxHP;

    public void Init(int maxHP)
    {
        ClearBlockers();
        m_maxHP = maxHP;
        float healthWidth = m_width - 2* m_sidePadding;
        m_frameSprite.size = new Vector2(m_width,m_frameSprite.size.y);
        m_healthSprite.size = new Vector2(healthWidth,m_healthSprite.size.y);
        m_unitWidth = healthWidth/maxHP;
        transform.localPosition = new Vector3(-m_width/2,0,0);
        int blockerNum = maxHP-1;
        for (int i = 1; i <= blockerNum; i++)
        {
            float x = m_unitWidth*i + m_sidePadding;
            var blocker = Instantiate(m_blockerPrefab,m_blockersHolder);
            blocker.transform.localPosition = new Vector3(x,0,0);
        }
        gameObject.SetActive(true);
    }


    private void ClearBlockers()
    {
        foreach (var item in m_blockers)
        {
            Destroy(item);
            m_blockers.Clear();
        }
    }

    public void SetHP(int hp)
    {
        int h = math.clamp(hp,0,m_maxHP);
        m_healthSprite.size = new Vector2(m_unitWidth*hp,m_healthSprite.size.y);
    }
}
