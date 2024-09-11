using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public class IsoMoveLocamotion : LocamotionBase
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private float m_speed;
    [SerializeField] private string m_animateTrigger;

    private IEnumerator LocaMotionMoveCoroutine(float3 end, float stopping_dist)
    {
        if(Transform == null)
            yield break;
        var dist = math.distance(Transform.position, end);
        var dir = ((Vector3)end - Transform.position).normalized;
        var stopDist = stopping_dist == 0 ? m_stopDistance : stopping_dist;
        while (dist > stopDist)
        {
            Transform.position += dir * m_speed * Time.deltaTime;
            dist = math.distance(Transform.position, end);
            yield return null;
        }

        m_animator.SetTrigger("Idle");
    }

    public override IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0)
    {
        Direction = (end - start).CoordToDirection();
        m_animator.SetFloat("DirBlend", (int)Direction);
        m_animator.SetTrigger(m_animateTrigger);

        var grid = GridManager.ActivePathGrid;
        var endPos = end.ToWorldPosition(grid);
        yield return StartCoroutine(LocaMotionMoveCoroutine(endPos, stopping_dist));
    }

    public override IEnumerator StartLocamotion(float3 end, float speed_override)
    {
        if (Transform == null)
            yield break;
        var dist = math.distance(Transform.position, end);
        var dir = ((Vector3)end - Transform.position).normalized;
        var speed = speed_override == 0? m_speed : speed_override;
        while (dist > m_stopDistance)
        {
            Transform.position += dir * speed * Time.deltaTime;
            dist = math.distance(Transform.position, end);
            yield return null;
        }
        m_animator.SetTrigger("Idle");
    }
}
