using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectileUnit : UnitBase
{
    [SerializeField] protected LocamotionType m_locamotionType;

    protected IsoGridCoord[] m_targets;

    protected override void Init()
    {
        base.Init();
        StartCoroutine(StartTrajectile());
    }

    public virtual void SetTargets(IsoGridCoord[] targets)
    {
        m_targets = new IsoGridCoord[targets.Length];
        targets.CopyTo(m_targets, 0);
    }


    public override void Damage(ActionTileInfo attack_info, PlayBackMode mode)
    {
        m_animator.SetTrigger("Damage");

        var deltaStatus = UnitStatus.Empty;
        deltaStatus.hp = -1;
        UpdateStatus(deltaStatus);
    }

    private IEnumerator StartTrajectile()
    {
        foreach (var t in m_targets)
        {
            yield return MoveAndAction(t);
        }
    }


    private IEnumerator MoveAndAction(IsoGridCoord coord)
    {
        yield return m_pathAgent.MoveStraight(m_locamotionType, coord);
        foreach (var module in m_actionModules)
        {
            var param = new ActionModuleParam
            {
                unit = this,
                confirmedCoord = new IsoGridCoord[] { coord }
            };
            ActionAvailable = false;
            module.Actived = false;
            module.Build(param);
            m_unitStatus.moves = 0;
            yield return module.ExcuteAction();
        }
    }
}
