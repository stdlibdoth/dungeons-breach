using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(ILocamotion))]
public class PathFindingAgent : MonoBehaviour
{
    [SerializeField] private PathFindingMask m_intrinsicMask;
    [SerializeField] private PathFindingMask m_blockingMask;

    [SerializeField] private List<IsoGridCoord> m_wp;

    private ILocamotion m_moveLocamotion;
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
        m_moveLocamotion = GetComponent<ILocamotion>();
    }

    public IEnumerator MoveAgent(IsoGridCoord target)
    {
        m_coord = transform.position.ToIsoCoordinate(GridManager.ActiveGrid);

        var grid = GridManager.ActiveGrid;
        var tileMask = grid.PathFindingTileMask(m_coord);
        grid.UpdatePathFindingMask(m_coord, tileMask ^ m_intrinsicMask);

        if (IsoGridPathFinding.FindPathAstar(m_coord, target, grid, m_blockingMask, out var path))
        {
            List<IsoGridCoord> waypoints = new List<IsoGridCoord>();
            for (int i = path.Count - 2; i > 0; i--)
            {
                if (path[i-1].x != path[i + 1].x && path[i-1].y != path[i + 1].y)
                {
                    Debug.Log(path[i]);
                    waypoints.Add(path[i]);
                }
            }
            waypoints.Add(target);


            m_wp = waypoints;
            for (int i = 0; i < waypoints.Count; i++)
            {
                var moveTarget = waypoints[i];
                yield return StartCoroutine(m_moveLocamotion.StartLocamotion(m_coord, moveTarget));
                m_coord = moveTarget;
            }
            tileMask = grid.PathFindingTileMask(m_coord);
            grid.UpdatePathFindingMask(m_coord, tileMask | m_intrinsicMask);
        }
    }
}
