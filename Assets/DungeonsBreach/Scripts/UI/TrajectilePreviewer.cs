using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class TrajectilePreviewer : ProjectilePreviewer
{

    [SerializeField] private TrajectileUnit m_trajectileUnit;
    [SerializeField] private int m_numOfSamples;
    private const int m_indexOffset = 1;

    public override void SetPreviewer(Vector3 start, Vector3 end, bool is_player,PreviewKey preview_key)
    {
        float[] xPos = new float[m_numOfSamples + 1];
        float increment = (end.x-start.x)/m_numOfSamples;
        for (int i = 0; i < xPos.Length; i++)
        {
            xPos[i] = i*increment;
        }
        var trajectory = m_trajectileUnit.GetTrajectory(start, end);
        var relativePos = trajectory.OutputSequenceWithX(xPos);
        var worldPos = new Vector3[xPos.Length];
        string highlighterKey = is_player?m_highlighterKeyPlayer:m_highlighterKeyMonster;

        int startIndex = m_indexOffset<m_numOfSamples?m_indexOffset:m_numOfSamples;
        for (int i = startIndex; i < relativePos.Length-1; i++)
        {
            worldPos[i] = start + (Vector3)(Vector2)relativePos[i];
            BattleUIController.ActionPreviewer.RegistorPreview(highlighterKey,worldPos[i],preview_key);
        }
    }
}