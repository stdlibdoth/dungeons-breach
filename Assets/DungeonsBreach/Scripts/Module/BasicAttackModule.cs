using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BasicAttackModule : ActionModule
{
    protected List<UnitDamageAction> m_tempDamagePreview = new List<UnitDamageAction>();

    public BasicAttackModule()
    {
        m_previewKey = new PreviewKey(this);
    }

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
        Debug.Log(unit +"  " + ModuleName + "  action");
        await PlayAnimation(unit);
        var confirmed = new List<IsoGridCoord>(m_confirmedActionRange);

        if(m_previewKey == this)
            BattleUIController.ActionPreviewer.ClearPreview(m_previewKey);

        foreach (var attack in m_profile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(unit.PathAgent.Coordinate, unit.PathAgent.Direction);
            if (!confirmed.Contains(coord))
                continue;

            if (LevelManager.TryGetUnits(coord, out var hits))
            {
                var attackInfo = attack;
                attackInfo.pushDir = attack.pushDir.RotateRelativeTo(unit.PathAgent.Direction);
                List<IAction> damageActions = new List<IAction>();
                foreach (var hit in hits)
                {
                    damageActions.Add(hit.Damage(attackInfo));
                }
                damageActions.Sort((a,b)=>new ActionComparer().Compare(a,b));
                for (int i = 0; i < damageActions.Count; i++)
                {
                    Debug.Log(damageActions[i] + "   " + damageActions[i].Priority.value);
                    await damageActions[i].ExcuteAction();
                }
            }
        }
        EventManager.GetTheme<ActionModuleTheme>("ActionModuleTheme").GetTopic("OnModuleExecute").Invoke(this);
    }

    private async UniTask PlayAnimation(UnitBase unit)
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
                data?.animator?.SetFloat("DirBlend", (int)unit.PathAgent.Direction);
            }
        }
        await UniTask.WaitForEndOfFrame(this);
        if (animationData.animator != null)
            await UniTask.WaitUntil(() => !animationData.animator.GetCurrentAnimatorStateInfo(0).IsName(animationData.animationState));    }

    #endregion

    #region IPreviewable

    public override IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data)
    {
        m_previewKey = new PreviewKey(this);
        m_actionParam = data;
        return this;
    }

    public override ActionTileInfo[] StartPreview()
    {
        var unit = m_actionParam.unit;
        var confirmed = new List<IsoGridCoord>(m_confirmedActionRange);
        List<ActionTileInfo> actionInfo = new List<ActionTileInfo>();
        foreach (var attack in m_profile.data)
        {
            IsoGridCoord coord = attack.relativeCoord.OnRelativeTo(unit.PathAgent.Coordinate, unit.PathAgent.Direction);
            if (!confirmed.Contains(coord))
                continue;

            var target = attack.Copy();
            target.relativeCoord = coord;
            BattleUIController.ShowActionTarget(this, target);

            if (LevelManager.TryGetUnits(coord, out var hits))
            {
                var attackInfo = target;
                attackInfo.pushDir = target.pushDir.RotateRelativeTo(unit.PathAgent.Direction);
                foreach (var hit in hits)
                {
                   var action = hit.Damage(attackInfo);
                   action.PreviewKey = PreviewKey;
                   m_tempDamagePreview.Add(action);
                   actionInfo.AddRange(action.StartPreview());
                }
            }
        }
        return actionInfo.ToArray();
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
