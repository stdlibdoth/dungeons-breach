using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocamotion
{
    public IsoGridDirection Direction { get; set; }
    public Transform Transform {  get; set; }
    public LocamotionType Type { get; set; }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end);
}
