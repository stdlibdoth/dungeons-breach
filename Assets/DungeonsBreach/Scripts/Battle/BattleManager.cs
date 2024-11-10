using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using Cysharp.Threading.Tasks;


public class BattleManager : Singleton<BattleManager>
{
    [SerializeField] private BattleRoundTheme m_roundTheme;
    [SerializeField] private ActionModuleTheme m_actionModuleTheme;

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
                    GetSingleton().m_moveRange = value.PathAgent.ReachableCoordinates(value.MovesAvalaible, GridManager.ActivePathGrid);
                    BattleUIController.HighlightPathRange(GetSingleton().m_moveRange, "MoveRange");
                }
                else if (value == null)
                {
                    var previewKey = new PreviewKey(GetSingleton().m_selectedUnit.PathAgent);
                    MoveAction.StopPreview(previewKey);

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
        m_roundTheme.GetTopic("RoundStart").AddListener(OnRoundStart);
    }

    private async void Start()
    {
        await AsyncStart();
    }


    private async UniTask AsyncStart()
    {
        m_inputActions.UI.Enable();
        m_inputActions.UI.Point.performed += OnPointerPoint;
        m_inputActions.UI.Click.performed += OnClick;
        m_inputActions.UI.RightClick.performed += OnRightClick;
        await UniTask.WaitForEndOfFrame(this);
        await ResetUnits();
    }


    #endregion


    #region  public methods

    public static void Deselect()
    {
        SelectedUnit = null;
    }

    public static async UniTask ResetUnits()
    {
        foreach (var unit in LevelManager.Units)
        {
            var status = UnitStatus.Empty;
            unit.UpdateStatus(status);
            unit.ResetActions();
        }
        await UniTask.Yield();
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

    private void OnRoundStart(int round)
    {
        SelectedUnit = null;
    }

    private void OnPointerCoordChange(IsoGridCoord coord)
    {
        if (SelectedUnit == null)
            return;

        if (!SelectedUnit.CompareTag("PlayerUnit"))
            return;

        m_unitPathFound = false;
        bool moduleActived = SelectedUnit.ActivedModule(out var module);

        var previewKey = new PreviewKey(SelectedUnit.PathAgent);
        MoveAction.StopPreview(previewKey);
        //move range preview
        if (SelectedUnit.MovesAvalaible > 0 && !moduleActived && !SelectedUnit.PathAgent.IsMoving)
        {
            List<IsoGridCoord> range = new List<IsoGridCoord>(m_moveRange);
            if (range.Contains(coord))
            {
                var agent = SelectedUnit.PathAgent;
                m_unitPathFound = IsoGridPathFinding.FindPathAstar(agent.Coordinate, coord, GridManager.ActivePathGrid, agent.BlockingMask, out var path);
                if (m_unitPathFound)
                {
                    BattleUIController.ShowPathTrail(path);

                    var moveAction = SelectedUnit.Move(LocamotionType.Instant, coord, true, false);
                    moveAction.StartPreview();
                    ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemyAttack).UpdateActionPreview();
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


            SelectedUnit.SetDirection(SelectedUnit.PathAgent.Coordinate.DirectionTo(m_pointerGridCoord, GridManager.ActivePathGrid));
            var param = new ActionModuleParam(SelectedUnit, new IsoGridCoord[] { m_pointerGridCoord }, false);
            module.GeneratePreview(param);
            m_actionRange = module.ActionRange(SelectedUnit.PathAgent.Coordinate);
            BattleUIController.HighlightActionRange(m_actionRange, "ActionRange");
            IsoGridCoord[] confirmed = module.ConfirmActionTargets();
            if (confirmed.Length > 0)
            {
                BattleUIController.ActionPreviewer.InitPreview();
                //module.GeneratePreview(param);
                BattleUIController.CursorController.SetCursor("TargetValid");
                module.PreviewKey = PreviewKey.GlobalKey;
                module.StartPreview();
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
        IsoGridCoord[] confirmed = action_module.ConfirmActionTargets();
        if (confirmed.Length > 0)
        {
            action_module.StopPreview();
            BattleUIController.ActionPreviewer.InitPreview();
            action_module.PreviewKey = new PreviewKey(action_module);
            action_module.StartPreview();
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
                action_module.StartPreview();
                action_module.StopPreview();
                BattleUIController.DisposeActionHighlights();
            }
        }
    }


    private void ModuleActionHandler(ActionModule action_module, UnitBase action_unit, IsoGridCoord[] input_coords)
    {
        var param = new ActionModuleParam(action_unit, input_coords, false);
        action_module.Build(param);
        var confirmed = action_module.ConfirmActionTargets();
        if (action_unit.CompareTag("PlayerUnit"))
        {
            if (confirmed.Length > 0)
            {
                action_module.Actived = false;
                action_module.IsAvailable = false;
                action_module.GeneratePreview(param);
                action_module.ExcuteAction();
                StopActionPreview(action_module);
                BattleUIController.DisposeActionHighlights();
            }
        }
    }



    private void StopActionPreview(ActionModule actionModule)
    {
        actionModule.StopPreview();
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

        if (SelectedUnit != null)
        {
            var previewKey = new PreviewKey(SelectedUnit.PathAgent);
            MoveAction.StopPreview(previewKey);
        }


        ActionModule activedModule = null;
        bool moduleActived = SelectedUnit == null ? false : SelectedUnit.ActivedModule(out activedModule);
        bool selectCondition = LevelManager.TryGetUnits(m_pointerGridCoord, out var units) && (!moduleActived);

        if (selectCondition)
        {
            SelectedUnit = units[0];
        }
        else if (SelectedUnit.CompareTag("PlayerUnit"))
        {
            if (!moduleActived && m_unitPathFound)
            {
                BattleUIController.DisposeMoveHighlights();
                var action = SelectedUnit.Move(LocamotionType.Default, m_pointerGridCoord,false);
                _ = action.ExcuteAction();
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

    private void OnRightClick(InputAction.CallbackContext obj)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit = null;
        }
    }

    #endregion
}
