using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;

public class ShiftMoveLocamotion : LocamotionBase
{
    [SerializeField] private float m_speed;

    private float m_distance;
    private float m_stopDistanceTemp;

    private async UniTask LocamotionMoveCoroutine(float3 end, float stopping_dist = 0)
    {
        if(Transform == null)
            return;
        m_distance = math.distance(Transform.position, end);
        m_stopDistanceTemp = stopping_dist == 0 ? m_stopDistance : stopping_dist;
        var dir = ((Vector3)end - Transform.position).normalized;

        while (m_distance >= m_stopDistanceTemp)
        {
            Transform.position += dir * m_speed * Time.deltaTime;
            m_distance = math.distance(Transform.position, end);
            await UniTask.Yield();
        }
        await UniTask.Yield();
    }

    public override async UniTask StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0)
    {
        var grid = GridManager.ActivePathGrid;
        var endPos = end.ToWorldPosition(grid);
        await LocamotionMoveCoroutine(endPos,stopping_dist);
        await UniTask.WaitUntil(()=>{
            return m_distance<m_stopDistanceTemp;
        });
    }

    public override async UniTask StartLocamotion(float3 end, float speed_override)
    {
        await LocamotionMoveCoroutineSpeed(end, speed_override);
        await UniTask.WaitUntil(()=>{
            return m_distance<m_stopDistance;
        });
    }

    private async UniTask LocamotionMoveCoroutineSpeed(float3 end, float speed_override)
    {
        if (Transform == null)
            return;
        m_distance = math.distance(Transform.position, end);
        var dir = ((Vector3)end - Transform.position).normalized;
        var speed = speed_override == 0 ? m_speed : speed_override;
        while (m_distance > m_stopDistance)
        {
            Transform.position += dir * speed * Time.deltaTime;
            m_distance = math.distance(Transform.position, end);
            await UniTask.Yield();
        }
        await UniTask.Yield();
    }

}
