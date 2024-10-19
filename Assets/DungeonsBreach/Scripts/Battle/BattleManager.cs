using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using Unity.VisualScripting;


public class BattleManager : Singleton<BattleManager>
{
    [SerializeField] private bool m_testMode;
    [SerializeField] private string m_monsterTag;
    [SerializeField] private string m_playerTag;

    private DefaultInputActions m_inputActions;
    private IsoGridCoord m_pointerGridCoord;

    private UnitBase m_selectedUnit;
    private UnityEvent<IsoGridCoord> m_onPointerCoordChange;

    private IsoGridCoord[] m_actionRange;
    private IsoGridCoord[] m_moveRange;

    bool m_unitPathFound;


    public static UnitBase SelectedUnit
    {
        get { return GetSingleton().m_selectedUnit; }
        private set
        {
            if (GetSingleton().m_selectedUnit != value)
            {
                //deactive module
                if (GetSingleton().m_selectedUnit != null)
                {
                    if (GetSingleton().m_selectedUnit.ActivedModule(out var module))
                    {
                        GetSingleton().StopActionPreview(module);
                        module.Actived = false;
                    }
                }

                //show move range
                BattleUIController.DisposeMoveHighlights();
                if (value != null && value.MovesAvalaible > 0)
                {
                    GetSingleton().m_moveRange = value.Agent.ReachableCoordinates(value.MovesAvalaible, GridManager.ActivePathGrid);
                    BattleUIController.HighlightPathRange(GetSingleton().m_moveRange, "MoveRange");
                }
                else if (value == null)
                {
                    GetSingleton().m_selectedUnit.Agent.StopMovePreview();
                    ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).UpdateActionPreview();
                    BattleUIController.CursorController.ResetCursor();
                    BattleUIController.DisposeActionHighlights();
                    BattleUIController.HidePathTrail();
                }
                GetSingleton().m_selectedUnit = value;
                EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("SelectedUnitChange").Invoke(value);
            }
        }
    }

    #region monobehaviour

    protected override void Awake()
    {
        base.Awake();
        m_inputActions = new DefaultInputActions();
        m_onPointerCoordChange = new UnityEvent<IsoGridCoord>();

        m_onPointerCoordChange.AddListener(OnPointerCoordChange);
    }

    private IEnumerator Start()
    {
        m_inputActions.UI.Enable();
        m_inputActions.UI.Point.performed += OnPointerPoint;
        m_inputActions.UI.Click.performed += OnClick;
        m_inputActions.UI.RightClick.performed += OnRightClick;
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(ResetUnits());
    }

    #endregion


    #region  public methods
    public static IEnumerator ResetUnits()
    {
        foreach (var unit in LevelManager.Units)
        {
            var status = UnitStatus.Empty;
            unit.UpdateStatus(status);
            unit.ResetActions();
        }
        yield return null;
    }


    public static IEnumerator StartNextTurn()
    {
        SelectedUnit = null;
        yield return ActionTurn.CreateOrGetActionTurn(ActionTurnType.PlayerTurn).ExcuteEndTurnDeles();
        yield return ActionTurn.StartActionTurns(ActionTurnType.EnvironmentAction, ActionTurnType.EnemySpawn);
        yield return ActionTurn.StartActionTurns(ActionTurnType.EnemyMoveAndAction, ActionTurnType.EnemySpawnPreview);
        yield return ActionTurn.CreateOrGetActionTurn(ActionTurnType.PlayerTurn).ExcuteStartTurnDeles();
    }


    public static void UpdateActionPreview(ActionModule action_module)
    {
        GetSingleton().UpdateModuleActionPreview(action_module);
    }

    public static void TriggerModuleActionPreview(ActionModule action_module)
    {
        GetSingleton().TriggerActionPreview(action_module);
    }

    #endregion

    #region Events

    private void OnPointerCoordChange(IsoGridCoord coord)
    {
        if (SelectedUnit == null)
            return;

        m_unitPathFound = false;
        bool moduleActived = SelectedUnit.ActivedModule(out var module);
        SelectedUnit.Agent.StopMovePreview();

        //move range preview
        if (SelectedUnit.MovesAvalaible > 0 && !moduleActived && !SelectedUnit.Agent.IsMoving)
        {
            List<IsoGridCoord> range = new List<IsoGridCoord>(m_moveRange);
            if (range.Contains(coord))
            {
                var agent = SelectedUnit.Agent;
                m_unitPathFound = IsoGridPathFinding.FindPathAstar(agent.Coordinate, coord, GridManager.ActivePathGrid, agent.BlockingMask, out var path);
                if (m_unitPathFound)
                {
                    BattleUIController.ShowPathTrail(path);
                    SelectedUnit.Agent.StartMovePreview(coord);
                    ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).UpdateActionPreview();
                    if(SelectedUnit.CompareTag("PlayerUnit"))
                        ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).CheckPreview();
                    BattleUIController.CursorController.SetCursor("MoveAvailable");
                }
                else
                {
                    ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).UpdateActionPreview();
                    BattleUIController.CursorController.SetCursor("MoveUnavailable");
                    BattleUIController.HidePathTrail();
                }
            }
            else
            {
                BattleUIController.HidePathTrail();
                BattleUIController.CursorController.ResetCursor();
            }
        }


        //action module preview
        else if (moduleActived && module.IsAvailable)
        {
            BattleUIController.DisposeMoveHighlights();
            StopActionPreview(module);

            SelectedUnit.SetDirection(SelectedUnit.Agent.Coordinate.DirectionTo(m_pointerGridCoord, GridManager.ActivePathGrid));
            var param = new ActionModuleParam(SelectedUnit, new IsoGridCoord[] { m_pointerGridCoord }, false);
            module.GeneratePreview(param);
            m_actionRange = module.ActionRange();
            BattleUIController.HighlightActionRange(m_actionRange, "ActionRange");
            IsoGridCoord[] confirmed = module.ConfirmActionTargets();
            if (confirmed.Length > 0)
            {
                BattleUIController.ActionPreviewer.InitPreview();
                module.GeneratePreview(param);
                BattleUIController.CursorController.SetCursor("TargetValid");
                module.PreviewKey = PreviewKey.GlobalKey;
                StartCoroutine(module.StartPreview());
            }
            else
            {
                BattleUIController.CursorController.SetCursor("TargetInvalid");
            }
        }
    }

    #endregion

    #region Helpers

    private void TriggerActionPreview(ActionModule action_module)
    {
        action_module.StopDamagePreview();
        IsoGridCoord[] confirmed = action_module.ConfirmActionTargets();
        if (confirmed.Length > 0)
        {
            BattleUIController.ActionPreviewer.InitPreview();
            action_module.PreviewKey = PreviewKey.GlobalKey;
            StartCoroutine(action_module.StartPreview());
        }
    }

    private void UpdateModuleActionPreview(ActionModule action_module)
    {
        var confirmed = action_module.ConfirmActionTargets();
        if (action_module.ActionParam.unit.CompareTag("MonsterUnit"))
        {
            if (confirmed.Length > 0)
            {
                StopActionPreview(action_module);
                BattleUIController.ActionPreviewer.InitPreview();
                action_module.PreviewKey = new PreviewKey(action_module);
                StartCoroutine(action_module.StartPreview());
                action_module.StopDamagePreview();

                BattleUIController.DisposeActionHighlights();
            }
        }
    }


    private void ModuleActionHandler(ActionModule action_module, UnitBase action_unit, IsoGridCoord[] input_coords)
    {
        Debug.Log("Module Action: " + action_module.ModuleName);
        var param = new ActionModuleParam(action_unit, input_coords, false);
        action_module.Build(param);
        var confirmed = action_module.ConfirmActionTargets();
        action_module.Actived = false;
        action_module.IsAvailable = false;
        if (action_unit.CompareTag("PlayerUnit"))
        {
            if (confirmed.Length > 0)
            {
                action_module.GeneratePreview(param);
                StartCoroutine(action_module.ExcuteAction());
                StopActionPreview(action_module);
                BattleUIController.DisposeActionHighlights();
            }
        }
        else if (action_unit.CompareTag("MonsterUnit"))
        {
            if (confirmed.Length > 0)
            {
                ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).RegistorAction(action_module);
                StopActionPreview(action_module);

                BattleUIController.ActionPreviewer.InitPreview();
                action_module.GeneratePreview(param);
                StartCoroutine(action_module.StartPreview());
                action_module.StopDamagePreview();

                BattleUIController.DisposeActionHighlights();
            }
        }
    }



    private void StopActionPreview(ActionModule actionModule)
    {
        actionModule.StopDamagePreview();
        BattleUIController.ActionPreviewer.ClearPreview(PreviewKey.GlobalKey);
        BattleUIController.CursorController.ResetCursor();
    }

    #endregion


    #region Input

    private void OnPointerPoint(InputAction.CallbackContext obj)
    {
        var sPos = obj.ReadValue<Vector2>();
        var wPos = Camera.main.ScreenToWorldPoint(new Vector3(sPos.x, sPos.y, 0));
        var grid = GridManager.ActiveTileGrid;
        var coord = wPos.ToIsoCoordinate(grid);

        bool checkRange = grid.CheckRange(coord);

        if (m_pointerGridCoord != coord)
        {
            if (checkRange)
            {
                BattleUIController.ShowPointerHighlight(coord);
            }
            else
            {
                BattleUIController.HidePointerHighlight();
                BattleUIController.HidePathTrail();
                //BattleUIController.DisposeMoveHighlights();
            }
            m_pointerGridCoord = coord;
            m_onPointerCoordChange.Invoke(coord);
        }
    }

    private void OnClick(InputAction.CallbackContext obj)
    {
        var grid = GridManager.ActiveTileGrid;
        if (!grid.CheckRange(m_pointerGridCoord))
            return;

        SelectedUnit?.Agent.StopMovePreview();
        ActionModule activedModule = null;
        bool moduleActived = SelectedUnit == null ? false : SelectedUnit.ActivedModule(out activedModule);
        bool selectCondition = LevelManager.TryGetUnits(m_pointerGridCoord, out var units) && (!moduleActived);

        if (selectCondition)
        {
            SelectedUnit = units[0];
        }
        else
        {
            if (!moduleActived && m_unitPathFound)
            {
                BattleUIController.DisposeMoveHighlights();
                var action = SelectedUnit.Move(LocamotionType.Default, m_pointerGridCoord,false);
                StartCoroutine(action.ExcuteAction());
                BattleUIController.StartPathTrailing(SelectedUnit);
                BattleUIController.CursorController.ResetCursor();
            }
            else if (moduleActived && activedModule.IsAvailable)
            {
                ModuleActionHandler(activedModule, SelectedUnit, new IsoGridCoord[] { m_pointerGridCoord });
                BattleUIController.CursorController.ResetCursor();
            }
        }
    }

    // private IsoGridCoord[] ConfirmActionRange(IsoGridCoord[] action_range, IsoGridCoord input)
    // {
    //     List<IsoGridCoord> range = new List<IsoGridCoord>();
    //     foreach (var coord in action_range)
    //     {
    //         if (coord == input)
    //             range.Add(coord);
    //     }
    //     return range.ToArray();
    // }


    private void OnRightClick(InputAction.CallbackContext obj)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit = null;
        }
    }

    #endregion


    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 70, 100, 30), "Update"))
        {
            var turn = ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack);
            turn.UpdateActionPreview();
        }
    }
}
