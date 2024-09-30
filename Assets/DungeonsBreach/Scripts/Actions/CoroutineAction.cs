using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineAction : IAction
{
    public ActionPriority Priority { get; set; }
    protected  CoroutineActionParam m_param;
    public virtual IAction Build<T>(T param) where T : IActionParam
    {
        m_param = param as CoroutineActionParam;
        return this;
    }

    public virtual IEnumerator ExcuteAction()
    {
        if(m_param!= null)
            yield return m_param.coroutineDelegate.Invoke();
        else
            yield return null;
    }

}



public class CoroutineActionParam:IActionParam
{
    public CoroutineDelegate coroutineDelegate;
}


public delegate IEnumerator CoroutineDelegate();