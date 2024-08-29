using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAction : IAction
{
    public ActionPriority Priority { get; set; }

    private AttackTileInfo m_attackInfo;
    private Animator m_animator;
    private UnitBase m_unit;

    public IEnumerator ExcuteAction()
    {
        m_animator.SetTrigger("Damage");

        var deltaStatus = UnitStatusBase.Empty;
        deltaStatus.hp = -1;
        m_unit.UpdateStatus(deltaStatus);

        if(m_attackInfo.pushDist > 0)
        {
            var targetTile = m_unit.Agent.Coordinate + m_attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)m_attackInfo.pushDir];
            if (LevelManager.TryGetUnit(targetTile, out var hit))
            {
                var temp = AttackTileInfo.Default;
                //temp.pushDist = 0;
                hit.Damage(temp, PlayBackMode.Instant);
            }
            else
            {
                //var pushTarget = targetTile + m_attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)m_attackInfo.pushDir];
                m_unit.Move(m_attackInfo.pushType, targetTile, PlayBackMode.Instant, false);
            }
        }
        yield return null;
    }

    public IAction Build<T>(T p) where T : IActionParam
    {
        var param = p as DamageActionParam;
        m_attackInfo = param.attackInfo;
        m_animator = param.animator;
        m_unit = param.unit;
        return this;
    }
}
