using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocamotion
{
    public IsoGridDirection Direction { get; set; }

    public IEnumerator StartLocamotion(IsoGridCoord start, IsoGridCoord end);
}
