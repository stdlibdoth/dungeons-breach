using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePreviewer : MonoBehaviour
{
    [Header("UnitSprite")]
    [SerializeField] private GameObject m_unitSpriteObject;


    private Vector3 m_originPosition;
    private GameObject m_spriteInstance;
    private bool m_isPreviewing;


    public void StartPreview(Vector3 preview_pos)
    {
        if (m_unitSpriteObject == null)
            return;


        if (!m_isPreviewing)
        {
            m_originPosition = m_unitSpriteObject.transform.position;
            m_spriteInstance = Instantiate(m_unitSpriteObject, m_originPosition, Quaternion.identity);
        }
        m_spriteInstance.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        m_unitSpriteObject.transform.position = preview_pos;
        m_isPreviewing = true;
    }

    public void StopPreview()
    {
        if (!m_isPreviewing)
            return;
        //Debug.Log("stop preview " + m_unitSpriteObject.transform.parent.name + "   " + m_unitSpriteObject.transform.position);
        m_unitSpriteObject.transform.position = m_originPosition;
        m_isPreviewing = false;
        Destroy(m_spriteInstance);
    }
}
