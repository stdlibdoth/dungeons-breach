using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public enum UnitDirection
{
    Forward,
    Rightward,
    Back,
    Left,
}


public class UnitBase : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] protected UnitStatusBase m_intrinsicStatus;
    [SerializeField] protected Animator m_animator;
    [SerializeField] protected PathFindingAgent m_pathAgent;


    [Space]
    [Header("unit status")]
    [SerializeField] protected UnitStatusBase m_unitStatus;

    public PathFindingAgent Agent
    {
        get { return m_pathAgent; }
    }

    public int MoveRange
    {
        get { return m_unitStatus.moves; }
    }

    public bool IsAttackingMode { get;set; }

    public AttackProfile AttackProfile
    {
        get
        {
            var attackProfile = new AttackProfile();
            var length = m_unitStatus.attack.data.Length;
            attackProfile.data = new AttackTileInfo[length];
            for (int i = 0; i < length; i++)
            {
                attackProfile.data[i] = m_intrinsicStatus.attack.data[i];
            }
            return attackProfile;
        }
    }

    protected void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        m_pathAgent.Init();
        m_unitStatus = UnitStatusBase.Empty;
        UpdateStatus(m_intrinsicStatus);
        m_unitStatus.hp = m_unitStatus.maxHP;
        m_unitStatus.moves = m_unitStatus.moveRange;
        m_unitStatus.attackPoints = 1;
        m_animator.SetFloat("DirBlend", (int)m_pathAgent.Direction);
        LevelManager.AddUnit(this);
    }


    public void ResetActions()
    {
        m_unitStatus.moves = m_unitStatus.moveRange;
        m_unitStatus.attackPoints = 1;
        
    }

    public virtual void UpdateStatus(UnitStatusBase delta_status)
    {
        m_unitStatus.Update(delta_status);
        if (m_unitStatus.hp <= 0)
            Die();
    }

    public virtual void TurnCW(IsoGridDirection relative_dir)
    {
        m_pathAgent.Direction = m_pathAgent.Direction.RotateRelativeTo(relative_dir);
        m_animator.SetFloat("DirBlend", (int)m_pathAgent.Direction);
    }

    public virtual void SetDirection(IsoGridDirection dir)
    {
        m_pathAgent.Direction = dir;
        m_animator.SetFloat("DirBlend", (int)m_pathAgent.Direction);
    }

    public virtual void Attack(PlayBackMode mode)
    {
        var action = new AttackAction();
        var param = new AttackActionParam
        {
            animator = m_animator,
            profile = m_unitStatus.attack,
            unit = this,
        };
        action.Build(param);
        m_unitStatus.attackPoints = 0;
        BattleManager.RegistorAction(action, mode);
    }


    public virtual void Damage(AttackTileInfo attack_info, PlayBackMode mode)
    {
        var action = new DamageAction();
        DamageActionParam param = new DamageActionParam
        {
            animator = m_animator,
            attackInfo = attack_info,
            unit = this,
        };
        action.Build(param);
        BattleManager.RegistorAction(action, mode);
    }


    public virtual void Move(LocamotionType locamotion, IsoGridCoord target, PlayBackMode mode, bool use_move_point = true)
    {
        var action = new MoveAction();
        var param = new MoveActionParam
        {
            locamotion = locamotion,
            moveAgentDelegate = m_pathAgent.MoveAgent,
            target = target,
        };
        if (use_move_point)
            m_pathAgent.OnReachingTarget.AddListener(() => m_unitStatus.moves = 0);
        action.Build(param);
        BattleManager.RegistorAction(action, mode);
    }

    public virtual void Die()
    {
        var tileMask = GridManager.ActivePathGrid.PathFindingTileMask(Agent.Coordinate);
        GridManager.ActivePathGrid.UpdatePathFindingMask(Agent.Coordinate, tileMask ^ m_pathAgent.IntrinsicMask);
        gameObject.SetActive(false);
        //m_animator.SetTrigger("Die");
        LevelManager.RemoveUnit(this);
    }


}
