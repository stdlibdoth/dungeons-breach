using UnityEngine;
using System;
using System.Collections.Generic;


public class TurnState
{
    public readonly int id;
    private IReadOnlyList<IAction> m_actions;

    public TurnState(int id, IReadOnlyList<IAction> actions)
    {
        this.id = id;
        m_actions = actions;
    }

    public IReadOnlyList<IAction> Actions
    {
        get { return m_actions; }
    }
}


public interface ITurnStateElement
{
    public abstract void RecordValue(int key);
    public abstract object SetValueByKey(int key);
    public abstract object Default { get; }
    public abstract object Value { get; set; }
}


public class TurnStateElement<T> : ITurnStateElement
{
    protected Dictionary<int, T> m_values;


    protected bool m_isDirty;
    protected T m_tempValue;

    public object Default
    {
        get
        {
            if(m_values.ContainsKey(-1))
            {
                return m_values[-1];
            }
            return default(T);
        }
    }
    public object Value
    {
        get
        {
            return m_tempValue;
        }
        set
        {
            m_tempValue = (T)value;
        }
    }

    public TurnStateElement(T value)
    {
        m_values = new Dictionary<int, T>();
        m_values[-1] = value;
    }

    public void RecordValue(int key)
    {
        if (!(m_tempValue is T))
            return;

        if (m_isDirty)
        {
            m_values[key] = (T)m_tempValue;
        }
    }

    public object SetValueByKey(int key)
    {
        if (m_values.ContainsKey(key))
            return m_values[key];
        return m_tempValue;
    }

    public void SetDirty(bool dirty)
    {
        m_isDirty = dirty;
    }
}