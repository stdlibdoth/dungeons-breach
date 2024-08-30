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
    private static List<IAction> m_tempActions;

    private static int m_roundCount;

    private UnityEvent m_onStartPlayerTurn;


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
                GetSingleton().m_endTurnBoard.AddAction(action);
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


    private void StartTurn()
    {



        m_onStartPlayerTurn.Invoke();
    }


    public static IEnumerator EndPlayerTurn()
    {
        var singleton = GetSingleton();

        yield return singleton.StartCoroutine(GetSingleton().m_endTurnBoard.ExcuteActions());

        //foreach (var u in LevelManager.Units)
        //{
        //    if(u.CompareTag(singleton.m_playerTag))
        //    {
        //        u.tag = singleton.m_monsterTag;
        //    }
        //    else if(u.CompareTag(singleton.m_monsterTag))
        //    {
        //        u.tag = singleton.m_playerTag;
        //    }
        //}
    }



    //public static void BattleStart()
    //{
    //    m_roundCount = 0;
    //    m_startTurnBoard = new ActionBlackboard();
    //    m_endTurnBoard = new ActionBlackboard();
    //}



    #region UI
    private void DisplayTileInfo()
    {

    }

    #endregion

    #region Helpers

    #endregion


    #region Events

    //private void OnSelectedUnitChange(UnitBase unit)
    //{
    //    Debug.Log(unit + " Selected");
    //}

    private void OnPointerCoordChange(IsoGridCoord coord)
    {
        if (SelectedUnit == null)
            return;

        m_unitPathFound = false;
        if (SelectedUnit.MoveRange > 0 && !SelectedUnit.IsAttackingMode && !SelectedUnit.Agent.IsMoving)
        {
            var dist = IsoGridCoord.Distance(SelectedUnit.Agent.Coordinate, coord);
            if (GridManager.ActivePathGrid.CheckRange(coord) && dist <= SelectedUnit.MoveRange && dist > 0)
            {
                var agent = SelectedUnit.Agent;
                m_unitPathFound = IsoGridPathFinding.FindPathAstar(agent.Coordinate, coord, GridManager.ActivePathGrid, agent.BlockingMask, out var path);
            }
        }
        else if (SelectedUnit.IsAttackingMode)
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


        bool selectCondition = LevelManager.TryGetUnit(m_pointerGridCoord, out var unit) && ((SelectedUnit == null) ||
            !SelectedUnit.IsAttackingMode);

        if (selectCondition)
        {
            //if (unit.CompareTag("PlayerUnit"))
                SelectedUnit = unit;
            //show unit info only
            //else
            //{

            //}
        }
        else
        {
            if (!SelectedUnit.IsAttackingMode && m_unitPathFound)
            {
                SelectedUnit.Move(LocamotionType.Default, m_pointerGridCoord, PlayBackMode.Instant, !m_testMode);
            }
            else if (SelectedUnit.IsAttackingMode)
            {
                if (SelectedUnit.CompareTag("PlayerUnit"))
                {
                    SelectedUnit.Attack(PlayBackMode.Instant);
                    SelectedUnit.ResetActions();
                }
                else if(SelectedUnit.CompareTag("MonsterUnit"))
                {
                    SelectedUnit.Attack(PlayBackMode.EndOfTurn);
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
