using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : IAction
{
    private MoveActionParam m_param;

    public ActionPriority Priority { get; set; }

    public IAction Build<T>(T p) where T : IActionParam
    {
        m_param = p as MoveActionParam;
        return this;
    }

    public IEnumerator ExcuteAction()
    {
        if (m_param.ignorePathing)
        {
            var agent = m_param.agent;
            yield return agent.MoveStraight(m_param.locamotion, m_param.target);
            yield return new WaitForSeconds(0.3f);

            var grid = GridManager.ActivePathGrid;
            var tileMask = grid.PathingMaskSingleTile(agent.Coordinate);

            //check falling
            if (agent.FallMask.CheckMaskOverlap(tileMask))
            {
                Vector3 target = agent.transform.position + Vector3.down * grid.CellSize * 0.5f;
                yield return agent.AnimateAgent(LocamotionType.Shift, target);

                LevelManager.TryGetUnits(agent.Coordinate, out List<UnitBase> units);
                foreach (var unit in units)
                {
                    if(unit.Agent ==  agent)
                    {
                        var delta = UnitStatus.Empty;
                        delta.hp = int.MinValue;
                        unit.UpdateStatus(delta);
                    }
                }
            }
        }
        else
            yield return m_param.agent.MoveAgent(m_param.locamotion, m_param.target);
    }
}


public class MoveActionParam:IActionParam
{
    public LocamotionType locamotion;
    public IsoGridCoord target;
    public PathFindingAgent agent;
    public bool ignorePathing;
}
