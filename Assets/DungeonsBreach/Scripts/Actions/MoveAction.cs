using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : IAction
{
    private LocamotionType m_locamotion;
    private IsoGridCoord m_target;
    private PathFindingAgent m_agent;




    public ActionPriority Priority { get; set; }

    public IAction Build<T>(T p) where T : IActionParam
    {
        var param = p as MoveActionParam;
        m_locamotion = param.locamotion;
        m_target = param.target;
        m_agent = param.agent;
        return this;
    }

    public IEnumerator ExcuteAction()
    {
        yield return m_agent.MoveAgent(m_locamotion, m_target);
    }
}


public class MoveActionParam:IActionParam
{
    public LocamotionType locamotion;
    public IsoGridCoord target;
    public PathFindingAgent agent;
}
