using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public class IsoMoveLocamotion : MonoBehaviour,ILocamotion
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_stopDistance;
    [SerializeField] private LocamotionType m_type;

    public IsoGridDirection Direction { get; set; }

    public Transform Transform { get; set; }
    public LocamotionType Type
    {
        get { return m_type; }
        set { m_type = value; }
    }
    private IEnumerator LocaMotionMoveCoroutine(float3 end)
    {
        if(Transform == null)
            yield break;
        var dist = math.distance(Transform.position, end);
        var dir = ((Vector3)end - Transform.position).normalized;
        while (dist > m_stopDistance)
        {
            Transform.position += dir * m_speed * Time.deltaTime;
            dist = math.distance(Transform.position, end);
            yield return null;
        }

        m_animator.SetTrigger("Idle");
    }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end)
    {
        Direction = (end - start).CoordToDirection();
        m_animator.SetFloat("DirBlend", (int)Direction);
        m_animator.SetTrigger("Walk");

        var grid = GridManager.ActivePathGrid;
        var endPos = end.ToWorldPosition(grid);
        yield return StartCoroutine(LocaMotionMoveCoroutine(endPos));
    }
}
