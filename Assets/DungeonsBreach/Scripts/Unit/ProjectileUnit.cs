using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileUnit : UnitBase
{
    [SerializeField] protected LocamotionType m_locamotionType;

    protected IsoGridCoord m_target;

    protected override void Init()
    {
        base.Init();
        StartCoroutine(StartProjectile());
    }

    public override void Damage(ActionTileInfo attack_info, PlayBackMode mode)
    {
        m_animator.SetTrigger("Damage");

        var deltaStatus = UnitStatus.Empty;
        deltaStatus.hp = -1;
        UpdateStatus(deltaStatus);
    }

    private IEnumerator StartProjectile()
    {
        int dist = m_unitStatus.moveRange;
        int blockDist = GridManager.ActivePathGrid.MaskLineCast(m_pathAgent.BlockingMask, m_pathAgent.Coordinate, m_pathAgent.Direction, dist, out var coord);
        Debug.Log(coord);
        yield return m_pathAgent.MoveStraight(m_locamotionType, coord);

        foreach (var module in m_actionModules)
        {
            var param = new ActionModuleParam
            {
                unit = this,
                confirmedCoord = new IsoGridCoord[] { coord },
            };
//            ActionAvailable = false;
            module.Actived = false;
            module.Build(param);
            m_unitStatus.moves = 0;
            yield return module.ExcuteAction();
        }
    }
}
