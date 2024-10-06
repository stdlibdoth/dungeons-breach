using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ActionModule : Module, IAction, IPreviewable<ActionModuleParam>
{
    [Space]
    [Header("profile")]
    [SerializeField] protected ActionTileProfile m_profile;

    [Space]
    [Header("Animation Data")]
    [SerializeField] protected bool m_animationDataOverride;
    [SerializeField] protected AnimationStateData m_animationData;

    [Space]
    [Header("Blocking Mask")]
    [SerializeField] protected PathFindingMask m_blockingMask;


    protected UnityEvent<string, bool> m_onActionAvailable = new UnityEvent<string, bool>();
    protected bool m_isAvailable;
    protected PreviewKey m_previewKey;
    protected PreviewKey m_lastKey;

    protected IsoGridCoord[] m_confirmedActionRange;
    protected ActionModuleParam m_actionParam;


    public ActionModuleParam ActionParam
    {
        get{return m_actionParam;}
    }

    public bool Actived { get; set; }

    public virtual PreviewKey PreviewKey
    {
        get { return m_previewKey; }
        set
        {
            m_lastKey = m_previewKey;
            m_previewKey = value;
        }
    }

    public virtual bool IsAvailable
    {
        get { return m_isAvailable; }
        set
        {
            if (value != m_isAvailable)
            {
                m_isAvailable = value;
                m_onActionAvailable.Invoke(ModuleName, value);
            }
        }
    }

    public virtual UnityEvent<string, bool> OnActionAvailable
    {
        get { return m_onActionAvailable; }
    }

    public ActionTileProfile Profile
    {
        get { return m_profile; }
    }


    // public virtual IsoGridCoord[] ActionTarget()
    // {
    //     List<IsoGridCoord> targets = new List<IsoGridCoord>();
    //     var range = ActionRangeInternal(m_actionParam.unit.Agent.Coordinate, m_actionParam.unit.Agent.Direction);
    //     for (int i = 0; i < m_confirmedActionRange.Length; i++)
    //     {
    //         for (int j = 0; j < range.Length; j++)
    //         {
    //             if (m_confirmedActionRange[i] == range[j])
    //                 targets.Add(m_confirmedActionRange[i]);
    //         }
    //     }
    //     return targets.ToArray();
    // }

    public IsoGridCoord[] ActionRange()
    {
        return ActionRangeInternal(m_actionParam.unit.Agent.Coordinate, m_actionParam.unit.Agent.Direction);
    }

    public virtual void ResetPreviewKey()
    {
        m_previewKey = m_lastKey;
    }

    public virtual IsoGridCoord[] ConfirmActionTargets()
    {
        var actionRange = ActionRangeInternal(m_actionParam.unit.Agent.Coordinate, m_actionParam.unit.Agent.Direction);
        List<IsoGridCoord> confirmedTargets = new List<IsoGridCoord>();
        foreach (var inputCoord in m_actionParam.actionInputCoords)
        {
            foreach (var coord in actionRange)
            {
                if (coord == inputCoord)
                {
                    confirmedTargets.Add(inputCoord);
                    break;
                }
            }
        }
        m_confirmedActionRange = confirmedTargets.ToArray();
        return confirmedTargets.ToArray();
    }

    public ActionPriority Priority { get; set; }

    public abstract IEnumerator ExcuteAction();
    public abstract IAction Build<T>(T param) where T : IActionParam;

    public abstract IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data);

    public abstract IEnumerator StartPreview();

    public abstract void StopPreview();


    protected virtual IsoGridCoord[] ActionRangeInternal(IsoGridCoord center, IsoGridDirection dir)
    {
        List<IsoGridCoord> range = new List<IsoGridCoord>();
        foreach (var tileInfo in m_profile.data)
        {
            IsoGridCoord coord = tileInfo.relativeCoord.OnRelativeTo(center, dir);
            var grid = GridManager.ActivePathGrid;
            if (grid.CheckRange(coord))
            {
                bool overlap = m_blockingMask.CheckMaskOverlap(grid.PathingMaskSingleTile(coord));
                if (!overlap)
                    range.Add(coord);
            }
        }
        return range.ToArray();
    }

}

public class ActionModuleParam : IActionParam
{
    public UnitBase unit;

    public IsoGridCoord[] actionInputCoords;

    public IsoGridCoord[] WorldCoord
    {
        get
        {
            IsoGridCoord[] worldCoord = new IsoGridCoord[actionInputCoords.Length];
            for (int i = 0; i < actionInputCoords.Length; i++)
            {
                worldCoord[i] = actionInputCoords[i].OnRelativeTo(unit.Agent.Coordinate, unit.Agent.Direction);
            }
            return worldCoord;
        }
    }
}



[System.Serializable]
public class AnimationStateData
{
    public Animator animator;
    public string animationState;

    public void StopAnimation()
    {
        animator?.Play("Idle");
    }

    public void PlayAnimation(bool check_current_state = false)
    {
        if (animator == null)
            return;

        bool boo = check_current_state;
        if (check_current_state)
            boo = animator.GetCurrentAnimatorStateInfo(0).IsName(animationState);

        if (!boo)
        {
            animator.Play(animationState);
        }
    }
}