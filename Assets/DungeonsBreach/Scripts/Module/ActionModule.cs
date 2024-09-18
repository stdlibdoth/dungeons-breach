using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ActionModule : Module, IAction,IPreviewable<ActionModuleParam>
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


    protected UnityEvent<string,bool> m_onActionAvailable = new UnityEvent<string,bool>();
    protected bool m_isAvailable;

    public bool Actived { get;set; }

    public virtual bool IsAvailable
    {
        get { return m_isAvailable; }
        set
        {
            if(value!= m_isAvailable)
            {
                m_isAvailable = value;
                m_onActionAvailable.Invoke(ModuleName, value);
            }
        }
    }

    public virtual UnityEvent<string,bool> OnActionAvailable
    {
        get { return m_onActionAvailable;}
    }

    public virtual IsoGridCoord[] ActionRange(IsoGridCoord center, IsoGridDirection dir)
    {
        List<IsoGridCoord> range = new List<IsoGridCoord>();
        foreach (var tileInfo in m_profile.data)
        {
            IsoGridCoord coord = tileInfo.relativeCoord.OnRelativeTo(center, dir);
            var grid = GridManager.ActivePathGrid;
            if(grid.CheckRange(coord))
            {
                bool overlap = m_blockingMask.CheckMaskOverlap(grid.PathingMaskSingleTile(coord));
                if(!overlap)
                    range.Add(coord);
            }
        }
        return range.ToArray();
    }

    public abstract ActionPriority Priority { get; set; }
    public abstract IEnumerator ExcuteAction();
    public abstract IAction Build<T>(T param) where T : IActionParam;

    public abstract IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data);

    public abstract IEnumerator StartPreview();

    public abstract void StopPreview();

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
        if(animator == null)
            return;

        bool boo = check_current_state;
        if(check_current_state)
            boo = animator.GetCurrentAnimatorStateInfo(0).IsName(animationState);
        
        if(!boo)
        {
            animator.Play(animationState);
        }
    }
}