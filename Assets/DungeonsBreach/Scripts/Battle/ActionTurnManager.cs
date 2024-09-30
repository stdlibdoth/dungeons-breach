using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ActionTurn
{
    private static Dictionary<ActionTurnType,ActionTurn> m_actionTurns = new Dictionary<ActionTurnType, ActionTurn>();
    private static Queue<IAction> m_tempActions = new Queue<IAction>();

    public static ActionTurn CreateOrGetActionTurn(ActionTurnType type)
    {
        if(!m_actionTurns.ContainsKey(type))
            m_actionTurns[type] = new ActionTurn(type);
        return m_actionTurns[type];
    }

    public static void ClearAll()
    {      
        m_actionTurns = new Dictionary<ActionTurnType, ActionTurn>();
    }

    public static IEnumerator StartActionTurns(ActionTurnType start_type,ActionTurnType end_type)
    {
        var turns = SortTurns();
        if(turns.Length == 0)
            yield break;

        var min = turns[0].Type;
        var max = turns[turns.Length -1].Type;
        if(start_type>max || end_type<min)
            yield break;

        var startType = start_type < min?min :start_type;
        var endType = end_type < max?end_type :max;
        var startIndex = GetIndex(startType,turns);
        var endIndex = GetIndex(endType,turns);

        for (int i = startIndex; i <= endIndex; i++)
        {
            yield return turns[i].ExcuteActionTurn();
        }
    }

    private static ActionTurn[] SortTurns()
    {
        List<ActionTurn> turns = new List<ActionTurn>(m_actionTurns.Values);
        turns.Sort((a,b)=>new ActionTurnComparer().Compare(a,b));
        return turns.ToArray();
    }


    public static void RegistorTempAction(IAction action)
    {
        m_tempActions.Enqueue(action);
    }

    public static IEnumerator ExcuteTempActions()
    {
        while (m_tempActions.TryDequeue(out var action))
        {
            yield return action.ExcuteAction();
        }
    }


    private static int GetIndex(ActionTurnType type, ActionTurn[] turns)
    {
        for (int i = 0; i < turns.Length; i++)
        {
            if(turns[i].Type == type)
                return i;
        }
        return -1;
    }
}
