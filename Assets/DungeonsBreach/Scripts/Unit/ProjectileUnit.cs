using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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

    protected override void Start()
    {

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


    public override UnitSpawnAction Spawn(IsoGridCoord coord)
    {
        var action = new UnitSpawnAction();
        var param = new SpawnActionParam
        {
            onSpawn = null,
            unit = this,
            enableNotification = true,
        };
        action.Build(param);
        return action;
    }


    public async UniTask StartProjectile()
    {
        base.SpawnUnit();
        int dist = m_unitStatus.moveRange;
        int blockDist = GridManager.ActivePathGrid.MaskLineCast(m_pathAgent.BlockingMask, m_pathAgent.Coordinate, m_pathAgent.Direction, dist, out var coord);
        await m_pathAgent.MoveStraight(m_locamotionType, coord);

        foreach (var module in m_actionModules)
        {
            var param = new ActionModuleParam(this,new IsoGridCoord[] { coord },false);
            module.Actived = false;
            module.Build(param);
            module.ConfirmActionTargets();
            m_unitStatus.moves = 0;
            await module.ExcuteAction();
        }
    }
    
    public override async void Die()
    {
        UnitDieAction dieAction = new UnitDieAction();
        dieAction.Build(new UnitDieActionParam{
            unit = this,
        });
        BattleUIController.ActionPreviewer.ClearPreview(PreviewKey);
        await dieAction.ExcuteAction();
    }
}
