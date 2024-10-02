using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ShiftMoveLocamotion : LocamotionBase
{
    [SerializeField] private float m_speed;

    private float m_distance;
    private float m_stopDistanceTemp;

    private IEnumerator LocamotionMoveCoroutine(float3 end, float stopping_dist = 0)
    {
        if(Transform == null)
            yield break;
        m_distance = math.distance(Transform.position, end);
        m_stopDistanceTemp = stopping_dist == 0 ? m_stopDistance : stopping_dist;
        var dir = ((Vector3)end - Transform.position).normalized;

        while (m_distance >= m_stopDistanceTemp)
        {
            Transform.position += dir * m_speed * Time.deltaTime;
            m_distance = math.distance(Transform.position, end);
            yield return null;
        }
        yield return null;
    }

    public override IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0)
    {
        var grid = GridManager.ActivePathGrid;
        var endPos = end.ToWorldPosition(grid);
        StartCoroutine(LocamotionMoveCoroutine(endPos,stopping_dist));
        yield return new WaitUntil(()=>{
            return m_distance<m_stopDistanceTemp;
        });
    }

    public override IEnumerator StartLocamotion(float3 end, float speed_override)
    {
        StartCoroutine(LocamotionMoveCoroutineSpeed(end,speed_override));
        yield return new WaitUntil(()=>{
            return m_distance<m_stopDistance;
        });
    }

    private IEnumerator LocamotionMoveCoroutineSpeed(float3 end, float speed_override)
    {
        if (Transform == null)
            yield break;
        m_distance = math.distance(Transform.position, end);
        var dir = ((Vector3)end - Transform.position).normalized;
        var speed = speed_override == 0 ? m_speed : speed_override;
        while (m_distance > m_stopDistance)
        {
            Transform.position += dir * speed * Time.deltaTime;
            m_distance = math.distance(Transform.position, end);
            yield return null;
        }
        yield return null;
    }

}
