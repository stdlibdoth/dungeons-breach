using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Prajectile formular for 2d coordinate system
/// x = v0_x*t
/// y = v0_y*t - 0.5*g*t*t
/// </summary>


public class TrajectoryLocamotion : LocamotionBase
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private string m_animateTrigger;
    [SerializeField] private Transform m_shadow;

    [Space]
    [Header("Trojectory")]
    [SerializeField] private float m_animationLength;
    [SerializeField] private int m_animationCycle;
    [SerializeField] private float m_gravity;


    private IEnumerator MoveShadow(float t, Vector3 start, Vector3 end, float stop_distance)
    {
        if (m_shadow == null)
            yield break;
        Transform shadow = Instantiate(m_shadow, m_shadow.position,Quaternion.identity);
        shadow.gameObject.SetActive(true);

        var dist = math.distance(start, end);
        var dir = (end - start).normalized;
        var stopDist = stop_distance == 0 ? m_stopDistance : stop_distance;
        var speed = dist / t;
        while (dist > stopDist)
        {
            shadow.position += dir * speed * Time.deltaTime;
            dist = math.distance(shadow.position, end);
            yield return null;
        }
        Destroy(shadow.gameObject);
    }


    public override IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0)
    {
        var endWorldPos = end.ToWorldPosition(GridManager.ActivePathGrid);
        var startWorldPos = start.ToWorldPosition(GridManager.ActivePathGrid);

        float x0 = endWorldPos.x - startWorldPos.x;
        float y0 = endWorldPos.y - startWorldPos.y;


        m_animator.SetTrigger(m_animateTrigger);
        yield return new WaitUntil(() => m_animator.GetCurrentAnimatorStateInfo(0).IsName(m_animateTrigger));


        float tMax = m_animationLength * m_animationCycle;
        StartCoroutine(MoveShadow(tMax, startWorldPos, endWorldPos, 0.05f));
        Trajectory2D trajectory = new Trajectory2D(new float2(x0, y0), m_gravity, tMax);
        float t = 0;
        while (t < tMax)
        {
            t += Time.deltaTime;
            float2 pos = trajectory.OutputPointWithTime(t);
            Transform.position = new Vector3(pos.x, pos.y) + (Vector3)startWorldPos;
            yield return null;
        }
        yield return null;
    }

    public override IEnumerator StartLocamotion(float3 end, float stopping_dist = 0)
    {
        var startWorldPos = Transform.position;

        float x0 = end.x - startWorldPos.x;
        float y0 = end.y - startWorldPos.y;


        m_animator.SetTrigger(m_animateTrigger);
        yield return new WaitForEndOfFrame();
        var stopDist = stopping_dist == 0 ? m_stopDistance : stopping_dist;



        float tMax = m_animator.GetCurrentAnimatorStateInfo(0).length * m_animationCycle;
        Trajectory2D trajectory = new Trajectory2D(new float2(x0, y0), m_gravity, tMax);
        float t = 0;
        float distance = Vector3.Distance(Transform.position, end);
        while (t < tMax && distance > stopDist)
        {
            t += Time.fixedDeltaTime;
            float2 pos = trajectory.OutputPointWithTime(t);
            Transform.position = new Vector3(pos.x, pos.y) + startWorldPos;
            yield return null;
        }
        yield return null;
    }
}
