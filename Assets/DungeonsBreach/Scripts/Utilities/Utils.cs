using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;


public static class Utils
{
    public const float animDirBlendMax = 3;


    public static float IsoDirToAnimBlend(IsoGridDirection dir)
    {
        return (int)dir;
    }

    public static IsoGridCoord AlongUnitDirection(IsoGridCoord coord, IsoGridDirection grid_dir,UnitDirection unit_dir)
    {
        int index = ((int)grid_dir + (int)unit_dir) % IsoGridMetrics.directionCount;
        return coord + IsoGridMetrics.GridDirectionToCoord[index];
    }
}
