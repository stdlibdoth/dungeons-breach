using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathFindingAgent : MonoBehaviour
{
    [SerializeField] private PathFindingMask m_intrinsicMask;
    [SerializeField] private PathFindingMask m_blockingMask;

    [SerializeField] private List<IsoGridCoord> m_wp;

    private IsoMoveLocamotion m_moveLocamotion;
    private IsoGridCoord m_coord;

    public PathFindingMask IntrinsicMask
    {
        get { return m_intrinsicMask; }
    }

    public PathFindingMask BlockingMask
    {
        get { return m_blockingMask; }
    }

    public PathFindingAgent(PathFindingMask intrinsic, PathFindingMask blocking)
    {
        m_intrinsicMask = intrinsic;
        m_blockingMask = blocking;
    }

    private void Awake()
    {
        m_moveLocamotion = GetComponent<IsoMoveLocamotion>();
    }

    public IEnumerator MoveAgent(IsoGridCoord target)
    {
        m_coord = transform.position.ToIsoCoordinate(GridManager.ActiveGrid);
        var grid = GridManager.ActiveGrid;
        if (IsoGridPathFinding.FindPathAstar(m_coord, target, grid, m_blockingMask, out var path))
        {
            List<IsoGridCoord> waypoints = new List<IsoGridCoord>();
            waypoints.Add(m_coord);
            var lastWaypoint = m_coord;
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (path[i].x != lastWaypoint.x && path[i].y != lastWaypoint.y)
                {
                    lastWaypoint = path[i + 1];
                    waypoints.Add(lastWaypoint);
                }
                else if (path[i] == target)
                {
                    waypoints.Add(path[i]);
                }
            }


            m_wp = waypoints;
            for (int i = 1; i < waypoints.Count; i++)
            {
                var moveTarget = waypoints[i];
                Debug.Log(moveTarget.ToString());
                yield return StartCoroutine(m_moveLocamotion.StartLocamotion(m_coord, moveTarget));
                m_coord = moveTarget;
            }
        }
    }
}
