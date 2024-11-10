using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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

    public static async UniTask StartActionTurns(ActionTurnType start_type,ActionTurnType end_type)
    {
        var turns = SortTurns(start_type, end_type);
        if (turns.Length == 0)
            return;

        var min = turns[0].Type;
        var max = turns[turns.Length -1].Type;
        if (start_type > max || end_type < min)
            return;

        var startType = start_type < min?min :start_type;
        var endType = end_type < max?end_type :max;
        var startIndex = GetIndex(startType,turns);
        var endIndex = GetIndex(endType,turns);

        for (int i = startIndex; i <= endIndex; i++)
        {
            if (turns[i].IsActive)
            {
                await turns[i].ExcuteActionTurn();
            }
        }
    }

    private static ActionTurn[] SortTurns(ActionTurnType start_type, ActionTurnType end_type)
    {
        List<ActionTurn> turns = new List<ActionTurn>(m_actionTurns.Values);
        turns.RemoveAll((turn) => turn.Type < start_type || turn.Type > end_type);
        turns.Sort((a,b)=>new ActionTurnComparer().Compare(a,b));
        return turns.ToArray();
    }


    public static void RegistorTempAction(IAction action)
    {
        m_tempActions.Enqueue(action);
    }


    public static async UniTask StartNextTurn()
    {
        EventManager.GetTheme<BattleRoundTheme>("BattleRoundTheme").GetTopic("RoundStart").Invoke(0);
        await CreateOrGetActionTurn(ActionTurnType.PlayerTurn).ExcuteEndTurnDeles();
        await StartActionTurns(ActionTurnType.EnvironmentAction, ActionTurnType.EnemySpawn);
        await UniTask.WaitForSeconds(1f);
        await AICalculation();
        await StartActionTurns(ActionTurnType.EnemyMove, ActionTurnType.EnemySpawnPreview);
        await CreateOrGetActionTurn(ActionTurnType.PlayerTurn).ExcuteStartTurnDeles();
    }

    public static async UniTask ExcuteTempActions()
    {
        while (m_tempActions.TryDequeue(out var action))
        {
            await action.ExcuteAction();
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


    private static async UniTask AICalculation()
    {
        foreach (var unit in LevelManager.Units)
        {
            UnitAIAgent aiAgent = unit.GetComponent<UnitAIAgent>();
            if (aiAgent!= null)
            {
                var aiAction = aiAgent.Build(new AIAgentActionParam());
                CreateOrGetActionTurn(ActionTurnType.EnemyMove).RegistorAction(aiAction);
            }
        }
        await UniTask.Yield();
    }
}
