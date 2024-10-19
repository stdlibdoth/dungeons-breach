using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TurnInfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_text;
    [SerializeField] private GameObject m_titleGO;
    [SerializeField] private float m_titleFadeDelay;
    [SerializeField] private List<ActionTurnType> m_showTitleTurns;


    [Header("Themes")]
    [SerializeField] private TurnTheme m_turnTheme;

    private void Awake()
    {
        RegistorShowTitleTurns();
        m_turnTheme.GetTopic("ActionTurnStart").AddListener(OnActionTurnStart);
        m_turnTheme.GetTopic("ActionTurnEnd").AddListener(OnActionTurnEnd);
    }


    private void RegistorShowTitleTurns()
    {
        foreach (var item in m_showTitleTurns)
        {
            var turn = ActionTurn.CreateOrGetActionTurn(item);
            turn.SubOnTurnStart(ShowTitle);
        }
    }

    private IEnumerator ShowTitle(ActionTurn turn)
    {
        m_text.text = ActionTurnName.names[(int)turn.Type];
        m_titleGO.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(m_titleFadeDelay);
        
        m_titleGO.gameObject.SetActive(false);
    }


    #region Theme events
    private void OnActionTurnStart(ActionTurnType turn_type, ActionTurn turn)
    {
        if (turn_type == ActionTurnType.PlayerTurn)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }



    private void OnActionTurnEnd(ActionTurnType turn_type, ActionTurn turn)
    {
        if (turn_type == ActionTurnType.PlayerTurn)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    #endregion

}
