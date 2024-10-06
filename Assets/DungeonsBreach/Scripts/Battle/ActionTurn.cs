using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public partial class ActionTurn
{
    [SerializeField] protected ActionTurnType m_type;
    public ActionTurnType Type {get{return m_type;}}

    protected List<IAction> m_actions;

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

    public void CancelAction(IAction action)
    {
        m_actions.Remove(action);
    }

    public void UpdateActionTurn()
    {
        var actions = new IAction[m_actions.Count];
        for (int i = 0; i < m_actions.Count; i++)
        {
            actions[i] = m_actions[i];
        }

        m_actions.Clear();

        for (int i = 0; i < actions.Length; i++)
        {
            if(actions[i] is ActionModule actionModule)
            {
                BattleUIController.ActionPreviewer.ClearPreview(actionModule.PreviewKey);
                BattleManager.UpdateModuleAction(actionModule,actionModule.ActionParam.unit,IsoGridCoord.Zero);
            }           
        }
    }

    protected virtual IEnumerator ExcuteActionTurn()
    {
        foreach (var item in m_turnStartDeles)
        {
            yield return item.Invoke(this);
        }

        m_actions.Sort((a,b)=>new ActionComparer().Compare(a,b));
        for (int i = 0; i < m_actions.Count; i++)
        {
            Debug.Log("turn start---------------------------------");
            yield return m_actions[i].ExcuteAction();
        }

        foreach (var item in m_turnEndDeles)
        {
            yield return item.Invoke(this);
        }
        m_turnStartDeles = new List<ActionTurnDelegate>();
        m_turnEndDeles = new List<ActionTurnDelegate>();
        m_actions = new List<IAction>();
    }
    
    protected ActionTurn(ActionTurnType type)
    {
        m_type = type;
        m_actions = new List<IAction>();
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