using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class LocamotionBase : MonoBehaviour, ILocamotion
{
    [SerializeField] protected LocamotionType m_type;
    [SerializeField] protected float m_stopDistance;
    public IsoGridDirection Direction { get; set; }
    public Transform Transform { get; set; }
    public LocamotionType Type
    {
        get { return m_type; }
        set { m_type = value; }
    }

    public abstract UniTask StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0);

    public abstract UniTask StartLocamotion(float3 end, float speed_override = 0);
}
