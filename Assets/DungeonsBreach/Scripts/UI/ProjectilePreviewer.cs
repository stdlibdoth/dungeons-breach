using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePreviewer : MonoBehaviour
{
    //[SerializeField] protected LineRenderer m_lineRenderer;
    [SerializeField] protected Material m_enemyMat;
    [SerializeField] protected Material m_playerMat;

    [Tooltip("In terms of grid's cell size")]
    [SerializeField] private float m_interval;
    [SerializeField] protected string m_highlighterKeyPlayer;
    [SerializeField] protected string m_highlighterKeyMonster;



    public virtual void SetPreviewer(Vector3 start, Vector3 end, bool is_player, PreviewKey preview_key)
    {
        float cellSize = GridManager.ActiveTileGrid.CellSize;
        float interval = m_interval*cellSize;
        float offset = 0.5f*interval;
        int highlightCount = (int)(Vector3.Distance(start,end)/interval);
        Vector3 dir = (end -start).normalized;
        string highlighterKey = is_player? m_highlighterKeyPlayer:m_highlighterKeyMonster;
        for (int i = 0; i < highlightCount; i++)
        {
            var position = start + offset*dir + i*dir*interval;
            BattleUIController.ActionPreviewer.RegistorPreview(highlighterKey,position,preview_key);
        }
    }

}
