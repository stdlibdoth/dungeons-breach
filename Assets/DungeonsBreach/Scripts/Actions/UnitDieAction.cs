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
        var tileMask = GridManager.ActivePathGrid.PathingMaskSingleTile(unit.PathAgent.Coordinate);
        GridManager.ActivePathGrid.UpdatePathFindingMask(unit.PathAgent.Coordinate, tileMask ^ unit.PathAgent.IntrinsicMask);
        //gameObject.SetActive(false);
        //m_animator.SetTrigger("Die");
        EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("UnitDie").Invoke(unit);
        yield return new WaitForSeconds(m_param.delay);
        //GameObject.Destroy(unit.gameObject);
        unit.gameObject.SetActive(false);
        yield return null;
    }
}



public class UnitDieActionParam:IActionParam
{
    public UnitBase unit;
    public float delay;
}