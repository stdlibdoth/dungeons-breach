using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

[System.Serializable]

public class PathFindingAgent : MonoBehaviour
{
    [SerializeField] private PathFindingMask m_intrinsicMask;
    [SerializeField] private PathFindingMask m_blockingMask;
    [SerializeField] private PathFindingMask m_fallingMask;
    [SerializeField] private IsoGridDirection m_direction;

    [SerializeField] private Transform m_locamotionsHolder;
    [SerializeField] private Transform m_targetTransform;
    [SerializeField] private MovePreviewer m_movePreviewer;

    private UnityEvent m_onReachingTarget;

    private List<ILocamotion> m_locamotions;
    private IsoGridCoord m_coord;
    private IsoGridCoord m_originCoord;


    private bool m_isMoving;
    private bool m_isPreviewing;
    private bool m_showPreviewVisual;

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

    public PathFindingMask FallMask
    {
        get { return m_fallingMask; }
    }

    public IsoGridCoord Coordinate
    {
        get { return m_coord; }
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
        m_originCoord = m_coord;
        Direction = m_direction;
        var tileMask = grid.PathingMaskSingleTile(m_coord);
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


    public IsoGridCoord[] ReachableCoordinates(int range,PathGrid grid)
    {
        PathFindingMask mask = m_blockingMask | m_fallingMask;
        return IsoGridPathFinding.FindRange(m_coord, range, grid, mask);
    }

    public async UniTask MoveAgent(LocamotionType locamotion_type, IsoGridCoord target)
    {
        if(target == m_coord)
            return;
        TryGetLocamotion(locamotion_type,out var locamotion);
        var grid = GridManager.ActivePathGrid;
        var tileMask = grid.PathingMaskSingleTile(m_coord);
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

            for (int i = 0; i < waypoints.Count; i++)
            {
                var moveTarget = waypoints[i];
                await locamotion.StartLocamotion(m_coord, moveTarget);
                m_coord = moveTarget;
                m_originCoord = m_coord;
            }
            tileMask = grid.PathingMaskSingleTile(m_coord);
            grid.UpdatePathFindingMask(m_coord, tileMask | m_intrinsicMask);
            m_onReachingTarget.Invoke();
            m_onReachingTarget.RemoveAllListeners();
        }
        Direction = locamotion.Direction;
        m_isMoving = false;
    }


    //Set the preview of the Agent
    public void StartMovePreview(IsoGridCoord target,bool show_visual)
    {
        if (m_movePreviewer == null)
            return;
        var grid = GridManager.ActivePathGrid;
        if (!m_isPreviewing)
        {
            m_originCoord = m_coord;
        }
        grid.ResetMaskBits(m_coord, m_intrinsicMask);
        grid.SetMaskBits(target, m_intrinsicMask);
        m_coord = target;
        m_showPreviewVisual = show_visual;
        if (show_visual)
        {
            //Debug.Log("start preview " + transform.parent.name + "   " + target);
            m_movePreviewer.StartPreview(target.ToWorldPosition(grid));
        }
        m_isPreviewing = true;
    }

    public void StopMovePreview()
    {
        if (m_movePreviewer == null || !m_isPreviewing)
            return;
        var grid = GridManager.ActivePathGrid;
        grid.ResetMaskBits(m_coord, m_intrinsicMask);
        m_coord = m_originCoord;
        grid.SetMaskBits(m_coord, m_intrinsicMask);
        if(m_showPreviewVisual)
            m_movePreviewer.StopPreview();
        m_isPreviewing=false;
        m_showPreviewVisual = false;
    }

    //animation only , not changing the mask
    public async UniTask AnimateAgent(LocamotionType locamotion_type, IsoGridCoord target, float stop_distance)
    {
        if (target == m_coord)
            return;

        TryGetLocamotion(locamotion_type, out var locamotion);
        await locamotion.StartLocamotion(m_coord, target, stop_distance);
    }

    //animation only , not changing the mask
    public async UniTask AnimateAgent(LocamotionType locamotion_type, Vector3 target, float speed_override = 0)
    {
        TryGetLocamotion(locamotion_type, out var locamotion);
        await locamotion.StartLocamotion(target, speed_override);
    }

    //no path finding
    public async UniTask MoveStraight(LocamotionType locamotion_type, IsoGridCoord target, float stop_distance = 0)
    {
        m_isMoving = true;
        var grid = GridManager.ActivePathGrid;
        var tileMask = grid.PathingMaskSingleTile(m_coord);
        grid.UpdatePathFindingMask(m_coord, tileMask ^ m_intrinsicMask);
        TryGetLocamotion(locamotion_type, out var locamotion);
        await locamotion.StartLocamotion(m_coord, target, stop_distance);
        m_coord = target;
        m_originCoord = m_coord;
        tileMask = grid.PathingMaskSingleTile(m_coord);
        grid.UpdatePathFindingMask(m_coord, tileMask | m_intrinsicMask);
        Direction = locamotion.Direction;
        m_isMoving = false;
        m_onReachingTarget.Invoke();
        m_onReachingTarget.RemoveAllListeners();
        await UniTask.Yield();
    }

    //no path finding, no iso grid snapping
    public async UniTask MoveStraight(LocamotionType locamotion_type, Vector3 target,float stop_distance = 0)
    {
        m_isMoving = true;
        TryGetLocamotion(locamotion_type, out var locamotion);
        await locamotion.StartLocamotion(target, stop_distance);
        m_coord = target.ToIsoCoordinate(GridManager.ActivePathGrid);
        m_originCoord = m_coord;
        Direction = locamotion.Direction;
        m_isMoving= false;
        m_onReachingTarget.Invoke();
        m_onReachingTarget.RemoveAllListeners();
        await UniTask.Yield();
    }
}
