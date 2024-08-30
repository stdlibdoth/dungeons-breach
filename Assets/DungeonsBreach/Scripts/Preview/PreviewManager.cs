using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewManager : Singleton<PreviewManager>
{
    [SerializeField] private Toggle m_attackToggle;
    [SerializeField] private Button m_endPlayerTurnBtn;


    private void Start()
    {
        var theme = EventManager.GetTheme<UnitTheme>("UnitTheme");
        theme.GetTopic("SelectedUnitChange").AddListener(OnSelectedChange);
        m_attackToggle.onValueChanged.AddListener(AttackToggle);
        m_endPlayerTurnBtn.onClick.AddListener(() => StartCoroutine(EndTurnActions()));
    }
    private IEnumerator EndTurnActions()
    {
        m_endPlayerTurnBtn.interactable = false;
        yield return StartCoroutine(BattleManager.EndPlayerTurn());
        m_endPlayerTurnBtn.interactable = true;
    }


    private void OnSelectedChange(UnitBase unit)
    {

        m_attackToggle.interactable = unit != null;
        if(unit != null)
        {
            unit.IsAttackingMode = false;
            m_attackToggle.SetIsOnWithoutNotify(false);
        }
    }


    private void AttackToggle(bool value)
    {
        BattleManager.SelectedUnit.IsAttackingMode = value;
    }
}
