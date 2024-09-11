using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackModule : ActionModule
{
    private ActionModuleParam m_actionParam;
    [Space]
    [SerializeField]private ActionTileProfile m_profile;

    public override ActionPriority Priority { get; set; }
    public override ActionTileProfile ActionTileProfile { get { return m_profile; } }

    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        IsAvailable = false;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;
        Debug.Log(unit + "  attack");
        yield return PlayAnimation(unit);
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


    private IEnumerator PlayAnimation(UnitBase unit)
    {
        var animationData = m_animationData;
        if (m_animationDataOverride)
        {
            m_animationData?.PlayAnimation();
        }
        else
        {
            if (unit.GenerateActionAnimationData("Attack", out var data))
            {
                animationData = data;
                data?.PlayAnimation();
                data?.animator?.SetFloat("DirBlend", (int)unit.Agent.Direction);
            }
        }
        yield return new WaitForEndOfFrame();
        if (animationData.animator != null)
            yield return new WaitUntil(() => !animationData.animator.GetCurrentAnimatorStateInfo(0).IsName(animationData.animationState));
    }
}
