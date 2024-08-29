using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewManager : Singleton<PreviewManager>
{
    [SerializeField] private Toggle m_attackToggle;

    private void Start()
    {
        m_attackToggle.onValueChanged.AddListener(AttackToggle);
    }


    private void AttackToggle(bool value)
    {
        BattleManager.SelectedUnit.IsAttackingMode = value;
    }
}
