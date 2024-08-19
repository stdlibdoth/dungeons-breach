using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class Topic
{
    public string topicName;
    public UnityEvent handler;


    public Topic(string name, UnityEvent handler)
    {
        this.topicName = name;
        this.handler = handler;
    }
}

[System.Serializable]
public class Topic<T0>
{
    public string topicName;
    public UnityEvent<T0> handler;


    public Topic(string name, UnityEvent<T0> handler)
    {
        this.topicName = name;
        this.handler = handler;
    }
}



[System.Serializable]
public class Topic<T0,T1>
{
    public string topicName;
    public UnityEvent<T0,T1> handler;


    public Topic(string name, UnityEvent<T0,T1> handler)
    {
        this.topicName = name;
        this.handler = handler;
    }
}


[System.Serializable]
public class Topic<T0,T1,T2>
{
    public string topicName;
    public UnityEvent<T0, T1, T2> handler;


    public Topic(string name, UnityEvent<T0, T1, T2> handler)
    {
        this.topicName = name;
        this.handler = handler;
    }
}
