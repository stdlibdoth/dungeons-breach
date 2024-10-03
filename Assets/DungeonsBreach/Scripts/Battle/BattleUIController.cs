using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : Singleton<BattleUIController>
{
    [SerializeField] private TileHighlighter m_pointerHighlight;
    [Header("Turn Control")]
    [SerializeField] private Button m_endPlayerTurnBtn;
    [SerializeField] private Button m_undoMovementBtn;
    [SerializeField] private Button m_resetTurnBtn;



    [Header("Refs")]
    [SerializeField] private ActionPreviewer m_previewer;
    [SerializeField] private CursorController m_cursorController;
    [SerializeField] private PathTrailer m_pathTrailer;
    [SerializeField] private TileHighlighterFactory m_tileHighlighterFactory;


    private TileHighlight m_pathHL;
    private TileHighlight m_actionRangeHL;



    public static ActionPreviewer ActionPreviewer
    {
        get { return GetSingleton().m_previewer; }
    }

    public static CursorController CursorController
    {
        get{return GetSingleton().m_cursorController;}
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
        yield return BattleManager.EndPlayerTurn();
        m_endPlayerTurnBtn.interactable = true;
        yield return BattleManager.StartTurn();
    }

    #region Range Highlights

    public static void HighlightPathRange(IsoGridCoord[] coords, string highliter_name)
    {
        var singleton = GetSingleton();
        singleton.m_pathHL?.Release();
        singleton.m_pathHL = new TileHighlight(singleton.m_tileHighlighterFactory,coords,highliter_name);
    }

    public static void DisposeMoveHighlights()
    {
        GetSingleton().m_pathHL?.Release();
    }

    public static void DisposeActionHighlights()
    {
        GetSingleton().m_actionRangeHL?.Release();
    }

    public static void HighlightActionRange(IsoGridCoord[] coords, string highliter_name)
    {
        var singleton = GetSingleton();
        singleton.m_actionRangeHL?.Release();
        singleton.m_actionRangeHL = new TileHighlight(singleton.m_tileHighlighterFactory,coords,highliter_name);
    }
    
    #endregion

    #region  pointer
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

    #endregion


    #region  Path trailer
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
    #endregion


    #region action highlight
    public static void ShowActionTarget(ActionModule module, IsoGridCoord[] coords)
    {
        var singleton = GetSingleton();
        for (int i = 0; i < coords.Length; i++)
        {
            var data = new ActionPreviewerData("Target",IsoGridDirection.SE,coords[i]);
            ActionPreviewer.RegistorPreview(data,module.PreviewKey);
        }
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
