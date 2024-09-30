using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackModule : ActionModule
{
    protected ActionModuleParam m_actionParam;

    protected List<UnitDamageAction> m_tempDamagePreview = new List<UnitDamageAction>();

#region IAction
    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        UnitStatus deltaStatus = UnitStatus.Empty;
        deltaStatus.moves = -m_actionParam.unit.MovesAvalaible;
        m_actionParam.unit.UpdateStatus(deltaStatus);
        Actived = false;
        IsAvailable = false;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;
        Debug.Log(unit +"  " + ModuleName + "  action");
        yield return PlayAnimation(unit);
        var confirmed = new List<IsoGridCoord>(m_actionParam.confirmedCoord);

        if((object)m_previewKey == this)
            BattleUIController.ActionPreviewer.ClearPreview(this);

        foreach (var attack in m_profile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(unit.Agent.Coordinate, unit.Agent.Direction);
            if (!confirmed.Contains(coord))
                continue;

            if (LevelManager.TryGetUnits(coord, out var hits))
            {
                var attackInfo = attack;
                attackInfo.pushDir = attack.pushDir.RotateRelativeTo(unit.Agent.Direction);
                foreach (var hit in hits)
                {
                    StartCoroutine(hit.Damage(attackInfo).ExcuteAction());
                }
            }
        }
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

    #endregion

    #region IPreviewable

    public override void ResetPreviewKey()
    {
        foreach (var item in m_tempDamagePreview)
        {
            item.ResetPreviewKey();
        }
    }

    public override IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data)
    {
        m_previewKey = this;
        m_actionParam = data;
        return this;
    }

    public override IEnumerator StartPreview()
    {
        var unit = m_actionParam.unit;
        var confirmed = new List<IsoGridCoord>(m_actionParam.confirmedCoord);
        foreach (var attack in m_profile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(unit.Agent.Coordinate, unit.Agent.Direction);
            if (!confirmed.Contains(coord))
                continue;

            if (LevelManager.TryGetUnits(coord, out var hits))
            {
                var attackInfo = attack;
                attackInfo.pushDir = attack.pushDir.RotateRelativeTo(unit.Agent.Direction);
                foreach (var hit in hits)
                {
                   var action = hit.Damage(attackInfo);
                   action.PreviewKey = PreviewKey;
                   m_tempDamagePreview.Add(action);
                   yield return action.StartPreview();
                }
            }
        }
        yield return null;
    }

    public override void StopPreview()
    {
        foreach (var item in m_tempDamagePreview)
        {
            item.StopPreview();
        }
        m_tempDamagePreview.Clear();
    }

    #endregion
}
