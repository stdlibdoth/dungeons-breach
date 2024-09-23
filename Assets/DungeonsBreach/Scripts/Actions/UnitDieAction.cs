using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDieAction : IAction
{
    private UnitDieActionParam m_param;

    public ActionPriority Priority { get;set;}

    public IAction Build<T>(T param) where T : IActionParam
    {
        m_param = param as UnitDieActionParam;
        return this;
    }

    public IEnumerator ExcuteAction()
    {
        var unit = m_param.unit;
        var tileMask = GridManager.ActivePathGrid.PathingMaskSingleTile(unit.Agent.Coordinate);
        GridManager.ActivePathGrid.UpdatePathFindingMask(unit.Agent.Coordinate, tileMask ^ unit.Agent.IntrinsicMask);
        //gameObject.SetActive(false);
        //m_animator.SetTrigger("Die");
        LevelManager.RemoveUnit(unit);
        EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("UnitDie").Invoke(unit);
        GameObject.Destroy(unit.gameObject);
        yield return null;
    }
}



public class UnitDieActionParam:IActionParam
{
    public UnitBase unit;
}

