using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public IEnumerator StartTrajectile()
    {
        base.SpawnUnit();
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
                confirmedCoord = new IsoGridCoord[] { coord },
            };
            module.Actived = false;
            module.Build(param);
            m_unitStatus.moves = 0;
            yield return module.ExcuteAction();
        }
    }

    // public override UnitSpawnAction Spawn(IsoGridCoord coord)
    // {
    //     return new UnitSpawnAction();
    // }

    public override void Die()
    {
        UnitDieAction dieAction = new UnitDieAction();
        Debug.Log("previewkey: " + PreviewKey.GetHashCode() + "  " + gameObject.name);
        BattleUIController.ActionPreviewer.ClearPreview(PreviewKey);
        dieAction.Build(new UnitDieActionParam{
            unit = this,
        });
        StartCoroutine(dieAction.ExcuteAction());
    }
}
