using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class DamageAction : IAction
{
    public ActionPriority Priority { get; set; }

    private ActionTileInfo m_attackInfo;
    private Animator m_animator;
    private UnitBase m_unit;

    public IEnumerator ExcuteAction()
    {
        m_animator.SetTrigger("Damage");

        var deltaStatus = UnitStatus.Empty;
        deltaStatus.hp = -m_attackInfo.value;
        m_unit.UpdateStatus(deltaStatus);

        if(m_unit.CompareTag("PlayerUnit"))
        {
            GameManager.UpdatePlayerStatus(new PlayerStatus{
                maxHP = 0,
                hp = -m_attackInfo.value,
                defence = 0,
            });
        }
        if(m_attackInfo.pushDist > 0)
        {
            var targetTile = m_unit.Agent.Coordinate + m_attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)m_attackInfo.pushDir];
            if (LevelManager.TryGetUnits(targetTile, out var hits))
            {
                var temp = ActionTileInfo.Default;
                float stopDist = GridManager.ActivePathGrid.CellSize / 2;
                Vector3 pos = m_unit.Agent.Coordinate.ToWorldPosition(GridManager.ActivePathGrid);
                yield return m_unit.StartCoroutine(m_unit.Agent.AnimateAgent(LocamotionType.Shift, targetTile, stopDist));
                yield return m_unit.StartCoroutine(m_unit.Agent.AnimateAgent(LocamotionType.Shift, pos, 3));
                m_unit.UpdateStatus(deltaStatus);
                foreach (var hit in hits)
                {
                    hit.Damage(temp, PlayBackMode.Instant);
                }
            }
            else
            {
                //var pushTarget = targetTile + m_attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)m_attackInfo.pushDir];
                m_unit.Move(m_attackInfo.pushType, targetTile, PlayBackMode.Instant, false);
            }
        }
        yield return null;
    }

    public IAction Build<T>(T p) where T : IActionParam
    {
        var param = p as DamageActionParam;
        m_attackInfo = param.attackInfo;
        m_animator = param.animator;
        m_unit = param.unit;
        return this;
    }
}
