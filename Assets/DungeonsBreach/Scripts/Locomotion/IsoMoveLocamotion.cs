using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public class IsoMoveLocamotion : MonoBehaviour,ILocamotion
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_stopDistance;

    public IsoGridDirection Direction { get; set; }


    private IEnumerator LocaMotionMoveCoroutine(float3 end)
    {
        var dist = math.distance(transform.position, end);
        var dir = (Vector3)end - transform.position;
        while (dist > m_stopDistance)
        {
            transform.position += dir * m_speed * Time.deltaTime;
            dist = math.distance(transform.position, end);
            yield return null;
        }

        m_animator.SetTrigger("Idle");
    }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end)
    {
        Direction = (end - start).CoordToDirection();
        m_animator.SetFloat("DirBlend", (int)Direction);
        m_animator.SetTrigger("Walk");

        var grid = GridManager.ActiveGrid;
        var endPos = end.ToWorldPosition(grid);
        yield return StartCoroutine(LocaMotionMoveCoroutine(endPos));
    }
}
