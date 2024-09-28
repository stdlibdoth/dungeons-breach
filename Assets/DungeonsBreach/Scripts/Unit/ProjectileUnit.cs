using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileUnit : UnitBase
{
    [SerializeField] protected LocamotionType m_locamotionType;
    [SerializeField] protected ActionModule m_actionModule;

    public int TravelRange
    {
        get{ return m_intrinsicStatus.moveRange;}
    }

    public ActionModule ActionModule
    {
        get{return m_actionModule;}
    }

    protected override void SpawnUnit()
    {
        base.SpawnUnit();
        StartCoroutine(StartProjectile());
    }

    public override UnitDamageAction Damage(ActionTileInfo attack_info)
    {
        var action = new SelfDamageAction();
        DamageActionParam param = new DamageActionParam
        {
            attackInfo = attack_info,
            unit = this,
        };
        return action.Build(param) as UnitDamageAction;
    }


    private IEnumerator StartProjectile()
    {
        int dist = m_unitStatus.moveRange;
        int blockDist = GridManager.ActivePathGrid.MaskLineCast(m_pathAgent.BlockingMask, m_pathAgent.Coordinate, m_pathAgent.Direction, dist, out var coord);
        yield return m_pathAgent.MoveStraight(m_locamotionType, coord);

        foreach (var module in m_actionModules)
        {
            var param = new ActionModuleParam
            {
                unit = this,
                confirmedCoord = new IsoGridCoord[] { coord },
            };
            module.Actived = false;
            module.Build(param);
            m_unitStatus.moves = 0;
            yield return module.ExcuteAction();
        }
    }
    
    public override void Die()
    {
        UnitDieAction dieAction = new UnitDieAction();
        dieAction.Build(new UnitDieActionParam{
            unit = this,
        });
        BattleManager.RegistorAction(dieAction,PlayBackMode.Instant);
    }
}
