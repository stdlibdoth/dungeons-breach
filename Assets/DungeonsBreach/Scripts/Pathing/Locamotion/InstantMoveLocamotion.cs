using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class InstantMoveLocamotion : LocamotionBase
{
    [SerializeField] private Animator m_animator;

    public override async UniTask StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0)
    {
        if (Transform == null)
            return;

        m_animator.SetTrigger("Idle");
        m_animator.SetFloat("DirBlend", (int)Direction);
        var grid = GridManager.ActivePathGrid;
        Transform.position = end.ToWorldPosition(grid);
        await UniTask.Yield();
    }

    public override async UniTask StartLocamotion(float3 end,float speed_override)
    {
        Transform.position = end;
        await UniTask.Yield();
    }
}
