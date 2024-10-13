using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public partial class ActionTurn
{
    [SerializeField] protected ActionTurnType m_type;
    public ActionTurnType Type { get { return m_type; } }

    protected List<IAction> m_actions;

    private List<ActionTurnDelegate> m_turnStartDeles = new List<ActionTurnDelegate>();
    private List<ActionTurnDelegate> m_turnEndDeles = new List<ActionTurnDelegate>();
   
    private bool m_isExecuting = false;

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
        if (m_actions.Contains(action) && action is ActionModule actionModule)
        {
            BattleUIController.ActionPreviewer.ClearPreview(actionModule.PreviewKey);
        }
    }

    public void UpdateActionPreview()
    {
        if (m_isExecuting)
            return;

        for (int i = 0; i < m_actions.Count; i++)
        {
            if (m_actions[i] is ActionModule actionModule)
            {
                BattleUIController.ActionPreviewer.ClearPreview(actionModule.PreviewKey);
                BattleManager.UpdateActionPreview(actionModule);
            }
        }
    }

    public void CheckPreview()
    {
        if (m_isExecuting)
            return;

        for (int i = 0; i < m_actions.Count; i++)
        {
            if (m_actions[i] is ActionModule actionModule)
            {
                var confirmedCoords = actionModule.ConfirmActionTargets();
                if(confirmedCoords.Length>0)
                {
                    BattleUIController.ActionPreviewer.ClearPreview(actionModule.PreviewKey);
                    BattleManager.TriggerModuleActionPreview(actionModule);
                }
            }
        }
    }

    protected virtual IEnumerator ExcuteActionTurn()
    {
        m_isExecuting = true;
        Debug.Log(m_type + "  Turn Start-----------------------------");
        foreach (var item in m_turnStartDeles)
        {
            yield return item.Invoke(this);
        }

        m_actions.Sort((a, b) => new ActionComparer().Compare(a, b));
        for (int i = 0; i < m_actions.Count; i++)
        {
            Debug.Log(m_actions[i] + "  begin-----------------");
            yield return m_actions[i].ExcuteAction();
        }

        foreach (var item in m_turnEndDeles)
        {
            yield return item.Invoke(this);
        }
        m_turnStartDeles = new List<ActionTurnDelegate>();
        m_turnEndDeles = new List<ActionTurnDelegate>();
        m_actions = new List<IAction>();
        m_isExecuting = false;
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


public enum ActionTurnType : int
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