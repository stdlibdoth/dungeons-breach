using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackModule : ActionModule
{
    private ActionModuleParam m_actionParam;
    [SerializeField]private ActionTileProfile m_profile;
    [SerializeField]private Animator m_animator;
    [SerializeField]private string m_animateTrigger;


    public override ActionPriority Priority { get; set; }
    public override ActionTileProfile ActionTileProfile { get { return m_profile; } }

    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;
        Debug.Log(unit + "  attack");
        m_animator?.SetTrigger(m_animateTrigger);
        m_animator?.SetFloat("DirBlend", (int)unit.Agent.Direction);
        Debug.Log(m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
        yield return new WaitForEndOfFrame();
        Debug.Log(m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
        yield return new WaitUntil(() => !m_animator.GetCurrentAnimatorStateInfo(0).IsName(m_animateTrigger));
        var confirmed = new List<IsoGridCoord>(m_actionParam.confirmedCoord);
        foreach (var attack in m_profile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(unit.Agent.Coordinate, unit.Agent.Direction);
            Debug.Log(coord);
            if (!confirmed.Contains(coord))
                continue;

            if (LevelManager.TryGetUnits(coord, out var hits))
            {
                var attackInfo = attack;
                attackInfo.pushDir = attack.pushDir.RotateRelativeTo(unit.Agent.Direction);
                foreach (var hit in hits)
                {
                    hit.Damage(attackInfo, PlayBackMode.Instant);
                }
            }
        }
        yield return null;
    }
}
