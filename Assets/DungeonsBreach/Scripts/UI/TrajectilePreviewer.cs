using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectilePreviewer : ProjectilePreviewer
{
    [SerializeField] private int m_numOfSamples;
    [SerializeField] private TrajectileUnit m_trajectileUnit;

    public override void SetPreviewer(Vector3 start, Vector3 end, bool is_player)
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
        m_lineRenderer.positionCount = relativePos.Length;
        for (int i = 0; i < relativePos.Length; i++)
        {
            worldPos[i] = start + (Vector3)(Vector2)relativePos[i];
        }
        m_lineRenderer.SetPositions(worldPos);
        m_lineRenderer.material = is_player? m_playerMat:m_enemyMat;
        m_lineRenderer.enabled = true;
    }
}