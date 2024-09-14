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



    [SerializeField] private ActionBlackboard m_startTurnBoard;
    [SerializeField] private ActionBlackboard m_endTurnBoard;
    private static Queue<IAction> m_tempActions = new Queue<IAction>();

    private static int m_roundCount;

    private UnityEvent m_onStartPlayerTurn = new UnityEvent();


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
            if(GetSingleton().m_selectedUnit != value)
            {
                //deactive module
                if(GetSingleton().m_selectedUnit!= null)
                {
                    if(GetSingleton().m_selectedUnit.ActivedModule(out var module))
                        module.Actived = false;
                }

                //show move range
                GetSingleton().m_selectedUnit = value;
                BattleUIController.DisposeMoveHighlights();      
                if(value!= null && value.MovesAvalaible>0)
                {
                    GetSingleton().m_moveRange = value.Agent.ReachableCoordinates(value.MovesAvalaible,GridManager.ActivePathGrid);
                    BattleUIController.HighlightPathRange(GetSingleton().m_moveRange, "MoveRange");
                }
                else if(value == null)
                {

                    BattleUIController.DisposeActionHighlights();
                    BattleUIController.HidePathTrail();
                }
                EventManager.GetTheme<UnitTheme>("UnitTheme").GetTopic("SelectedUnitChange").Invoke(value);
            }
        }
    }

    public static UnityEvent OnStartPlayerTurn
    {
        get { return GetSingleton().m_onStartPlayerTurn; }
    }


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
        StartTurn();
    }


    public static int RoundCount
    {
        get { return m_roundCount; }
    }

    public static void RegistorAction(IAction action, PlayBackMode mode)
    {
        switch (mode)
        {
            case PlayBackMode.Instant:
                GetSingleton().StartCoroutine(action.ExcuteAction());
                break;
            case PlayBackMode.Temp:
                m_tempActions.Enqueue(action);
                break;
            case PlayBackMode.EndOfTurn:
                GetSingleton().m_endTurnBoard.AddAction(action);
                break;
            default:
                break;
        }
    }

    public static IEnumerator ExcuteTempActions()
    {
        var singlton = GetSingleton();
        while (m_tempActions.TryDequeue(out var action))
        {
            yield return singlton.StartCoroutine(action.ExcuteAction());
        }
    }


    public static void StartTurn()
    {
        foreach (var unit in LevelManager.Units)
        {
            var status = UnitStatus.Empty;
            unit.UpdateStatus(status);
            unit.ResetActions();
        }

        GetSingleton().m_onStartPlayerTurn.Invoke();
    }


    public static IEnumerator EndPlayerTurn()
    {
        var singleton = GetSingleton();
        SelectedUnit = null;
        yield return singleton.StartCoroutine(GetSingleton().m_endTurnBoard.ExcuteActions());
        BattleUIController.ClearAllActionTarget();
    }

    #region Events

    private void OnPointerCoordChange(IsoGridCoord coord)
    {
        if (SelectedUnit == null)
            return;

        m_unitPathFound = false;
        bool moduleActived = SelectedUnit.ActivedModule(out var module);
        if (SelectedUnit.MovesAvalaible > 0 && !moduleActived && !SelectedUnit.Agent.IsMoving)
        {
            List<IsoGridCoord> range = new List<IsoGridCoord>(m_moveRange);
            if (range.Contains(coord))
            {
                var agent = SelectedUnit.Agent;
                m_unitPathFound = IsoGridPathFinding.FindPathAstar(agent.Coordinate, coord, GridManager.ActivePathGrid, agent.BlockingMask, out var path);
                if(m_unitPathFound)
                    BattleUIController.ShowPathTrail(path);
                else
                    BattleUIController.HidePathTrail();
            }
        }
        else if (moduleActived && module.IsAvailable)
        {
            SelectedUnit.SetDirection(SelectedUnit.Agent.Coordinate.DirectionTo(m_pointerGridCoord, GridManager.ActivePathGrid));
            m_actionRange = module.ActionRange(SelectedUnit.Agent.Coordinate,SelectedUnit.Agent.Direction);
            BattleUIController.DisposeMoveHighlights();
            BattleUIController.HighlightActionRange(m_actionRange,"ActionRange");
        }
    }

    #endregion

    #region Helpers

    private void ModuleActionHandler(ActionModule actionModule)
    {
        Debug.Log("Module Action: " + actionModule.ModuleName);
        if (SelectedUnit.CompareTag("PlayerUnit"))
        {
            IsoGridCoord[] confirmed = ConfirmActionRange(actionModule);
            if (confirmed.Length > 0)
            {
                SelectedUnit.ModuleAction(actionModule.ModuleName, confirmed, PlayBackMode.Instant);
                BattleUIController.DisposeActionHighlights();
            }
        }
        else if (SelectedUnit.CompareTag("MonsterUnit"))
        {
            IsoGridCoord[] confirmed = ConfirmActionRange(actionModule);
            if (confirmed.Length > 0)
            {
                SelectedUnit.ModuleAction(actionModule.ModuleName, confirmed, PlayBackMode.EndOfTurn);
                BattleUIController.DisposeActionHighlights();
                BattleUIController.ShowActionTarget(SelectedUnit,confirmed);
            }
        }
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


        if(m_pointerGridCoord != coord)
        {
            if(checkRange)
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
                SelectedUnit.Move(LocamotionType.Default, m_pointerGridCoord, PlayBackMode.Instant);
                BattleUIController.StartPathTrailing(SelectedUnit);
            }
            else if (moduleActived && activedModule.IsAvailable)
            {
                ModuleActionHandler(activedModule);
            }
        }
    }

    private IsoGridCoord[] ConfirmActionRange(ActionModule module)
    {    
        List<IsoGridCoord> range = new List<IsoGridCoord>();
        foreach (var coord in m_actionRange)
        {
            if (coord == m_pointerGridCoord)
                range.Add(coord);
        }
        return range.ToArray();
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
