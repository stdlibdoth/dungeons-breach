using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ShiftMoveLocamotion : MonoBehaviour,ILocamotion
{
    [SerializeField] private float m_speed;
    [SerializeField] private float m_stopDistance;

    public IsoGridDirection Direction { get; set; }


    private IEnumerator LocaMotionMoveCoroutine(float3 end)
    {
        var dist = math.distance(transform.position, end);
        var dir = ((Vector3)end - transform.position).normalized;
        while (dist > m_stopDistance)
        {
            transform.position += dir * m_speed * Time.deltaTime;
            dist = math.distance(transform.position, end);
            yield return null;
        }
    }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end)
    {
        var grid = GridManager.ActiveGrid;
        var endPos = end.ToWorldPosition(grid);
        yield return StartCoroutine(LocaMotionMoveCoroutine(endPos));
    }
}
