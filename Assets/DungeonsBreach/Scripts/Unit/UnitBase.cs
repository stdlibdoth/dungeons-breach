using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


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
    [SerializeField] protected string m_unitName;
    [SerializeField] protected UnitStatus m_intrinsicStatus;
    [SerializeField] protected Animator m_animator;
    [SerializeField] protected PathFindingAgent m_pathAgent;

    [SerializeField] protected Transform m_modulesHolder;
    [SerializeField] protected SerializedAnimatorStates m_animatorStates;
    [SerializeField] protected UnitHealthBar m_healthBar;

    [Space]
    [Header("unit status")]
    [SerializeField] protected UnitStatus m_unitStatus;

    protected List<ActionModule> m_actionModules = new List<ActionModule>();



    // protected UnityEvent<UnitStatus> m_onStatusChange = new UnityEvent<UnitStatus>();


    public string UnitName
    {
        get { return m_unitName; }
    }
    
    public PathFindingAgent Agent
    {
        get { return m_pathAgent; }
    }

    public int MovesAvalaible
    {
        get { return m_unitStatus.moves; }
    }


    public Module[] Modules
    {
        get { return m_modulesHolder.GetComponentsInChildren<Module>(); }
    }

    // public UnityEvent<UnitStatus> OnStatusChange
    // {
    //     get { return m_onStatusChange;}
    // }


    protected void Start()
    {
        Spawn();
    }

    protected virtual void Spawn()
    {
        m_pathAgent.Init();
        m_unitStatus = PreviewAppliedModuleStatus();
        m_unitStatus.hp = m_unitStatus.maxHP;
        m_unitStatus.moves = m_unitStatus.moveRange;
        RefreshHealthBar(true);
        m_animator.SetFloat("DirBlend", (int)m_pathAgent.Direction);
        RefreshActionModules();
        LevelManager.AddUnit(this);
        EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("UnitSpawn").Invoke(this);
    }

    public virtual bool GenerateActionAnimationData(string animation_state, out ActionAnimationData data)
    {
        data = null;
        if(m_animatorStates.TryGetAnimatorState(animation_state))
        {
            data = new ActionAnimationData
            {
                animationState = animation_state,
                animator = m_animator,
            };
            return true;
        } 
        return false;
    }



    #region Modules

    protected virtual void RefreshActionModules()
    {
        m_actionModules.Clear();
        foreach (var item in m_modulesHolder.GetComponentsInChildren<ActionModule>(true))
        {
            m_actionModules.Add(item);
        }
    }

    // protected virtual void ApplyModuleStatus()
    // {
    //     var initial = m_intrinsicStatus;
    //     foreach (var module in Modules)
    //     {
    //         initial = module.Modified(initial);
    //     }
    //     m_unitStatus = initial;
    //     //m_onStatusChange.Invoke(m_unitStatus);
    // }

    public ActionModule[] ActionModules()
    {
        return m_actionModules.ToArray();
    }

    // public void EquipModule(Module module)
    // {
    //     module.transform.SetParent(m_modulesHolder.transform, false);
    //     if(module is ActionModule)
    //     {
    //         m_actionModules.Add((ActionModule)module);
    //     }
    //     RefreshModuleStatus();
    // }

    // public void RemoveModule(string module_id)
    // {
    //     Module m = null;
    //     foreach (var module in Modules)
    //     {
    //         if (module.ModuleName == module_id)
    //         {
    //             m = module;
    //             break;
    //         }
    //     }
    //     if (m != null)
    //     {
    //         m_actionModules.Remove(m as ActionModule);
    //         Destroy(m);
    //     }
    //     RefreshModuleStatus();
    // }

    public bool ActivedModule(out ActionModule module)
    {
        module = null;
        foreach (var m in m_actionModules)
        {
            if (m.Actived)
            {
                module = m;
                return true;
            }
        }
        return false;
    }

    #endregion


    #region Public Methods

    public UnitStatus PreviewAppliedModuleStatus()
    {
        var initial = m_intrinsicStatus;
        foreach (var module in Modules)
        {
            initial = module.Modified(initial);
        }
        return initial;
    }

    public void ResetActions()
    {
        m_unitStatus.moves = m_unitStatus.moveRange;
        foreach (var item in m_actionModules)
        {
            item.IsAvailable = true;
        }
    }

    public virtual void UpdateStatus(UnitStatus delta_status)
    {
        m_unitStatus += delta_status;
        //m_onStatusChange.Invoke(m_unitStatus);
        RefreshHealthBar(delta_status.maxHP!=0);
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

    public virtual void ModuleAction(string module_id, IsoGridCoord[] confirmed_coord,PlayBackMode mode)
    {
        foreach (var module in m_actionModules)
        {
            Debug.Log(module_id + " : " + module.ModuleName);
            if(module.ModuleName == module_id)
            {
                var param = new ActionModuleParam
                {
                    unit = this,
                    confirmedCoord = confirmed_coord,
                };
                module.Actived = false;
                BattleManager.RegistorAction(module.Build(param), mode);
                m_unitStatus.moves = 0;
                break;
            }
        }
    }

    public virtual void Damage(ActionTileInfo attack_info, PlayBackMode mode)
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
        var tileMask = GridManager.ActivePathGrid.PathingMaskSingleTile(Agent.Coordinate);
        GridManager.ActivePathGrid.UpdatePathFindingMask(Agent.Coordinate, tileMask ^ m_pathAgent.IntrinsicMask);
        //gameObject.SetActive(false);
        //m_animator.SetTrigger("Die");
        LevelManager.RemoveUnit(this);
        EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("UnitDie").Invoke(this);
        Destroy(gameObject);
    }

    #endregion

    #region helpers
    private void RefreshHealthBar(bool init)
    {
        if(m_healthBar == null)
            return;
        if(init)
        {
            m_healthBar.Init(m_unitStatus.maxHP);
        }
        m_healthBar.SetHP(m_unitStatus.hp);
    }


    #endregion

}
