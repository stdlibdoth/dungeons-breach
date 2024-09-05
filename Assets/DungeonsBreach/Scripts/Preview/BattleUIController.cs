using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : Singleton<BattleUIController>
{
    [SerializeField] private Toggle m_attackToggle;
    [SerializeField] private Button m_endPlayerTurnBtn;


    private ActionModule[] m_selectedActionModules = new ActionModule[0];

    private UnitBase m_prevSelection;

    private void Start()
    {
        var theme = EventManager.GetTheme<UnitTheme>("UnitTheme");
        theme.GetTopic("SelectedUnitChange").AddListener(OnSelectedChange);
        m_attackToggle.onValueChanged.AddListener(ActionToggle);
        m_endPlayerTurnBtn.onClick.AddListener(() => StartCoroutine(EndTurnActions()));
    }

    private IEnumerator EndTurnActions()
    {
        m_endPlayerTurnBtn.interactable = false;
        yield return StartCoroutine(BattleManager.EndPlayerTurn());
        m_endPlayerTurnBtn.interactable = true;
        BattleManager.StartTurn();
    }


    private void OnSelectedChange(UnitBase unit)
    {

        m_prevSelection?.OnUnitActionAvailable.RemoveListener(OnActionAvailable);
        m_prevSelection = unit;
        unit?.OnUnitActionAvailable.AddListener(OnActionAvailable);
        m_attackToggle.SetIsOnWithoutNotify(false);

        if(unit == null || !unit.ActionAvailable)
        {
            m_attackToggle.interactable = false;
            return;
        }
        m_attackToggle.interactable = true;
        m_selectedActionModules = new ActionModule[0];
        if (unit != null)
        {
            m_selectedActionModules = unit.ActionModules();
            foreach (var module in m_selectedActionModules)
                module.Actived = false;
        }
    }



    private void OnBattleStart()
    {

    }

    private void OnActionAvailable(bool available)
    {
        if (BattleManager.SelectedUnit == null)
            return;

        m_attackToggle.interactable = available;
        if (!available)
        {
            m_attackToggle.SetIsOnWithoutNotify(false);
        }
            
    }



    private void ActionToggle(bool value)
    {
        if(m_selectedActionModules.Length>0)
            m_selectedActionModules[0].Actived = value;
    }
}
