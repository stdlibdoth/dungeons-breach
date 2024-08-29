using UnityEngine;
using System.Collections.Generic;
using System;

public static class SingletonManager
{
    private static Dictionary<Type,object> m_singltons = new Dictionary<Type, object>();

    public static bool Add<T>(T instance)
    {
        var t = instance.GetType();
        if (m_singltons.ContainsKey(t))
            return false;
        m_singltons.Add(t, instance);
        return true;
    }
}



public abstract class Singleton<T>:MonoBehaviour where T: MonoBehaviour
{
    private static Singleton<T> m_instance;


    protected virtual void Awake()
    {
        m_instance = this;
        if(!SingletonManager.Add(m_instance))
        {
            Destroy(m_instance);
        }
        else
        {
            if(transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }
    }

    protected static T GetSingleton()
    {
        return m_instance as T;
    }

}
