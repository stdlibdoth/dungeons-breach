using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantMoveLocamotion : MonoBehaviour, ILocamotion
{
    [SerializeField] private Animator m_animator;
    public IsoGridDirection Direction { get; set; }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end)
    {
        Direction = (end - start).CoordToDirection();
        m_animator.SetTrigger("Idle");
        m_animator.SetFloat("DirBlend", (int)Direction);
        var grid = GridManager.ActiveGrid;
        transform.position = end.ToWorldPosition(grid);
        yield return null;
    }
}
