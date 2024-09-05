using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;


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

    bool m_unitPathFound;

    public static UnitBase SelectedUnit
    {
        get { return GetSingleton().m_selectedUnit; }
        private set
        {
            if(GetSingleton().m_selectedUnit != value)
            {
                GetSingleton().m_selectedUnit = value;
                Debug.Log(value + " Selected");
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

        yield return singleton.StartCoroutine(GetSingleton().m_endTurnBoard.ExcuteActions());
    }

    #region Helpers

    #endregion


    #region Events

    private void OnPointerCoordChange(IsoGridCoord coord)
    {
        if (SelectedUnit == null)
            return;

        m_unitPathFound = false;
        bool moduleActived = SelectedUnit.ActivedModule(out var module);
        if (SelectedUnit.MoveRange > 0 && !moduleActived && !SelectedUnit.Agent.IsMoving)
        {
            var dist = IsoGridCoord.Distance(SelectedUnit.Agent.Coordinate, coord);
            if (GridManager.ActivePathGrid.CheckRange(coord) && dist <= SelectedUnit.MoveRange && dist > 0)
            {
                var agent = SelectedUnit.Agent;
                m_unitPathFound = IsoGridPathFinding.FindPathAstar(agent.Coordinate, coord, GridManager.ActivePathGrid, agent.BlockingMask, out var path);
            }
        }
        else if (moduleActived && SelectedUnit.ActionAvailable)
        {
            SelectedUnit.SetDirection(SelectedUnit.Agent.Coordinate.DirectionTo(m_pointerGridCoord, GridManager.ActivePathGrid));
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
        if(m_pointerGridCoord != coord)
        {
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
                SelectedUnit.Move(LocamotionType.Default, m_pointerGridCoord, PlayBackMode.Instant);
            }
            else if (moduleActived && SelectedUnit.ActionAvailable)
            {
                Debug.Log("Module Action: " + activedModule.Id);
                if (SelectedUnit.CompareTag("PlayerUnit"))
                {
                    List<IsoGridCoord> confirmed = FindOverlapTilesWithModule(activedModule);
                    if (confirmed.Count > 0)
                        SelectedUnit.ModuleAction(activedModule.Id, confirmed.ToArray(), PlayBackMode.Instant);
                }
                else if (SelectedUnit.CompareTag("MonsterUnit"))
                {
                    List<IsoGridCoord> confirmed = FindOverlapTilesWithModule(activedModule);
                    if (confirmed.Count > 0)
                        SelectedUnit.ModuleAction(activedModule.Id, confirmed.ToArray(), PlayBackMode.EndOfTurn);
                }
            }
        }
    }

    private List<IsoGridCoord> FindOverlapTilesWithModule(ActionModule module)
    {
        List<IsoGridCoord> confirmed = new List<IsoGridCoord>();
        foreach (var tileInfo in module.ActionTileProfile.data)
        {
            IsoGridCoord coord = tileInfo.relativeCoord.OnRelativeTo(SelectedUnit.Agent.Coordinate, SelectedUnit.Agent.Direction);
            if (coord == m_pointerGridCoord)
                confirmed.Add(coord);
        }
        return confirmed;
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
