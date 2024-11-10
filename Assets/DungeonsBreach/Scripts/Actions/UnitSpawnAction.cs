using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;


public class UnitSpawnAction : IAction
{
    public ActionPriority Priority { get; set; }
    protected SpawnActionParam m_param;

    public virtual IAction Build<T>(T param) where T : IActionParam
    {
        m_param = param as SpawnActionParam;
        return this;
    }

    public virtual async UniTask ExcuteAction()
    {
        if (m_param.enableNotification)
        {
            EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("UnitSpawn").Invoke(m_param.unit);
        }

        if(m_param.onSpawn!= null)
            await m_param.onSpawn.Invoke();

        await UniTask.Yield();
    }

}

public class SpawnActionParam:IActionParam
{
    public CoroutineDelegate onSpawn;
    public UnitBase unit;
    public bool enableNotification;
}


public delegate UniTask CoroutineDelegate();