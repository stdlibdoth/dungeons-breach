using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : Singleton<BattleUIController>
{

    [Header("Turn Control")]
    [SerializeField] private Button m_endPlayerTurnBtn;
    [SerializeField] private Button m_undoMovementBtn;
    [SerializeField] private Button m_resetTurnBtn;


    [SerializeField] private TileHighlighterFactory m_tileHighlighterFactory;


    private ActionModule[] m_selectedActionModules = new ActionModule[0];

    private UnitBase m_prevSelection;


    private TileHighlight m_pathHighlight;

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


    public static void HighlightPathRange(IsoGridCoord[] coords, string highliter_name)
    {
        var singleton = GetSingleton();
        singleton.m_pathHighlight?.Release();
        singleton.m_pathHighlight = new TileHighlight(singleton.m_tileHighlighterFactory,coords,highliter_name);
    }

    public static void DisposeRangeHighlights()
    {
        GetSingleton().m_pathHighlight?.Release();
    }


    #region Turn Control buttons
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
