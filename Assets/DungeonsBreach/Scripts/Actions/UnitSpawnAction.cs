using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawnAction : IAction
{
    public ActionPriority Priority { get; set; }
    protected SpawnActionParam m_param;

    public virtual IAction Build<T>(T param) where T : IActionParam
    {
        m_param = param as SpawnActionParam;
        return this;
    }

    public virtual IEnumerator ExcuteAction()
    {
        if (m_param.enableNotification)
        {
            EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("UnitSpawn").Invoke(m_param.unit);
        }

        if(m_param.onSpawn!= null)
            yield return m_param.onSpawn.Invoke();

        yield return null;
    }

}

public class SpawnActionParam:IActionParam
{
    public CoroutineDelegate onSpawn;
    public UnitBase unit;
    public bool enableNotification;
}


public delegate IEnumerator CoroutineDelegate();