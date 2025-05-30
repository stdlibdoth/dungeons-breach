using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[System.Serializable]

public class PathFindingAgent : MonoBehaviour
{
    [SerializeField] private PathFindingMask m_intrinsicMask;
    [SerializeField] private PathFindingMask m_blockingMask;
    [SerializeField] private Transform m_locamotionsHolder;
    [SerializeField] private Transform m_targetTransform;
    [SerializeField] protected IsoGridDirection m_initDirection;

    [SerializeField] private List<IsoGridCoord> m_wp;




    private UnityEvent m_onReachingTarget;

    private List<ILocamotion> m_locamotions;
    [SerializeField] private IsoGridCoord m_coord;
    private IsoGridCoord m_prevCoord;

    private IsoGridDirection m_direction;
    private bool m_isMoving;

    public IsoGridDirection Direction
    {
        get { return m_direction; }
        set
        {
            m_direction = value;
            foreach (var lm in m_locamotions)
            {
                lm.Direction = value;
            }
        }
    }

    public bool IsMoving
    {
        get { return m_isMoving; }
    }

    public UnityEvent OnReachingTarget
    {
        get { return m_onReachingTarget; }
    }

    public PathFindingMask IntrinsicMask
    {
        get { return m_intrinsicMask; }
    }

    public PathFindingMask BlockingMask
    {
        get { return m_blockingMask; }
    }

    public IsoGridCoord Coordinate
    {
        get { return m_coord; }
    }

    public PathFindingAgent(PathFindingMask intrinsic, PathFindingMask blocking)
    {
        m_intrinsicMask = intrinsic;
        m_blockingMask = blocking;
    }



    private void Awake()
    {
        m_locamotions = new List<ILocamotion>();
        m_locamotions.AddRange(m_locamotionsHolder.GetComponentsInChildren<ILocamotion>(true));
        m_onReachingTarget = new UnityEvent();
        foreach (var locamotion in m_locamotions)
        {
            locamotion.Transform = m_targetTransform;
        }
    }

    public void Init()
    {
        var grid = GridManager.ActivePathGrid;
        m_coord = m_targetTransform.position.ToIsoCoordinate(grid);
        Direction = m_initDirection;
        var tileMask = grid.PathFindingTileMask(m_coord);
        grid.UpdatePathFindingMask(m_coord, tileMask | m_intrinsicMask);
    }

    private bool TryGetLocamotion(LocamotionType type, out ILocamotion locamotion)
    {
        locamotion = null;
        for (int i = 0; i < m_locamotions.Count; i++)
        {
            if (m_locamotions[i].Type == type)
            {
                locamotion = m_locamotions[i];
                return true;
            }
        }
        return false;
    }



    public IEnumerator MoveAgent(LocamotionType locamotion_type, IsoGridCoord target)
    {
        if(target == m_coord)
            yield break;

        TryGetLocamotion(locamotion_type,out var locamotion);
        var grid = GridManager.ActivePathGrid;
        var tileMask = grid.PathFindingTileMask(m_coord);
        grid.UpdatePathFindingMask(m_coord, tileMask ^ m_intrinsicMask);
        m_isMoving = true;
        if (IsoGridPathFinding.FindPathAstar(m_coord, target, grid, m_blockingMask, out var path))
        {
            List<IsoGridCoord> waypoints = new List<IsoGridCoord>();
            for (int i = path.Count - 2; i > 0; i--)
            {
                if (path[i-1].x != path[i + 1].x && path[i-1].y != path[i + 1].y)
                {
                    waypoints.Add(path[i]);
                }
            }
            waypoints.Add(target);

            m_wp = waypoints;
            for (int i = 0; i < waypoints.Count; i++)
            {
                var moveTarget = waypoints[i];
                yield return StartCoroutine(locamotion.StartLocamotion(m_coord, moveTarget));
                m_coord = moveTarget;
            }
            tileMask = grid.PathFindingTileMask(m_coord);
            grid.UpdatePathFindingMask(m_coord, tileMask | m_intrinsicMask);
            m_onReachingTarget.Invoke();
            m_onReachingTarget.RemoveAllListeners();
        }
        m_isMoving = false;
    }
}
