using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class HealModule : BasicAttackModule
{
    #region IAction
    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        UnitStatus deltaStatus = UnitStatus.Empty;
        deltaStatus.moves = -m_actionParam.unit.MovesAvalaible;
        m_actionParam.unit.UpdateStatus(deltaStatus);
        return this;
    }

    public override async UniTask ExcuteAction()
    {
        var unit = m_actionParam.unit;

        if (m_animationDataOverride)
            m_animationData.PlayAnimation();
        var confirmed = new List<IsoGridCoord>(m_confirmedActionRange);
        foreach (var attack in m_profile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(unit.PathAgent.Coordinate, unit.PathAgent.Direction);
            if (!confirmed.Contains(coord))
                continue;

            if (LevelManager.TryGetUnits(coord, out var hits))
            {
                foreach (var hit in hits)
                {
                    UnitStatus delta = UnitStatus.Empty;
                    delta.hp = -attack.value;
                    hit.UpdateStatus(delta);
                }
            }
        }
        EventManager.GetTheme<ActionModuleTheme>("ActionModuleTheme").GetTopic("OnModuleExecute").Invoke(this);
        await UniTask.Yield();
    }
    #endregion


    #region IPreviewable

    public override ActionTileInfo[] StartPreview()
    {
        var info = base.StartPreview();

        if(m_animationDataOverride)
        {
            AnimationStateData animData = new AnimationStateData{
                animationState = m_animationData.animationState + "Preview",
                animator = m_animationData.animator
            };
            animData.PlayAnimation();
        }
        return info;
    }

    #endregion
}