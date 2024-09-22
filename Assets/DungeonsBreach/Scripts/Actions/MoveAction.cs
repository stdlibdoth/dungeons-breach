using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : IAction
{
    private LocamotionType m_locamotion;
    private IsoGridCoord m_target;
    private MoveAgentDelegate m_moveDelegate;


    public ActionPriority Priority { get; set; }

    public IAction Build<T>(T p) where T : IActionParam
    {
        var param = p as MoveActionParam;
        m_locamotion = param.locamotion;
        m_target = param.target;
        m_moveDelegate = param.moveAgentDelegate;
        return this;
    }

    public IEnumerator ExcuteAction()
    {
        yield return m_moveDelegate.Invoke(m_locamotion, m_target);
        //yield return GameManager.DispachCoroutine(m_moveDelegate(m_locamotion, m_target));
    }
}


public class MoveActionParam:IActionParam
{
    public LocamotionType locamotion;
    public IsoGridCoord target;
    public MoveAgentDelegate moveAgentDelegate;
}
