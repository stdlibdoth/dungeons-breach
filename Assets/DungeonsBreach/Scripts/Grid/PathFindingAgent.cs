using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathFindingAgent
{
    [SerializeField]private PathFindingMask m_intrinsicMask;
    [SerializeField]private PathFindingMask m_blockingMask;


    [SerializeField] private IsoGridCoord m_start;
    [SerializeField] private IsoGridCoord m_target;


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


    //public List<IsoGridCoord> FindPath(IsoGridCoord start, IsoGridCoord target)
    //{
    //    m_start = start;
    //    m_target = target;

    //}



}
