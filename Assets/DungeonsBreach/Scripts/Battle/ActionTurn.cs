using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[System.Serializable]
public partial class ActionTurn
{
    [SerializeField] protected ActionTurnType m_type;
    public ActionTurnType Type {get{return m_type;}}

    protected HashSet<IAction> m_actions;

    private List<ActionTurnDelegate> m_turnStartDeles = new List<ActionTurnDelegate>();
    private List<ActionTurnDelegate> m_turnEndDeles = new List<ActionTurnDelegate>();

    public void SubOnTurnStart(ActionTurnDelegate dele)
    {
        m_turnStartDeles.Add(dele);
    }

    public void SubOnTurnEnd(ActionTurnDelegate dele)
    {
        m_turnEndDeles.Add(dele);
    }

    public void RegistorAction(IAction action)
    {
        m_actions.Add(action);
    }

    protected virtual IEnumerator ExcuteActionTurn()
    {
        foreach (var item in m_turnStartDeles)
        {
            yield return item.Invoke(this);
        }

        var actions = SortActions();
        for (int i = 0; i < actions.Count; i++)
        {
            yield return actions[i].ExcuteAction();
        }

        foreach (var item in m_turnEndDeles)
        {
            yield return item.Invoke(this);
        }
        m_turnStartDeles = new List<ActionTurnDelegate>();
        m_turnEndDeles = new List<ActionTurnDelegate>();
        m_actions = new HashSet<IAction>();
    }
    
    protected ActionTurn(ActionTurnType type)
    {
        m_type = type;
        m_actions = new HashSet<IAction>();
    }
    private List<IAction> SortActions()
    {
        var result = new List<IAction>(m_actions);
        result.Sort((a,b)=>new ActionComparer().Compare(a,b));
        return result;
    }

}


public class ActionTurnComparer : Comparer<ActionTurn>
{
    public override int Compare(ActionTurn x, ActionTurn y)
    {
        return x.Type - y.Type;
    }
}


public delegate IEnumerator ActionTurnDelegate(ActionTurn actionTurn);


public enum ActionTurnType:int
{
    EnemyMoveAndActionPreview,
    EnemySpawnPreview,
    Player,
    Environment,
    EndOfTurn1,
    EndOfTurn2,
    EndOfTurn3,
    EnemyAction,
    EnemySpawn,
}