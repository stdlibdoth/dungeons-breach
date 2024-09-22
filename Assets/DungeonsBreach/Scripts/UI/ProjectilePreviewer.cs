using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePreviewer : MonoBehaviour
{
    [SerializeField] protected LineRenderer m_lineRenderer;
    [SerializeField] protected Material m_enemyMat;
    [SerializeField] protected Material m_playerMat;


    public virtual void SetPreviewer(Vector3 start, Vector3 end, bool is_player)
    {
        m_lineRenderer.positionCount = 2;
        Vector3[] pos = new Vector3[2];
        pos[0] = end;
        pos[1] = start;
        m_lineRenderer.SetPositions(pos);
        m_lineRenderer.material = is_player? m_playerMat:m_enemyMat;
        m_lineRenderer.enabled = true;
    }

    public void ClearPreviewer()
    {
        m_lineRenderer.enabled = false;
    }
}
