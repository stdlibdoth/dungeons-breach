using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : Singleton<BattleUIController>
{
    [SerializeField] private TileHighlighter m_pointerHighlight;
    [Header("Turn Control")]
    [SerializeField] private Button m_endPlayerTurnBtn;
    [SerializeField] private Button m_undoMovementBtn;
    [SerializeField] private Button m_resetTurnBtn;


    [SerializeField] private TileHighlighterFactory m_tileHighlighterFactory;


    [SerializeField] private PathTrailer m_pathTrailer;

    private ActionModule[] m_selectedActionModules = new ActionModule[0];

    private UnitBase m_prevSelection;


    private TileHighlight m_pathHighlight;
    private TileHighlight m_actionHighlight;


    private Dictionary<UnitBase,TileHighlight> m_actionTargetHighlights = new Dictionary<UnitBase, TileHighlight>();


    public static TileHighlighterFactory TileHighlighterFactory
    {
        get { return GetSingleton().m_tileHighlighterFactory; }
    }


    private void Start()
    {
        m_endPlayerTurnBtn.onClick.AddListener(() => StartCoroutine(EndTurnActions()));
        m_undoMovementBtn.onClick.AddListener(UndoMovementBtnPressed);
        m_resetTurnBtn.onClick.AddListener(ResetBtnPressed);
    }

    private IEnumerator EndTurnActions()
    {
        m_endPlayerTurnBtn.interactable = false;
        yield return StartCoroutine(BattleManager.EndPlayerTurn());
        m_endPlayerTurnBtn.interactable = true;
        BattleManager.StartTurn();
    }

    #region public methods

    public static void HighlightPathRange(IsoGridCoord[] coords, string highliter_name)
    {
        var singleton = GetSingleton();
        singleton.m_pathHighlight?.Release();
        singleton.m_pathHighlight = new TileHighlight(singleton.m_tileHighlighterFactory,coords,highliter_name);
    }

    public static void DisposeMoveHighlights()
    {
        GetSingleton().m_pathHighlight?.Release();
    }

    public static void DisposeActionHighlights()
    {
        GetSingleton().m_actionHighlight?.Release();
    }

    public static void HighlightActionRange(IsoGridCoord[] coords, string highliter_name)
    {
        var singleton = GetSingleton();
        singleton.m_actionHighlight?.Release();
        singleton.m_actionHighlight = new TileHighlight(singleton.m_tileHighlighterFactory,coords,highliter_name);
    }
    

    public static void ShowPointerHighlight(IsoGridCoord coord)
    {
        GetSingleton().m_pointerHighlight.transform.position = coord.ToWorldPosition(GridManager.ActivePathGrid);
        GetSingleton().m_pointerHighlight.gameObject.SetActive(true);
    }

    public static void ShowPointerHighlight(Vector3 pos)
    {
        GetSingleton().m_pointerHighlight.transform.position = pos;
        GetSingleton().m_pointerHighlight.gameObject.SetActive(true);
    }

    public static void HidePointerHighlight()
    {
        GetSingleton().m_pointerHighlight.gameObject.SetActive(false);
    }


    public static void ShowPathTrail(List<IsoGridCoord> path)
    {
        GetSingleton().m_pathTrailer.Init(path);
    }

    public static void HidePathTrail()
    {
        GetSingleton().m_pathTrailer.HideTrail();
    }

    public static void StartPathTrailing(UnitBase unit)
    {
        GetSingleton().m_pathTrailer.StartTrailing(unit);
    }



    public static void ShowActionTarget(UnitBase unit, IsoGridCoord[] coords)
    {
        var singleton = GetSingleton();
        var factory = singleton.m_tileHighlighterFactory;
        if(singleton.m_actionTargetHighlights.ContainsKey(unit))
        {
            singleton.m_actionTargetHighlights[unit].Release();
        }
        singleton.m_actionTargetHighlights[unit] = new TileHighlight(factory,coords,"Target");
    }

    public static void ClearAllActionTarget()
    {
        var singleton = GetSingleton();
        foreach (var item in singleton.m_actionTargetHighlights)
        {
            item.Value.Release();
        }
        singleton.m_actionTargetHighlights.Clear();
    }

    #endregion

    #region Turn Control
    private void UndoMovementBtnPressed()
    {

    }


    private void ResetBtnPressed()
    {

    }
#endregion


    private void OnBattleStart()
    {

    }

}
