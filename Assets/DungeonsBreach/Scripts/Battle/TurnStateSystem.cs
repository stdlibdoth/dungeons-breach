using UnityEngine;
using System.Collections.Generic;

public class TurnStateSystem : Singleton<TurnStateSystem>
{
    private static HashSet<ITurnStateElement> m_elements;
    private static Dictionary<int,TurnState> m_states;
    private static List<int> m_sequence;
    private static int m_index;


    private static int m_stateCount;

    public static void RegistorElement(ITurnStateElement element)
    {
        m_elements.Add(element);
    }

    public TurnState Step(List<IAction> actions)
    {
        TurnState state = new TurnState(m_stateCount,actions);
        m_states[m_stateCount] = state;
        foreach (var item in m_elements)
        {
            item.RecordValue(m_stateCount);
        }
        if(m_index == m_sequence.Count)
        {
            m_sequence.Add(m_stateCount);
        }
        else
        {
            m_sequence[m_index] = m_stateCount;
        }
        m_stateCount++;
        m_index++;
        return state;
    }

    public (bool,TurnState) PlayBack(int key)
    {
        if(!m_states.ContainsKey(key))
            return (false,null);

        m_index = m_sequence.IndexOf(key);
        foreach (var item in m_elements)
        {
            item.SetValueByKey(key);
        }
        return (true, m_states[key]);
    }

    public TurnState StepBack(int step_count = 1)
    {
        m_index = Mathf.Clamp(m_index-step_count,0,m_sequence.Count -1);
        var key = m_sequence[m_index];
        foreach (var item in m_elements)
        {
            item.SetValueByKey(key);
        }
        return m_states[key];
    }
}
