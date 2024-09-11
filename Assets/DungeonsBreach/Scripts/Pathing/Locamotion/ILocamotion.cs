using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public interface ILocamotion
{
    public IsoGridDirection Direction { get; set; }
    public Transform Transform {  get; set; }
    public LocamotionType Type { get; set; }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end, float stopping_dist = 0);

    public IEnumerator StartLocamotion(float3 end, float speed_override = 0);
}
