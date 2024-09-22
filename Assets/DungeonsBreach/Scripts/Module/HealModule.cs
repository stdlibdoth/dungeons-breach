using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealModule : BasicAttackModule
{
    public override ActionPriority Priority {get; set;}


    #region IAction
    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;
        Debug.Log(unit + "  heal");
        if(m_animationDataOverride)
            m_animationData.PlayAnimation();
        var confirmed = new List<IsoGridCoord>(m_actionParam.confirmedCoord);
        foreach (var attack in m_profile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(unit.Agent.Coordinate, unit.Agent.Direction);
            if (!confirmed.Contains(coord))
                continue;

            if (LevelManager.TryGetUnits(coord, out var hits))
            {
                foreach (var hit in hits)
                {
                    UnitStatus delta = UnitStatus.Empty;
                    delta.hp = attack.value;
                    hit.UpdateStatus(delta);
                }
            }
        }
        yield return null;
    }
    #endregion


    #region IPreviewable

    public override IEnumerator StartPreview()
    {
        yield return base.StartPreview();

        if(m_animationDataOverride)
        {
            AnimationStateData animData = new AnimationStateData{
                animationState = m_animationData.animationState + "Preview",
                animator = m_animationData.animator
            };
            animData.PlayAnimation();
        }
    }

    #endregion
}