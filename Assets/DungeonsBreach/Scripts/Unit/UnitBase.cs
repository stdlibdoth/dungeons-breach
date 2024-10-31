using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DG.Tweening;


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



    [SerializeField] protected SpriteRenderer m_spriteRenderer;

    [Space]
    [Header("unit status")]
    protected UnitStatus m_unitStatus;

    protected List<ActionModule> m_actionModules = new List<ActionModule>();
    protected Sequence m_damangePreviewDOTweeen;
    protected bool m_isDead;

    public bool IsDead {  get { return m_isDead; } }

    public PreviewKey PreviewKey{get;set;}

    public string UnitName
    {
        get { return m_unitName; }
    }
    
    public PathFindingAgent PathAgent
    {
        get { return m_pathAgent; }
    }

    public int MovesAvalaible
    {
        get { return m_unitStatus.moves; }
    }
    public bool IsStationary
    {
        get{return m_unitStatus.stationary !=0;}
    }


    public Module[] Modules
    {
        get { return m_modulesHolder.GetComponentsInChildren<Module>(); }
    }

    protected virtual void Start()
    {
        SpawnUnit();
    }

    protected virtual void SpawnUnit()
    {
        //m_animator.enabled = false;
        m_pathAgent.Init();
        m_unitStatus = PreviewAppliedModuleStatus();
        m_unitStatus.hp = m_unitStatus.maxHP;
        m_unitStatus.moves = m_unitStatus.moveRange;
        RefreshHealthBar(true);
        if(m_animator!= null)
            m_animator.SetFloat("DirBlend", (int)m_pathAgent.Direction);
        RefreshActionModules();
        StartCoroutine(Spawn(m_pathAgent.Coordinate).ExcuteAction());
    }

    public virtual bool GenerateActionAnimationData(string animation_state, out AnimationStateData data)
    {
        data = null;
        if(m_animatorStates.TryGetAnimatorState(animation_state))
        {
            data = new AnimationStateData
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

    public bool TryFetchModule(string module_name, out ActionModule module)
    {
        module = null;
        foreach (var item in m_modulesHolder.GetComponentsInChildren<ActionModule>(true))
        {
            if(item.ModuleName == module_name)
            {
                module = item;
                return true;
            }
        }
        return false;
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
        ClampHP();
        RefreshHealthBar(delta_status.maxHP!=0);
        if (m_unitStatus.hp <= 0)
            Die();
    }

    public virtual void TurnCW(IsoGridDirection relative_dir)
    {
        m_pathAgent.Direction = m_pathAgent.Direction.RotateRelativeTo(relative_dir);
        m_animator?.SetFloat("DirBlend", (int)m_pathAgent.Direction);
    }

    public virtual void SetDirection(IsoGridDirection dir)
    {
        m_pathAgent.Direction = dir;
        m_animator?.SetFloat("DirBlend", (int)m_pathAgent.Direction);
    }

    #endregion

    #region Actions

    public virtual UnitSpawnAction Spawn(IsoGridCoord coord)
    {
        var action = new UnitSpawnAction();
        var param = new SpawnActionParam
        {
            onSpawn = UnitSpawnAnimation,
            unit = this,
            enableNotification = true,
        };
        action.Build(param);
        return action;
    }


    public virtual UnitDamageAction Damage(ActionTileInfo attack_info)
    {
        var action = new UnitDamageAction();
        DamageActionParam param = new DamageActionParam
        {
            animationAction = UnitDamangeAnimation,
            attackInfo = attack_info,
            unit = this,
        };

        UnitDamagePreviewData preview = new UnitDamagePreviewData
        {   
            unitHealthBar = m_healthBar,
            spriteRenderer = m_spriteRenderer,
        };
        action.Build(param);
        action.GeneratePreview(preview);
        return action;
    }

    public virtual MoveAction Move(LocamotionType locamotion, IsoGridCoord target,bool ignore_pathing, bool use_move_point = true)
    {
        var action = new MoveAction();
        var param = new MoveActionParam
        {
            locamotion = locamotion,
            agent = m_pathAgent,
            target = target,
            ignorePathing = ignore_pathing,
        };
        if (use_move_point)
            m_pathAgent.OnReachingTarget.AddListener(() => m_unitStatus.moves = 0);
        m_pathAgent.OnReachingTarget.AddListener(()=>{
            ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).UpdateActionPreview();
        });
        action.Build(param);
        action.GeneratePreview(param);
        return action;
    }

    public virtual void Die()
    {
        if (m_isDead)
            return;

        m_isDead = true;
        UnitDieAction dieAction = new UnitDieAction();
        dieAction.Build(new UnitDieActionParam{
            unit = this,
            delay = 0.3f,
        });
        ActionTurn.RegistorTempAction(dieAction);
    }


    #endregion

    #region helpers

    protected void ClampHP()
    {
        m_unitStatus.hp = math.clamp(m_unitStatus.hp,int.MinValue,m_unitStatus.maxHP);
    }

    protected void RefreshHealthBar(bool init)
    {
        if(m_healthBar == null)
            return;
        if(init)
        {
            m_healthBar.Init(m_unitStatus.maxHP);
        }
        m_healthBar.SetHP(m_unitStatus.hp);
    }


    protected IEnumerator UnitSpawnAnimation()
    {
        if(m_spriteRenderer!= null)
        {
            var color = m_spriteRenderer.color;
            color.a = 0;
            m_spriteRenderer.color = color;
            m_spriteRenderer.DOFade(1,1-0.1f);
        }
        yield return new WaitForSeconds(1);
        if(m_animator !=null)
            m_animator.enabled = true;
    }

    protected IEnumerator UnitDamangeAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(m_spriteRenderer.DOColor(Color.red,0.1f))
        .Append(m_spriteRenderer.DOColor(Color.white,0.1f));
        yield return null;
    }

    #endregion

}
