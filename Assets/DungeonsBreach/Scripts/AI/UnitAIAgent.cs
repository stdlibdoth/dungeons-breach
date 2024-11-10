using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Cysharp.Threading.Tasks;

public class UnitAIAgent : MonoBehaviour,IAction
{
    public class ActionData
    {
        public IsoGridCoord unitCoord;
        public IsoGridDirection unitDirection;
        public ActionModule actionModule;
        public IsoGridCoord actionTarget;
        public ActionTileInfo[] actionTileInfo;
    }


    [SerializeField] private UnitBase m_unit;
    [SerializeField] private ActionPriority m_priority;

    private Dictionary<IsoGridCoord,ActionData> m_actionData;
    private Random m_rng;

    public ActionPriority Priority
    {
        get { return m_priority; }
        set { m_priority = value; }
    }


    private void Start()
    {
        m_rng = new Random((uint)System.DateTime.Now.Ticks);
    }


    private void RegistorAction(ActionModule action_module, IsoGridCoord[] action_targets)
    {
        Debug.Log("Module Action: " + action_module.ModuleName);
        var param = new ActionModuleParam(m_unit, action_targets, false);
        action_module.Build(param);
        var confirmed = action_module.ConfirmActionTargets();
        action_module.Actived = false;
        action_module.IsAvailable = false;
        if (confirmed.Length > 0)
        {
            ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).RegistorAction(action_module);

            //stop last preview
            action_module.StopPreview();
            BattleUIController.ActionPreviewer.ClearPreview(PreviewKey.GlobalKey);


            BattleUIController.ActionPreviewer.InitPreview();
            action_module.GeneratePreview(param);
            action_module.StartPreview();
            action_module.StopPreview();

            BattleUIController.DisposeActionHighlights();
        }
    }


    public async UniTask ExcuteAction()
    {
        var units = LevelManager.UnitCoordinates;
        int maxScore = int.MinValue;
        ActionData selected= null;
        foreach (var data in m_actionData)
        {
            int score = 0;
            foreach (var info in data.Value.actionTileInfo)
            {
                if (units.ContainsKey(info.relativeCoord))
                {
                    var unit = units[info.relativeCoord];
                    if (unit.CompareTag("PlayerUnit"))
                        score += (info.value * 3);
                    else if (unit.CompareTag("HealthUnit"))
                        score += (info.value * 4);
                    else if (unit.CompareTag("ObstacleUnit"))
                        score += info.value;
                    else if (unit.CompareTag("MonsterUnit"))
                        score -= info.value * 2;
                }
            }
            if(score > maxScore)
            {
                maxScore = score;
                selected = data.Value;
            }
        }

        //if the max score is negative or zero, action will not be excuted.
        //Try to move close to player units, random choose target

        if(maxScore<=0 || selected == null)
        {
            List<UnitBase> targets = new List<UnitBase>();
            foreach (var item in units)
            {
                if(item.Value.CompareTag("PlayerUnit"))
                    targets.Add(item.Value);
            }
            var targetIndex = m_rng.NextInt(0, targets.Count);
            var t = targets[targetIndex];

            float minDist = float.MaxValue;
            IsoGridCoord dest = m_unit.PathAgent.Coordinate;
            foreach (var actionData in m_actionData.Values)
            {
                var dist = IsoGridPathFinding.ManhattanDistance(actionData.unitCoord, t.PathAgent.Coordinate);
                if (dist<minDist)
                {
                    minDist = dist;
                    dest = actionData.unitCoord;
                }
            }
            var moveAction = m_unit.Move(LocamotionType.Default, dest, false, true);
            await moveAction.ExcuteAction();
        }

        //if max score is positive, unit will move to the location and action will be registored.
        else
        {
            await UnitAction(selected);
        }
    }

    private async UniTask UnitAction(ActionData action_data)
    {
        var moveAction = m_unit.Move(LocamotionType.Default, action_data.unitCoord, false, true);
        await moveAction.ExcuteAction();
        m_unit.SetDirection(action_data.unitDirection);
        RegistorAction(action_data.actionModule, new IsoGridCoord[] { action_data.actionTarget });
        await UniTask.Yield();
    }



    private void CollectActionData(ActionModule action_module)
    {
        var moveRange = m_unit.PathAgent.ReachableCoordinates(m_unit.MovesAvalaible, GridManager.ActivePathGrid);
        foreach (var location in moveRange)
        {
            m_unit.PathAgent.StartMovePreview(location,false);
            var actionRange = action_module.ActionRange(location);
            foreach (var coord in actionRange)
            {
                if (!m_actionData.ContainsKey(coord))
                {
                    GetActionData(location, action_module, coord);
                }
            }
            m_unit.PathAgent.StopMovePreview();
        }
    }


    private void GetActionData(IsoGridCoord module_position, ActionModule action_module, IsoGridCoord action_target)
    {
        var originalDir = m_unit.PathAgent.Direction;
        var dir = module_position.DirectionTo(action_target, GridManager.ActivePathGrid);
        m_unit.PathAgent.Direction = dir;
        var param = new ActionModuleParam(m_unit, new IsoGridCoord[] { action_target }, false);
        action_module.GeneratePreview(param);
        var targets = action_module.ConfirmActionTargets();
        action_module.PreviewKey = PreviewKey.GlobalKey;
        var actionTileInfo = action_module.StartPreview();
        //Debug.Log(targets[0] + "   " + actionTileInfo.Length);
        action_module.StopPreview();
        BattleUIController.ActionPreviewer.ClearPreview(PreviewKey.GlobalKey);
        var actionData = new ActionData
        {
            actionModule = action_module,
            actionTarget = action_target,
            actionTileInfo = actionTileInfo,
            unitDirection = dir,
            unitCoord = module_position,
        };
        m_actionData.Add(action_target,actionData);
        m_unit.PathAgent.Direction = originalDir;
    }

    public IAction Build<T>(T param) where T : IActionParam
    {
        BattleUIController.ActionPreviewer.InitPreview();
        m_actionData = new Dictionary<IsoGridCoord, ActionData>();
        var modules = m_unit.ActionModules();
        if (modules.Length > 0)
        {
            CollectActionData(modules[0]);
        }
        return this;
    }
}

public class AIAgentActionParam: IActionParam
{

}
