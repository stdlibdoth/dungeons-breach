using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;


public enum LocamotionType
{
    Default,
    Instant,
    Shift,
    Trajectory
}


public interface ILocamotion
{
    public IsoGridDirection Direction { get; set; }
    public Transform Transform {  get; set; }
    public LocamotionType Type { get; set; }

    public UniTask StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0);

    public UniTask StartLocamotion(float3 end, float speed_override = 0);
}
