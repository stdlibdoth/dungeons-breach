using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;


public class BattleManager : Singleton<BattleManager>
{
    [SerializeField] private bool m_testMode;


    private static ActionBlackboard m_startTurnBoard;
    private static ActionBlackboard m_endTurnBoard;
    private static List<IAction> m_tempActions;

    private static int m_roundCount;


    private DefaultInputActions m_inputActions;
    private IsoGridCoord m_pointerGridCoord;

    private UnitBase m_selectedUnit;
    private UnityEvent<UnitBase> m_onSelectionChange;
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
                GetSingleton().m_onSelectionChange.Invoke(value);
            }
        }
    }


    protected override void Awake()
    {
        base.Awake();
        m_inputActions = new DefaultInputActions();
        m_onSelectionChange = new UnityEvent<UnitBase>();
        m_onPointerCoordChange = new UnityEvent<IsoGridCoord>();

        m_onSelectionChange.AddListener(OnSelectedUnitChange);
        m_onPointerCoordChange.AddListener(OnPointerCoordChange);
    }

    private void Start()
    {
        m_inputActions.UI.Enable();
        m_inputActions.UI.Point.performed += OnPointerPoint;
        m_inputActions.UI.Click.performed += OnClick;
        m_inputActions.UI.RightClick.performed += OnRightClick;
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
                m_tempActions.Add(action);
                break;
            case PlayBackMode.EndOfTurn:
                m_endTurnBoard?.AddAction(action);
                break;
            default:
                break;
        }
    }

    public static IEnumerator ExcuteTempActions()
    {
        var singlton = GetSingleton();
        foreach (var action in m_tempActions)
        {
            yield return singlton.StartCoroutine(action.ExcuteAction());
        }
    }

    public static void BattleStart()
    {
        m_roundCount = 0;
        m_startTurnBoard = new ActionBlackboard();
        m_endTurnBoard = new ActionBlackboard();
    }



    #region UI
    private void DisplayTileInfo()
    {

    }

    #endregion


    #region Events

    private void OnSelectedUnitChange(UnitBase unit)
    {
        Debug.Log(unit + " Selected");
    }

    private void OnPointerCoordChange(IsoGridCoord coord)
    {
        if (SelectedUnit != null)
        {
            m_unitPathFound = false;
            if (SelectedUnit.MoveRange > 0 && !SelectedUnit.IsAttackingMode && !SelectedUnit.Agent.IsMoving)
            {
                var dist = IsoGridCoord.Distance(SelectedUnit.Agent.Coordinate, coord);
                if (GridManager.ActivePathGrid.CheckRange(coord) && dist <= SelectedUnit.MoveRange && dist>0)
                {
                    var agent = SelectedUnit.Agent;
                    m_unitPathFound = IsoGridPathFinding.FindPathAstar(agent.Coordinate, coord, GridManager.ActivePathGrid, agent.BlockingMask, out var path);
                    //if(m_unitPathFound)
                    //{
                    //    Debug.Log(coord);
                    //}
                }
            }
            else if(SelectedUnit.IsAttackingMode)
            {
                var surrounding = GridManager.ActivePathGrid.SurroundingCoords(SelectedUnit.Agent.Coordinate);

                //find the turn
                if(m_pointerGridCoord!= SelectedUnit.Agent.Coordinate)
                {
                    int index = 0;
                    var min = int.MaxValue;
                    for (int i = 0; i < surrounding.Length; i++)
                    {
                        if (surrounding[i].x < 0)
                            continue;
                        int dist = IsoGridCoord.Distance(surrounding[i], m_pointerGridCoord);
                        if (dist<min)
                        {
                            min = dist;
                            index = i;
                        }
                    }
                    SelectedUnit.SetDirection((IsoGridDirection)index);
                }
               
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
        if(m_pointerGridCoord != coord)
        {
            m_pointerGridCoord = coord;
            m_onPointerCoordChange.Invoke(coord);
        }
    }

    private void OnClick(InputAction.CallbackContext obj)
    {
        var grid = GridManager.ActiveTileGrid;
        if (grid.CheckRange(m_pointerGridCoord))
        {
            if(SelectedUnit == null && LevelManager.TryGetUnit(m_pointerGridCoord, out var unit))
            {
                if(unit is PlayerUnit)
                    SelectedUnit = unit;
                
                //show unit info only
                else
                {

                }
            }
            else if(SelectedUnit != null)
            {
                if (!SelectedUnit.IsAttackingMode) //&& SelectedUnit.MoveRange > 0 && !SelectedUnit.IsAttacking && !SelectedUnit.Agent.IsMoving)
                {
                    if(m_unitPathFound)
                        SelectedUnit.Move(LocamotionType.Default, m_pointerGridCoord, PlayBackMode.Instant,!m_testMode);
                }
                else if(SelectedUnit.IsAttackingMode)
                {
                    SelectedUnit.Attack(PlayBackMode.Instant);
                    SelectedUnit.ResetActions();
                }
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
