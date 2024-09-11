using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ActionModule : Module, IAction
{
    [Space]
    [Header("Animation Data")]
    [SerializeField] protected bool m_animationDataOverride;
    [SerializeField] protected ActionAnimationData m_animationData;


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
    public abstract ActionTileProfile ActionTileProfile { get; }

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