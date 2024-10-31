using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InstantMoveLocamotion : LocamotionBase
{
    [SerializeField] private Animator m_animator;

    public override IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0)
    {
        if (Transform == null)
            yield break;

        m_animator.SetTrigger("Idle");
        m_animator.SetFloat("DirBlend", (int)Direction);
        var grid = GridManager.ActivePathGrid;
        Transform.position = end.ToWorldPosition(grid);
        yield return null;
    }

    public override IEnumerator StartLocamotion(float3 end,float speed_override)
    {
        Transform.position = end;
        yield return null;
    }
}
