using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePreviewer : MonoBehaviour
{
    [SerializeField] private LineRenderer m_lineRenderer;



    public virtual void SetPreviewer(Vector3 start, Vector3 end)
    {
        m_lineRenderer.positionCount = 2;
        Vector3[] pos = new Vector3[2];
        pos[0] = end;
        pos[1] = start;
        m_lineRenderer.SetPositions(pos);
        m_lineRenderer.enabled = true;
    }

    public void ClearPreviewer()
    {
        m_lineRenderer.enabled = false;
    }
}
