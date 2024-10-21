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
        get { return m_actionParam; }
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

    public IsoGridCoord[] ActionRange()
    {
        return ActionRangeInternal(m_actionParam.unit.PathAgent.Coordinate, m_actionParam.unit.PathAgent.Direction);
    }

    public virtual void ResetPreviewKey()
    {
        m_previewKey = m_lastKey;
    }

    public virtual IsoGridCoord[] ConfirmActionTargets()
    {
        var actionRange = ActionRangeInternal(m_actionParam.unit.PathAgent.Coordinate, m_actionParam.unit.PathAgent.Direction);
        List<IsoGridCoord> confirmedTargets = new List<IsoGridCoord>();
        foreach (var inputCoord in m_actionParam.GetInputCoordinates(false))
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

    public abstract void StopDamagePreview();


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

    //relative to action module
    private IsoGridCoord[] actionInputCoords;

    public ActionModuleParam(UnitBase unit, IsoGridCoord[] coords, bool is_relative)
    {
        this.unit = unit;
        actionInputCoords = new IsoGridCoord[coords.Length];
        SetActionInputCoordinatesInternal(coords, is_relative);
    }

    public IsoGridCoord[] GetInputCoordinates(bool is_relative)
    {
        IsoGridCoord[] coords = new IsoGridCoord[actionInputCoords.Length];
        actionInputCoords.CopyTo(coords, 0);
        if (!is_relative)
        {
            for (int i = 0; i < actionInputCoords.Length; i++)
            {
                coords[i] = actionInputCoords[i].OnRelativeTo(unit.PathAgent.Coordinate, unit.PathAgent.Direction);
            }
        }
        return coords;
    }

    private void SetActionInputCoordinatesInternal(IsoGridCoord[] input, bool is_relative)
    {
        if (is_relative)
        {
            for (int i = 0; i < input.Length; i++)
            {
                actionInputCoords[i] = input[i];
            }
        }
        else
        {
            for (int i = 0; i < input.Length; i++)
            {
                actionInputCoords[i] = input[i] - unit.PathAgent.Coordinate;
                int turnCount = IsoGridMetrics.directionCount - (int)unit.PathAgent.Direction;
                actionInputCoords[i] = actionInputCoords[i].RotateCCW(turnCount);
            }
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