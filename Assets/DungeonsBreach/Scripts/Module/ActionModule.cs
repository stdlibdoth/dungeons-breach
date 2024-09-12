using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ActionModule : Module, IAction
{
    [Space]
    [Header("profile")]
    [SerializeField] protected ActionTileProfile m_profile;

    [Space]
    [Header("Animation Data")]
    [SerializeField] protected bool m_animationDataOverride;
    [SerializeField] protected ActionAnimationData m_animationData;

    [Space]
    [Header("Blocking Mask")]
    [SerializeField] protected PathFindingMask m_blockingMask;


    protected UnityEvent<string,bool> m_onActionAvailable = new UnityEvent<string,bool>();
    protected bool m_isAvailable;


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

    public virtual UnityEvent<string,bool> OnActionAvailable{
        get { return m_onActionAvailable;}
    }
    public bool Actived { get;set; }

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

    //public virtual void AnimationDataOverride(ActionAnimationData data)
    //{
    //    m_animationData = new ActionAnimationData 
    //    {
    //        animationState = data.animationState,
    //        animator = data.animator,
    //    };
    //}
}


[System.Serializable]
public class ActionAnimationData
{
    public Animator animator;
    public string animationState;


    public void PlayAnimation()
    {
        animator?.Play(animationState);
    }
}