using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    public static IsoGrid ActiveGrid { get; set; }

}
