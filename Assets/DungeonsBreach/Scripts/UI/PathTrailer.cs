using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTrailer : MonoBehaviour
{
    [SerializeField] private LineRenderer m_lineRenderer;
    [SerializeField] private float m_distanceMargin = 0.05f;


    private List<Vector3> m_waypoints = new List<Vector3>();


    private Coroutine m_animationCoroutine;

    public void Init(List<IsoGridCoord> waypoints)
    {
        m_lineRenderer.enabled = false;
        m_waypoints = new List<Vector3>();
        for (int i = 0; i < waypoints.Count; i++)
        {
            m_waypoints.Add(waypoints[i].ToWorldPosition(GridManager.ActivePathGrid));
        }
        UpdateRenderer();
        m_lineRenderer.enabled = true;
    }

    public void HideTrail()
    {
        m_lineRenderer.enabled = false;
    }


    public void StartTrailing(UnitBase unit)
    {
        if(m_animationCoroutine!= null)
            StopCoroutine(m_animationCoroutine);
        m_animationCoroutine = StartCoroutine(AnimateTrail(unit));
    }

    private IEnumerator AnimateTrail(UnitBase unit)
    {
        m_lineRenderer.enabled = true;
        m_waypoints.Add(unit.transform.position);
        while (unit != null && unit.Agent.IsMoving && m_waypoints.Count>1)
        {
            if (Vector3.Distance(m_waypoints[m_waypoints.Count - 1], m_waypoints[m_waypoints.Count - 2]) < m_distanceMargin)
            {
                m_waypoints.RemoveAt(m_waypoints.Count - 2);
            }
            m_waypoints[m_waypoints.Count - 1] = unit.transform.position;
            UpdateRenderer();
            yield return null;
        }
        m_lineRenderer.enabled = false;
    }

    private void UpdateRenderer()
    {
        m_lineRenderer.positionCount = m_waypoints.Count;
        m_lineRenderer.SetPositions(m_waypoints.ToArray());
    }

}
