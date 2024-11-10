using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TrajectileUnit : UnitBase
{
    [SerializeField] protected LocamotionType m_locamotionType;
    [SerializeField] protected ActionModule m_actionModule;
    [SerializeField] protected TrajectoryLocamotion m_trajectileLocamotion;

    public ActionModule ActionModule
    {
        get{return m_actionModule;}
    }

    public Trajectory2D GetTrajectory(Vector3 start, Vector3 end)
    {
        return m_trajectileLocamotion.GetTrajectory(start,end);
    }

    protected IsoGridCoord[] m_targets;

    protected override void Start()
    {

    }

    public virtual void SetTargets(IsoGridCoord[] targets)
    {
        m_targets = new IsoGridCoord[targets.Length];
        targets.CopyTo(m_targets, 0);
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

    public async UniTask StartTrajectile()
    {
        base.SpawnUnit();
        foreach (var t in m_targets)
        {
            await MoveAndAction(t);
        }
    }


    private async UniTask MoveAndAction(IsoGridCoord coord)
    {
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
        BattleUIController.ActionPreviewer.ClearPreview(PreviewKey);
        dieAction.Build(new UnitDieActionParam{
            unit = this,
        });
        await dieAction.ExcuteAction();
    }
}
