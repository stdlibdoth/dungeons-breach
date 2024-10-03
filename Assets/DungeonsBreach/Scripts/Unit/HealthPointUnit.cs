using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class HealthPointUnit : ObstacleUnit
{
    private bool m_isDead;

    public override UnitDamageAction Damage(ActionTileInfo attack_info)
    {
        if (!m_isDead)
            return base.Damage(attack_info);
        else
        {
            return new UnitDamageAction();
        }
    }

    public override void UpdateStatus(UnitStatus delta_status)
    {
        base.UpdateStatus(delta_status);
        if (delta_status.hp < 0)
            GameManager.UpdatePlayerStatus(new PlayerStatus{
                    maxHP = 0,
                    hp = delta_status.hp,
                    defence = 0
                });
        if (m_unitStatus.hp <= 0)
            m_isDead = true;
    }


    public override void Die()
    {
        if (m_isDead)
            return;

        KeepBlockingDieAction dieAction = new KeepBlockingDieAction();
        dieAction.Build(new UnitDieActionParam
        {
            unit = this,
        });
        ActionTurn.RegistorTempAction(dieAction);
        m_healthBar.gameObject.SetActive(false);
    }
}


public class KeepBlockingDieAction : IAction
{
    private UnitDieActionParam m_param;

    public ActionPriority Priority { get; set; }

    public IAction Build<T>(T param) where T : IActionParam
    {
        m_param = param as UnitDieActionParam;
        return this;
    }

    public IEnumerator ExcuteAction()
    {
        var unit = m_param.unit;
        EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("UnitDie").Invoke(unit);
        yield return null;
    }
}