using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : IAction
{
    private Animator m_animator;
    private AttackProfile m_attackProfile;
    private UnitBase m_unit;

    public ActionPriority Priority { get; set; }


    public IEnumerator ExcuteAction()
    {
        Debug.Log(m_unit + "  attack");
        m_animator?.SetTrigger("Attack");
        m_animator?.SetFloat("DirBlend", (int)m_unit.Agent.Direction);
        yield return new WaitForEndOfFrame();
        var stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName("Attack"))
        {
            stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        foreach (var attack in m_attackProfile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(m_unit.Agent.Coordinate, m_unit.Agent.Direction);
            Debug.Log(m_unit.Agent.Direction);
            Debug.Log(m_unit.Agent.Coordinate);
            Debug.Log(coord);
            if (LevelManager.TryGetUnit(coord,out var hit))
            {
                var attackInfo = attack;
                attackInfo.pushDir = attack.pushDir.RotateRelativeTo(m_unit.Agent.Direction);
                hit.Damage(attackInfo, PlayBackMode.Instant);
            }
        }
        yield return null;
    }

    public IAction Build<T>(T param) where T : IActionParam
    {
        AttackActionParam ap = param as AttackActionParam;
        m_attackProfile = ap.profile;
        m_animator = ap.animator;
        m_unit = ap.unit;
        return this;
    }
}
