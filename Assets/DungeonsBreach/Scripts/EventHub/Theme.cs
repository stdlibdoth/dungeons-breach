using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct EventInfo
{
    public string theme;
    public string topic;

    public EventInfo(string theme, string topic)
    {
        this.theme = theme;
        this.topic = topic;
    }
}

[System.Serializable]
public class Theme : ThemeBase
{
    [SerializeField] private List<Topic> m_topics;
    private Dictionary<string, UnityEvent> m_handlers;

    private void Awake()
    {
        if(m_handlers== null)
            m_handlers = new Dictionary<string, UnityEvent>();

#if UNITY_EDITOR
        if (m_topics == null)
            m_topics = new List<Topic>();
        for (int i = 0; i < m_topics.Count; i++)
        {
            if (!m_handlers.ContainsKey(m_topics[i].topicName))
                m_handlers[m_topics[i].topicName] = m_topics[i].handler;
        }
#endif
    }

    public void Subscribe(string topic, UnityAction action)
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent>();
        if (!m_handlers.ContainsKey(topic))
            m_handlers[topic] = new UnityEvent();
        m_handlers[topic].AddListener(action);

//#if UNITY_EDITOR
//        for (int i = 0; i < m_topics.Count; i++)
//        {
//            if (m_topics[i].topicName == topic)
//                m_topics[i].handler.AddListener(action);
//        }
//#endif
    }


    public void Unsubscribe(string topic, UnityAction action)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveListener(action);

//#if UNITY_EDITOR
//        for (int i = 0; i < m_topics.Count; i++)
//        {
//            if (m_topics[i].topicName == topic)
//                m_topics[i].handler.RemoveListener(action);
//        }
//#endif
    }

    public void ClearTopic(string topic)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveAllListeners();

//#if UNITY_EDITOR
//        m_topics.RemoveAll(x => x.topicName == topic);
//#endif
    }

    public UnityEvent GetTopic(string topic)
    {
        if(m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent>();
        if (!m_handlers.ContainsKey(topic))
            RegisterTopic(topic, new UnityEvent());
        return m_handlers[topic];
    }

    private void RegisterTopic(string name, UnityEvent topic)
    {
#if UNITY_EDITOR
        Topic t = new Topic(name, topic);
        if (m_topics == null)
            m_topics = new List<Topic>();
        m_topics.Add(t);
#endif
        m_handlers[name] = topic;
    }

    public void RemoveTopic(string name)
    {
        m_handlers.Remove(name);
#if UNITY_EDITOR
        m_topics.RemoveAll(x => x.topicName == name);
#endif
    }


    public virtual void InvokeTopics(string[] topics)
    {
        for (int i = 0; i < topics.Length; i++)
        {
            if (m_handlers.ContainsKey(topics[i]))
            {
                m_handlers[topics[i]].Invoke();
            }
        }      
    }

    public virtual void InvokeAllTopics()
    {
        foreach (var handler in m_handlers)
        {
            handler.Value.Invoke();
        }
    }
}

[System.Serializable]
public class Theme<T0>:ThemeBase
{
    [SerializeField] private List<Topic<T0>> m_topics;
    private Dictionary<string, UnityEvent<T0>> m_handlers;

    private void Awake()
    {
        if(m_handlers == null)
        m_handlers = new Dictionary<string, UnityEvent<T0>>();

#if UNITY_EDITOR
        if (m_topics == null)
            m_topics = new List<Topic<T0>>();
        for (int i = 0; i < m_topics.Count; i++)
        {
            if (!m_handlers.ContainsKey(m_topics[i].topicName))
                m_handlers[m_topics[i].topicName] = m_topics[i].handler;
        }
#endif
    }
    
    public void Subscribe(string topic, UnityAction<T0> action)
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0>>();
        if (!m_handlers.ContainsKey(topic))
            m_handlers[topic] = new UnityEvent<T0>();
        m_handlers[topic].AddListener(action);

//#if UNITY_EDITOR
//        for (int i = 0; i < m_topics.Count; i++)
//        {
//            if (m_topics[i].topicName == topic)
//            {
//                m_topics[i].handler.AddListener(action);
//            }
//        }
//#endif
    }


    public void Unsubscribe(string topic, UnityAction<T0> action)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveListener(action);

//#if UNITY_EDITOR
//        for (int i = 0; i < m_topics.Count; i++)
//        {
//            if (m_topics[i].topicName == topic)
//                m_topics[i].handler.RemoveListener(action);
//        }
//#endif
    }

    public void ClearTopic(string topic)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveAllListeners();

//#if UNITY_EDITOR
//        m_topics.RemoveAll(x => x.topicName == topic);
//#endif
    }

    public UnityEvent<T0> GetTopic(string topic)
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0>>();
        if (!m_handlers.ContainsKey(topic))
            RegisterTopic(topic, new UnityEvent<T0>());
        return m_handlers[topic];
    }

    private void RegisterTopic(string name, UnityEvent<T0> topic)
    {
#if UNITY_EDITOR
        Topic<T0> t = new Topic<T0>(name, topic);
        if(m_topics == null)
            m_topics = new List<Topic<T0>>();
        m_topics.Add(t);
#endif
        m_handlers[name] = topic;
    }

    public void RemoveTopic(string name)
    {
        m_handlers.Remove(name);
#if UNITY_EDITOR
        m_topics.RemoveAll(x => x.topicName == name);
#endif
    }

    public virtual void InvokeAll(Dictionary<string, T0> args)
    {
        foreach (var handler in args)
        {
            if(m_handlers.ContainsKey(handler.Key))
            {
                m_handlers[handler.Key].Invoke(args[handler.Key]);
            }
        }
    }

    public virtual void InvokeAllTopics(T0 arg)
    {
        foreach (var handler in m_handlers)
        {
            handler.Value.Invoke(arg);
        }
    }
}


[System.Serializable]
public class Theme<T0, T1> : ThemeBase
{
    [SerializeField] private List<Topic<T0,T1>> m_topics;
    private Dictionary<string, UnityEvent<T0,T1>> m_handlers;

    private void Awake()
    {
        if(m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0,T1>>();
        for (int i = 0; i < m_topics.Count; i++)
        {
            if (!m_handlers.ContainsKey(m_topics[i].topicName))
                m_handlers[m_topics[i].topicName] = m_topics[i].handler;
        }
    }

    public void Subscribe(string topic, UnityAction<T0,T1> action)
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0, T1>>();
        if (!m_handlers.ContainsKey(topic))
            m_handlers[topic] = new UnityEvent<T0, T1>();
        m_handlers[topic].AddListener(action);
//#if UNITY_EDITOR
//        for (int i = 0; i < m_topics.Count; i++)
//        {
//            if (m_topics[i].topicName == topic)
//            {
//                m_topics[i].handler.AddListener(action);
//            }
//        }
//#endif
    }


    public void Unsubscribe(string topic, UnityAction<T0,T1> action)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveListener(action);
    }

    public void ClearTopic(string topic)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveAllListeners();
    }

    public UnityEvent<T0, T1> GetTopic(string topic)
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0,T1>>();
        if (!m_handlers.ContainsKey(topic))
            RegisterTopic(topic, new UnityEvent<T0,T1>());
        return m_handlers[topic];
    }

    public void RegisterTopic(string name, UnityEvent<T0,T1> topic)
    {
#if UNITY_EDITOR
        Topic<T0,T1> t = new Topic<T0,T1>(name, topic);
        if (m_topics == null)
            m_topics = new List<Topic<T0,T1>>();
        m_topics.Add(t);
#endif
        m_handlers[name] = topic;
    }

    public virtual void InvokeAllTopics(T0 arg0, T1 arg1)
    {
        foreach (var handler in m_handlers)
        {
            handler.Value.Invoke(arg0, arg1);
        }
    }
}

[System.Serializable]
public class Theme<T0, T1, T2> : ThemeBase
{
    [SerializeField] private List<Topic<T0, T1, T2>> m_topics;
    private Dictionary<string, UnityEvent<T0, T1, T2>> m_handlers;

    private void Awake()
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0, T1, T2>>();
        for (int i = 0; i < m_topics.Count; i++)
        {
            if (!m_handlers.ContainsKey(m_topics[i].topicName))
                m_handlers[m_topics[i].topicName] = m_topics[i].handler;
        }
    }

    public void Subscribe(string topic, UnityAction<T0, T1, T2> action)
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0, T1, T2>>();
        if (!m_handlers.ContainsKey(topic))
            m_handlers[topic] = new UnityEvent<T0, T1, T2>();
        m_handlers[topic].AddListener(action);
//#if UNITY_EDITOR
//        for (int i = 0; i < m_topics.Count; i++)
//        {
//            if (m_topics[i].topicName == topic)
//            {
//                m_topics[i].handler.AddListener(action);
//            }
//        }
//#endif
    }

    public void Unsubscribe(string topic, UnityAction<T0, T1, T2> action)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveListener(action);
    }

    public void ClearTopic(string topic)
    {
        if (m_handlers.ContainsKey(topic))
            m_handlers[topic].RemoveAllListeners();
    }

    public UnityEvent<T0, T1, T2> GetTopic(string topic)
    {
        if (m_handlers == null)
            m_handlers = new Dictionary<string, UnityEvent<T0, T1, T2>>();
        if (!m_handlers.ContainsKey(topic))
            RegisterTopic(topic, new UnityEvent<T0, T1, T2>());
        return m_handlers[topic];
    }

    public void RegisterTopic(string name, UnityEvent<T0, T1, T2> topic)
    {
#if UNITY_EDITOR
        Topic<T0, T1, T2> t = new Topic<T0, T1, T2>(name, topic);
        if (m_topics == null)
            m_topics = new List<Topic<T0, T1, T2>>();
        m_topics.Add(t);
#endif
        m_handlers[name] = topic;
    }


    public virtual void InvokeAllTopics(T0 arg0, T1 arg1, T2 arg2)
    {
        foreach (var handler in m_handlers)
        {
            handler.Value.Invoke(arg0, arg1, arg2);
        }
    }
}