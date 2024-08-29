using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantMoveLocamotion : MonoBehaviour, ILocamotion
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private LocamotionType m_type;
    public IsoGridDirection Direction { get; set; }
    public Transform Transform {  get; set; }
    public LocamotionType Type
    {
        get { return m_type; }
        set { m_type = value; }
    }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end)
    {
        if (Transform == null)
            yield break;

        Direction = (end - start).CoordToDirection();
        m_animator.SetTrigger("Idle");
        m_animator.SetFloat("DirBlend", (int)Direction);
        var grid = GridManager.ActivePathGrid;
        Transform.position = end.ToWorldPosition(grid);
        yield return null;
    }
}
